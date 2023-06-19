using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility_Handheld.Scripts
{
    internal struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    public class Win32
    {
        [DllImport("User32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }

        public static long GetLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
            {
                throw new Exception(GetLastError().ToString());
            }

            return lastInPut.dwTime;
        }
    }

    public class UserActivityDetector
    {
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public static bool IsAnyKeyDown()
        {
            // Iterate through virtual key codes from 1 to 255
            for (int vKey = 1; vKey <= 255; vKey++)
            {
                short keyState = GetAsyncKeyState(vKey);
                if ((keyState & 0x8000) != 0)
                {
                    // At least one key is being held down
                    return true;
                }
            }

            // No key is being held down
            return false;
        }

        public static bool IsAnyControllerButtonPressed(UserIndex controllerNo)
        {
            try
            {

                var controller = new Controller(controllerNo);
                var state = controller.GetState().Gamepad;
                Gamepad gamepad = controller.GetState().Gamepad;
                float tx = gamepad.LeftThumbX;
                float ty = gamepad.LeftThumbY;

                // Check each button state
                foreach (GamepadButtonFlags button in Enum.GetValues(typeof(GamepadButtonFlags)))
                {
                    if ((state.Buttons & button) != 0)
                    {
                        return true;
                    }
                }

                if (ty > 8000 || ty < -8000) return true;
                if (tx > 8000 || tx < -8000) return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}