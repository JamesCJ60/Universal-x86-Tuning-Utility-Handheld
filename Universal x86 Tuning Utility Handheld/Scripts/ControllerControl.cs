using Nefarius.Drivers.HidHide;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;

namespace Universal_x86_Tuning_Utility_Handheld.Scripts
{
    internal class ControllerControl
    {

        static HidHideControlService con = new HidHideControlService();
        static ViGEmClient viGEmClient = new ViGEmClient();
        static IXbox360Controller targetController = viGEmClient.CreateXbox360Controller();

        public static bool isStarted = false;

        public static async void SetUp()
        {
            await Task.Run(() =>
            {
                HideOriginalController(false);
                targetController.Connect();

                while (isStarted)
                {
                    EmulateController();
                    Thread.Sleep(10);
                }
            });
        }

        public static void Stop()
        {
            HideOriginalController(true);
            targetController.Disconnect();
            viGEmClient.Dispose();
        }

        static void HideOriginalController(bool isEnabled = false)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");

            foreach (ManagementObject device in searcher.Get())
            {
                object caption = device["Caption"];
                if (caption != null && caption.ToString().Contains("Xbox"))
                {
                    object deviceId = device["DeviceID"];
                    Console.WriteLine($"{deviceId}");

                    if (isEnabled == true)
                    {
                        con.IsActive = false;
                        con.RemoveBlockedInstanceId(deviceId.ToString());
                        con.ClearBlockedInstancesList();
                        con.IsActive = false;
                    }
                    else if (!isEnabled)
                    {
                        con.IsActive = true;
                        con.AddApplicationPath(Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe"));
                        con.AddApplicationPath(Assembly.GetEntryAssembly().Location);
                        con.AddBlockedInstanceId(deviceId.ToString());
                        con.IsActive = true;
                    }
                }
            }
        }

        public static async void EmulateController()
        {
            try
            {
                UpdateController(UserIndex.One);
                UpdateController(UserIndex.Two);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        static void UpdateController(UserIndex index)
        {

            Controller physicalController = new Controller(index);
            State state = physicalController.GetState();

            targetController.SetButtonState(Xbox360Button.Start, (state.Gamepad.Buttons & GamepadButtonFlags.Start) != 0);
            targetController.SetButtonState(Xbox360Button.Back, (state.Gamepad.Buttons & GamepadButtonFlags.Back) != 0);

            if (Global._appVis != Visibility.Visible)
            {
                targetController.SetButtonState(Xbox360Button.A, (state.Gamepad.Buttons & GamepadButtonFlags.A) != 0);
                targetController.SetButtonState(Xbox360Button.B, (state.Gamepad.Buttons & GamepadButtonFlags.B) != 0);
                targetController.SetButtonState(Xbox360Button.X, (state.Gamepad.Buttons & GamepadButtonFlags.X) != 0);
                targetController.SetButtonState(Xbox360Button.Y, (state.Gamepad.Buttons & GamepadButtonFlags.Y) != 0);
                targetController.SetButtonState(Xbox360Button.LeftShoulder, (state.Gamepad.Buttons & GamepadButtonFlags.LeftShoulder) != 0);
                targetController.SetButtonState(Xbox360Button.RightShoulder, (state.Gamepad.Buttons & GamepadButtonFlags.RightShoulder) != 0);
                targetController.SetButtonState(Xbox360Button.LeftThumb, (state.Gamepad.Buttons & GamepadButtonFlags.LeftThumb) != 0);
                targetController.SetButtonState(Xbox360Button.RightThumb, (state.Gamepad.Buttons & GamepadButtonFlags.RightThumb) != 0);

                targetController.SetButtonState(Xbox360Button.Up, (state.Gamepad.Buttons & GamepadButtonFlags.DPadUp) != 0);
                targetController.SetButtonState(Xbox360Button.Down, (state.Gamepad.Buttons & GamepadButtonFlags.DPadDown) != 0);
                targetController.SetButtonState(Xbox360Button.Left, (state.Gamepad.Buttons & GamepadButtonFlags.DPadLeft) != 0);
                targetController.SetButtonState(Xbox360Button.Right, (state.Gamepad.Buttons & GamepadButtonFlags.DPadRight) != 0);

                targetController.SetAxisValue(0, state.Gamepad.LeftThumbX);
                targetController.SetAxisValue(1, state.Gamepad.LeftThumbY);
                targetController.SetAxisValue(2, state.Gamepad.RightThumbX);
                targetController.SetAxisValue(3, state.Gamepad.RightThumbY);
                targetController.SetSliderValue(0, state.Gamepad.LeftTrigger);
                targetController.SetSliderValue(1, state.Gamepad.RightTrigger);
            }
        }
    }
}
