using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Intel;
using Wpf.Ui.Mvvm.Interfaces;

namespace Universal_x86_Tuning_Utility_Handheld.Scripts.Adaptive
{
    internal class CPUControl
    {
        private const int PowerLimitIncrement = 2; // watts

        private static int _newPowerLimit = 999; // watts
        public static int _currentPowerLimit = 999; // watts
        private static int _lastPowerLimit = 1000; // watts
        public static int _lastUsage = 0; // %


        public static string cpuCommand = "";
        public static string coCommand = "";
        public static async void UpdatePowerLimit(int temperature, int cpuLoad, int MaxPowerLimit, int MinPowerLimit, int MaxTemperature)
        {
            try
            {
                if (temperature >= MaxTemperature - 2)
                {
                    // Reduce power limit if temperature is too high
                    _newPowerLimit = Math.Max(MinPowerLimit, _newPowerLimit - PowerLimitIncrement);
                }
                else if (cpuLoad > 10 && temperature <= (MaxTemperature - 5))
                {
                    // Increase power limit if temperature allows and CPU load is high
                    _newPowerLimit = Math.Min(MaxPowerLimit, _newPowerLimit + PowerLimitIncrement);
                }

                if (_newPowerLimit < MinPowerLimit) _newPowerLimit = MinPowerLimit;
                if (_newPowerLimit > MaxPowerLimit) _newPowerLimit = MaxPowerLimit;

                // Apply new power limit if power limit has changed
                if (_newPowerLimit <= _lastPowerLimit - 1 || _newPowerLimit >= _lastPowerLimit + 1)
                {
                    int _TDP = _newPowerLimit;

                    // Detect if AMD CPU or APU
                    if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                    {
                        _TDP = _newPowerLimit * 1000;

                        if (_TDP >= 5000)
                        {
                            // Apply new power and temp limit
                            cpuCommand = $"--tctl-temp={MaxTemperature} --cHTC-temp={MaxTemperature} --apu-skin-temp={MaxTemperature} --stapm-limit={_TDP}  --fast-limit={_TDP} --stapm-time=64 --slow-limit={_TDP} --slow-time=128 --vrm-current=300000 --vrmmax-current=300000 --vrmsoc-current=300000 --vrmsocmax-current=300000 ";
                            // Save new TDP to avoid unnecessary reapplies
                            _lastPowerLimit = _newPowerLimit;
                            iGPUControl._currentPowerLimit = _newPowerLimit;
                        }

                    }

                    else if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                    {
                        _TDP = _newPowerLimit * 1000;

                        // Apply new power and temp limit
                        cpuCommand = $"--tctl-temp={MaxTemperature} --ppt-limit={_TDP} --edc-limit={(int)(_TDP * 1.33)} --tdc-limit={(int)(_TDP * 1.33)} ";
                        _lastPowerLimit = _newPowerLimit;
                    }

                    else if (Family.TYPE == Family.ProcessorType.Intel)
                    {
                        _TDP = _newPowerLimit;
                        // Apply new power and temp limit

                        TDP_Management.changeTDP(_newPowerLimit, _newPowerLimit);
                        _lastPowerLimit = _newPowerLimit;
                    }
                }
            }
            catch { }


            _lastUsage = cpuLoad;
        }
    }
}
