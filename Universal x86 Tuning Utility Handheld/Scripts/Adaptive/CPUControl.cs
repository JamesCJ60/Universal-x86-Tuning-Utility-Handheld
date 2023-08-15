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
        private static double PowerLimitIncrement = 0.5; // watts
        private static double _newPowerLimit = 999; // watts
        public static double _currentPowerLimit = 999; // watts
        private static double _lastPowerLimit = 1000; // watts
        public static int _lastUsage = 0; // %


        public static string cpuCommand = "";
        public static string coCommand = "";
        public static async void UpdatePowerLimit(int temperature, int cpuLoad, int MaxPowerLimit, int MinPowerLimit, int MaxTemperature, double fps = 0, int fpsLimit = 0)
        {
            try
            {
                if (temperature >= MaxTemperature - 2 || fpsLimit > 0 && fps > fpsLimit)
                {
                    // Reduce power limit if temperature is too high
                    _newPowerLimit = Math.Max(MinPowerLimit, _newPowerLimit - PowerLimitIncrement);
                }
                else if (temperature <= (MaxTemperature - 5) || fpsLimit > 0 && fps < fpsLimit)
                {
                    // Increase power limit if temperature allows
                    _newPowerLimit = Math.Min(MaxPowerLimit, _newPowerLimit + PowerLimitIncrement);
                }

                if (_newPowerLimit < MinPowerLimit) _newPowerLimit = MinPowerLimit;
                if (_newPowerLimit > MaxPowerLimit) _newPowerLimit = MaxPowerLimit;

                // Apply new power limit if power limit has changed
                if (_newPowerLimit <= _lastPowerLimit - 1 || _newPowerLimit >= _lastPowerLimit + 1)
                {
                    double _TDP = _newPowerLimit;

                    // Detect if AMD CPU or APU
                    if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                    {
                        _TDP = _newPowerLimit * 1000;

                        if (_TDP >= 5000)
                        {
                            // Apply new power and temp limit
                            cpuCommand = $"--tctl-temp={MaxTemperature} --cHTC-temp={MaxTemperature} --apu-skin-temp={MaxTemperature} --stapm-limit={(int)(_TDP)}  --fast-limit={(int)(_TDP)} --stapm-time=64 --slow-limit={(int)(_TDP)} --slow-time=128 --vrm-current=300000 --vrmmax-current=300000 --vrmsoc-current=300000 --vrmsocmax-current=300000 ";
                            // Save new TDP to avoid unnecessary reapplies
                            _lastPowerLimit = _newPowerLimit;
                            _currentPowerLimit = _newPowerLimit;
                            iGPUControl._currentPowerLimit = Convert.ToInt32(_newPowerLimit);
                        }

                    }

                    else if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
                    {
                        _TDP = _newPowerLimit * 1000;

                        // Apply new power and temp limit
                        cpuCommand = $"--tctl-temp={MaxTemperature} --ppt-limit={(int)(_TDP)} --edc-limit={(int)(_TDP * 1.33)} --tdc-limit={(int)(_TDP * 1.33)} ";
                        _lastPowerLimit = _newPowerLimit;
                        _currentPowerLimit = _newPowerLimit;
                    }

                    else if (Family.TYPE == Family.ProcessorType.Intel)
                    {
                        _TDP = _newPowerLimit;
                        // Apply new power and temp limit

                        TDP_Management.changeTDP(Convert.ToInt32(_newPowerLimit), Convert.ToInt32(_newPowerLimit));
                        _lastPowerLimit = _newPowerLimit;

                        _currentPowerLimit = _newPowerLimit;
                    }
                }
            }
            catch { }


            _lastUsage = cpuLoad;
        }
    }
}
