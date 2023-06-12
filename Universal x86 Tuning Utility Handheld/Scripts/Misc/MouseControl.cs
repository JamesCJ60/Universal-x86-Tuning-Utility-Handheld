using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace Universal_x86_Tuning_Utility_Handheld.Scripts.Misc
{
    internal class MouseControl
    {
        private static IMouseSimulator _mouseSimulator = new InputSimulator().Mouse;

        public const int RefreshRate = 61;
        public const int MovementDivider = 2_000;
        public const int ScrollDivider = 10_000;

        private static bool _wasLCDown;
        private static bool _wasRCDown;

        public static void RightButton(State state)
        {
            var isBDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
            if (isBDown && !_wasRCDown) _mouseSimulator.RightButtonDown();
            if (!isBDown && _wasRCDown) _mouseSimulator.RightButtonUp();
            _wasRCDown = isBDown;
        }

        public static void LeftButton(State state)
        {
            var isADown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
            if (isADown && !_wasLCDown) _mouseSimulator.LeftButtonDown();
            if (!isADown && _wasLCDown) _mouseSimulator.LeftButtonUp();
            _wasLCDown = isADown;
        }

        public static void Scroll(State state)
        {
            if (state.Gamepad.RightThumbX > 8000 || state.Gamepad.RightThumbX < -8000 || state.Gamepad.RightThumbY > 8000 || state.Gamepad.RightThumbY < -8000)
            {
                var x = state.Gamepad.RightThumbX / ScrollDivider;
                var y = state.Gamepad.RightThumbY / ScrollDivider;
                _mouseSimulator.HorizontalScroll(x);
                _mouseSimulator.VerticalScroll(y);
            }

        }

        public static void Movement(State state)
        {
            if (state.Gamepad.LeftThumbX > 8000 || state.Gamepad.LeftThumbX < -8000 || state.Gamepad.LeftThumbY > 8000 || state.Gamepad.LeftThumbY < -8000)
            {
                var x = state.Gamepad.LeftThumbX / MovementDivider;
                var y = state.Gamepad.LeftThumbY / MovementDivider;
                _mouseSimulator.MoveMouseBy(x, -y);
            }
        }
    }
}
