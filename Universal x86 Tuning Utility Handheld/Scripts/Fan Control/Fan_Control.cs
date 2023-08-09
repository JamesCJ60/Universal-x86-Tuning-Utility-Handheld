using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility_Handheld.Services;
using Universal_x86_Tuning_Utility_Handheld.Views.Windows;

namespace Universal_x86_Tuning_Utility_Handheld.Scripts.Fan_Control
{
    internal class Fan_Control
    {
        public static int MaxFanSpeed = 100;
        public static int MinFanSpeed = 0;
        public static int MinFanSpeedPercentage = 25;

        public static double FanSpeed = 0;

        public static ushort FanToggleAddress = Convert.ToUInt16("0x0", 16);
        public static ushort FanChangeAddress = Convert.ToUInt16("0x0", 16);

        public static byte EnableToggleAddress = Convert.ToByte("0x0", 16);
        public static byte DisableToggleAddress = Convert.ToByte("0x0", 16);

        public static ushort RegAddress = Convert.ToByte("0x0", 16);
        public static ushort RegData = Convert.ToByte("0x0", 16);

        public static bool fanControlEnabled = false;

        public static bool isSupported = false;

        public static void UpdateAddresses()
        {
            string fanConfig = "";
            fanConfig = $"{GetSystemInfo.Manufacturer.ToUpper()}_{GetSystemInfo.Product.ToUpper()}.json";
            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
            path = path.Replace("Universal x86 Tuning Utility Handheld.dll", null);

            path = "Fan Configs\\" + fanConfig;

            if (File.Exists(path))
            {
                isSupported = true;

                var fanControlDataManager = new FanConfigManager(path);

                var dataForDevice = fanControlDataManager.GetDataForDevice();

                // Access data for the device
                MinFanSpeed = dataForDevice.MinFanSpeed;
                MaxFanSpeed = dataForDevice.MaxFanSpeed;
                MinFanSpeedPercentage = dataForDevice.MinFanSpeedPercentage;
                FanToggleAddress = Convert.ToUInt16(dataForDevice.FanControlAddress, 16);
                FanChangeAddress = Convert.ToUInt16(dataForDevice.FanSetAddress, 16);
                EnableToggleAddress = Convert.ToByte(dataForDevice.EnableToggleAddress, 16);
                DisableToggleAddress = Convert.ToByte(dataForDevice.DisableToggleAddress, 16);

                RegAddress = Convert.ToUInt16(dataForDevice.RegAddress, 16);
                RegData = Convert.ToUInt16(dataForDevice.RegData, 16);

                WinRingEC_Management.reg_addr = RegAddress;
                WinRingEC_Management.reg_data = RegData;
            }
        }

        public static void enableFanControl()
        {
            WinRingEC_Management.ECRamWrite(FanToggleAddress, EnableToggleAddress);
            fanControlEnabled = true;
        }

        public static void disableFanControl()
        {
            WinRingEC_Management.ECRamWrite(FanToggleAddress, DisableToggleAddress);
            fanControlEnabled = false;
        }

        public static bool fanIsEnabled()
        {
            byte returnvalue = WinRingEC_Management.ECRamRead(FanToggleAddress);
            if (returnvalue == 0) { return false; } else { return true; }
        }

        public static void setFanSpeed(int speedPercentage)
        {
            if (speedPercentage < MinFanSpeedPercentage && speedPercentage > 0)
            {
                speedPercentage = MinFanSpeedPercentage;
            }

            byte setValue = (byte)Math.Round(((double)speedPercentage / 100) * MaxFanSpeed, 0);
            WinRingEC_Management.ECRamWrite(FanChangeAddress, setValue);

            FanSpeed = speedPercentage;
        }

        public static void readFanSpeed()
        {
            int fanSpeed = 0;

            byte returnvalue = WinRingEC_Management.ECRamRead(FanChangeAddress);

            double fanPercentage = Math.Round(100 * (Convert.ToDouble(returnvalue) / MaxFanSpeed), 0);
            FanSpeed = fanPercentage;
        }

        public static int GetCpuTemperature()
        {
            try
            {
                Computer computer = new Computer
                {
                    IsCpuEnabled = true,
                };
                computer.Open();
                var cpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
                cpu.Update();
                var temperature = cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
                int temp = 0;
                if (temperature != null)
                {
                    temp = (int)temperature.Value;
                }
                else
                {
                    temp = 0;
                }
                //computer.Close();
                return temp;
            }
            catch (Exception ex)
            {
                // Log exception
                return 0;
            }
        }

        public static void UpdateFanCurve(int[] speeds)
        {
            try
            {
                int[] temps = { 25, 35, 45, 55, 65, 75, 85, 95 };

                int cpuTemperature = 0;

                if (GetSensor.isOpen) cpuTemperature = MainWindow.CPUTemp;
                else cpuTemperature = GetCpuTemperature();

                var fanSpeed = Interpolate(speeds, temps, cpuTemperature);

                if (fanControlEnabled) setFanSpeed(fanSpeed);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static int Interpolate(int[] yValues, int[] xValues, int x)
        {
            int i = Array.FindIndex(xValues, t => t >= x);

            if (i == -1) // temperature is lower than the first input point
            {
                return yValues[0];
            }
            else if (i == 0) // temperature is equal to or higher than the first input point
            {
                return yValues[0];
            }
            else if (i == xValues.Length) // temperature is higher than the last input point
            {
                return yValues[xValues.Length - 1];
            }
            else // interpolate between two closest input points
            {
                return Interpolate(yValues[i - 1], xValues[i - 1], yValues[i], xValues[i], x);
            }
        }

        private static int Interpolate(int y1, int x1, int y2, int x2, int x)
        {
            return (y1 * (x2 - x) + y2 * (x - x1)) / (x2 - x1);
        }
    }
}
