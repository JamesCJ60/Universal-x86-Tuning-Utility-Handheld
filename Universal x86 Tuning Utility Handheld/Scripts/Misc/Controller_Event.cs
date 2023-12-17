using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility_Handheld.Properties;

namespace Universal_x86_Tuning_Utility_Handheld.Scripts.Misc
{
    internal class Controller_Event
    {
        public static Controller? controller;
        public static Gamepad currentGamePad;
        public static Gamepad previousGamePad;

        public static bool suspendEventsForGamepadHotKeyProgramming = false;

        public static DispatcherTimer timerController = new DispatcherTimer(DispatcherPriority.Send);

        public static buttonEvents buttonEvents = new buttonEvents();
        public static int activeTimerTickInterval = 60;
        public static int passiveTimerTickInterval = 100;


 

        public static void start_Controller_Management()
        {
            //create background thread to handle controller input
            getController();
            //timerController.Interval = TimeSpan.FromMilliseconds(activeTimerTickInterval);
            //timerController.Tick += controller_Tick;

            //timerController.Start();
            Thread controllerThread = new Thread(controller_Tickthread);
            controllerThread.IsBackground = true;
            controllerThread.Start();
        }
        private static string continuousInputNew = "";
        private static string continuousInputCurrent = "";
        private static int continuousInputCounter = 0;
        private static void controller_Tick(object sender, EventArgs e)
        {
            //start timer to read and compare controller inputs
            //Controller input handler
            //error number CM05
            try
            {
                if (controller == null)


                {
                    getController();
                    if (controller.IsConnected == false)
                    {
                        getController();
                    }
                }
                else if (!controller.IsConnected)
                {
                    getController();
                }


                if (controller != null)
                {
                    if (controller.IsConnected)
                    {
                        //a quick routine to check other controllers for the swap controller command
                        checkSwapController();

                        //var watch = System.Diagnostics.Stopwatch.StartNew();
                        currentGamePad = controller.GetState().Gamepad;

                        ushort btnShort = ((ushort)currentGamePad.Buttons);


                        if (!suspendEventsForGamepadHotKeyProgramming)
                        {
                            //check if controller combo is in controller hot key dictionary

                           
                                //reset continuousNew for every cycle
                                continuousInputNew = "";

                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.Back) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.Back))
                                {
                                    buttonEvents.raiseControllerInput("Back");
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.Start) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.Start))
                                {
                                    buttonEvents.raiseControllerInput("Start");
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb))
                                {
                                    buttonEvents.raiseControllerInput("L3");
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.RightThumb) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.RightThumb))
                                {
                                    buttonEvents.raiseControllerInput("R3");
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.A) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.A))
                                {
                                    buttonEvents.raiseControllerInput("A");
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.X) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.X))
                                {
                                    buttonEvents.raiseControllerInput("X");
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.Y) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.Y))
                                {
                                    buttonEvents.raiseControllerInput("Y");
                                }

                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.B) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.B))
                                {
                                    buttonEvents.raiseControllerInput("B");
                                }

                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
                                {
                                    buttonEvents.raiseControllerInput("LB");
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                                {
                                    buttonEvents.raiseControllerInput("RB");
                                }
                                if (currentGamePad.LeftTrigger > 200 && previousGamePad.LeftTrigger <= 200)
                                {
                                    buttonEvents.raiseControllerInput("LT");
                                }
                                if (currentGamePad.RightTrigger > 200 && previousGamePad.RightTrigger <= 200)
                                {
                                    buttonEvents.raiseControllerInput("RT");
                                }

                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) || currentGamePad.LeftThumbY > 12000)
                                {
                                    if (!previousGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && previousGamePad.LeftThumbY <= 12000)
                                    {
                                        buttonEvents.raiseControllerInput("Up");
                                    }
                                    else
                                    {
                                        continuousInputNew = "Up";
                                    }
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) || currentGamePad.LeftThumbY < -12000)
                                {
                                    if (!previousGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && previousGamePad.LeftThumbY >= -12000)
                                    {
                                        buttonEvents.raiseControllerInput("Down");
                                    }
                                    else
                                    {
                                        continuousInputNew = "Down";
                                    }
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || currentGamePad.LeftThumbX > 12000)
                                {
                                    if (!previousGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && previousGamePad.LeftThumbX <= 12000)
                                    {
                                        buttonEvents.raiseControllerInput("Right");
                                    }
                                    else
                                    {
                                        continuousInputNew = "Right";
                                    }
                                }
                                if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || currentGamePad.LeftThumbX < -12000)
                                {
                                    if (!previousGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && previousGamePad.LeftThumbX >= -12000)
                                    {
                                        buttonEvents.raiseControllerInput("Left");
                                    }
                                    else
                                    {
                                        continuousInputNew = "Left";
                                    }
                                }


                                if (continuousInputNew != continuousInputCurrent)
                                {
                                    continuousInputCurrent = continuousInputNew;
                                    continuousInputCounter = 1;
                                }
                                else
                                {
                                    if (continuousInputCurrent != "")
                                    {
                                        continuousInputCounter++;
                                        if (continuousInputCounter > 9)
                                        {

                                            buttonEvents.raiseControllerInput(continuousInputCurrent);
                                        }
                                    }

                                }

                            }



                        }
                        //watch.Stop();
                        //Debug.WriteLine($"Total Execution Time: {watch.ElapsedMilliseconds} ms");
                        //doSomeWork();

                        previousGamePad = currentGamePad;
                    }
            }
            catch (Exception ex)
            {

            }



        }
        private static void controller_Tickthread()
        {
            //start timer to read and compare controller inputs
            //Controller input handler
            //error number CM05
            while (true)
            {
                try
                {
                    if (controller == null)


                    {
                        getController();
                        if (controller.IsConnected == false)
                        {
                            getController();
                        }
                    }
                    else if (!controller.IsConnected)
                    {
                        getController();
                    }


                    if (controller != null)
                    {
                        if (controller.IsConnected)
                        {
                            //a quick routine to check other controllers for the swap controller command
                            checkSwapController();

                            //var watch = System.Diagnostics.Stopwatch.StartNew();
                            currentGamePad = controller.GetState().Gamepad;

                            ushort btnShort = ((ushort)currentGamePad.Buttons);


                            if (!suspendEventsForGamepadHotKeyProgramming)
                            {
                                

                                    //reset continuousNew for every cycle
                                    continuousInputNew = "";

                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.Back) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.Back))
                                    {
                                        buttonEvents.raiseControllerInput("Back");
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.Start) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.Start))
                                    {
                                        buttonEvents.raiseControllerInput("Start");
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb))
                                    {
                                        buttonEvents.raiseControllerInput("L3");
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.RightThumb) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.RightThumb))
                                    {
                                        buttonEvents.raiseControllerInput("R3");
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.A) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.A))
                                    {
                                        buttonEvents.raiseControllerInput("A");
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.X) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.X))
                                    {
                                        buttonEvents.raiseControllerInput("X");
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.Y) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.Y))
                                    {
                                        buttonEvents.raiseControllerInput("Y");
                                    }

                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.B) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.B))
                                    {
                                        buttonEvents.raiseControllerInput("B");
                                    }

                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
                                    {
                                        buttonEvents.raiseControllerInput("LB");
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) && !previousGamePad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                                    {
                                        buttonEvents.raiseControllerInput("RB");
                                    }
                                    if (currentGamePad.LeftTrigger > 200 && previousGamePad.LeftTrigger <= 200)
                                    {
                                        buttonEvents.raiseControllerInput("LT");
                                    }
                                    if (currentGamePad.RightTrigger > 200 && previousGamePad.RightTrigger <= 200)
                                    {
                                        buttonEvents.raiseControllerInput("RT");
                                    }

                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) || currentGamePad.LeftThumbY > 12000)
                                    {
                                        if (!previousGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && previousGamePad.LeftThumbY <= 12000)
                                        {
                                            buttonEvents.raiseControllerInput("Up");
                                        }
                                        else
                                        {
                                            continuousInputNew = "Up";
                                        }
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) || currentGamePad.LeftThumbY < -12000)
                                    {
                                        if (!previousGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && previousGamePad.LeftThumbY >= -12000)
                                        {
                                            buttonEvents.raiseControllerInput("Down");
                                        }
                                        else
                                        {
                                            continuousInputNew = "Down";
                                        }
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || currentGamePad.LeftThumbX > 12000)
                                    {
                                        if (!previousGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && previousGamePad.LeftThumbX <= 12000)
                                        {
                                            buttonEvents.raiseControllerInput("Right");
                                        }
                                        else
                                        {
                                            continuousInputNew = "Right";
                                        }
                                    }
                                    if (currentGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || currentGamePad.LeftThumbX < -12000)
                                    {
                                        if (!previousGamePad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && previousGamePad.LeftThumbX >= -12000)
                                        {
                                            buttonEvents.raiseControllerInput("Left");
                                        }
                                        else
                                        {
                                            continuousInputNew = "Left";
                                        }
                                    }


                                    if (continuousInputNew != continuousInputCurrent)
                                    {
                                        continuousInputCurrent = continuousInputNew;
                                        continuousInputCounter = 1;
                                    }
                                    else
                                    {
                                        if (continuousInputCurrent != "")
                                        {
                                            continuousInputCounter++;
                                            if (continuousInputCounter > 9)
                                            {

                                                buttonEvents.raiseControllerInput(continuousInputCurrent);
                                            }
                                        }

                                    }
                                }
                            }
                        Controller:
                            previousGamePad = currentGamePad;
                            Thread.Sleep(activeTimerTickInterval);
                        }

                    
                }
                catch (Exception ex)
                {

                }
            }
        }

        private static void checkSwapController()
        {

            //error number CM03
            try
            {
                List<Controller> controllerList = new List<Controller>();

                controllerList.Add(new Controller(UserIndex.One));
                controllerList.Add(new Controller(UserIndex.Two));
                controllerList.Add(new Controller(UserIndex.Three));
                controllerList.Add(new Controller(UserIndex.Four));

                foreach (Controller swapController in controllerList)
                {

                    if (swapController != null)
                    {
                        if (swapController.IsConnected)
                        {
                            Gamepad swapGamepad = swapController.GetState().Gamepad;
                            if (swapGamepad.Buttons.HasFlag(GamepadButtonFlags.Start) && swapGamepad.Buttons.HasFlag(GamepadButtonFlags.Back))
                            {

                                controller = swapController;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }


        private static void getController()
        {
            //error number CM04
            try
            {
                int controllerNum = 1;
                //get controller used, loop controller number if less than 5, so if controller is connected make num = 5 to get out of while loop
                while (controllerNum < 6)
                {
                    switch (controllerNum)
                    {
                        default:
                            break;
                        case 1:
                            controller = new Controller(UserIndex.One);
                            break;
                        case 2:
                            controller = new Controller(UserIndex.Two);
                            break;
                        case 3:
                            controller = new Controller(UserIndex.Three);
                            break;
                        case 4:
                            controller = new Controller(UserIndex.Four);
                            break;
                        case 5:
                            timerController.Interval = TimeSpan.FromMilliseconds(1500);
                            controllerNum = 6;
                            buttonEvents.raiseControllerStatusChanged();
                            break;
                    }
                    if (controller == null)
                    {
                        controllerNum++;
                    }
                    else
                    {
                        if (controller.IsConnected)
                        {
                            controllerNum = 6;
                            timerController.Interval = TimeSpan.FromMilliseconds(activeTimerTickInterval);
                            buttonEvents.raiseControllerStatusChanged();
                        }
                        else
                        {
                            controllerNum++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    public class buttonEvents
    {

        public event EventHandler controllerStatusChangedEvent;
        public void raiseControllerStatusChanged()
        {
            controllerStatusChangedEvent?.Invoke(this, EventArgs.Empty);
        }


        public event EventHandler<controllerInputEventArgs> controllerInput;

        public void raiseControllerInput(string action)
        {

            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                controllerInput?.Invoke(this, new controllerInputEventArgs(action));
            });

        }

        public event EventHandler openAppEvent;
        public void RaiseOpenAppEvent()
        {
            openAppEvent?.Invoke(this, EventArgs.Empty);
        }
    }
    public class controllerInputEventArgs : EventArgs
    {
        public string Action { get; set; }
        public controllerInputEventArgs(string action)
        {
            this.Action = action;
        }
    }
    public class controllerPageInputEventArgs : EventArgs
    {
        public string Action { get; set; }
        public string WindowPage { get; set; }
        public controllerPageInputEventArgs(string action, string windowpage)
        {
            this.Action = action;
            this.WindowPage = windowpage;
        }
    }
    public class controllerUserControlInputEventArgs : EventArgs
    {
        public string Action { get; set; }
        public string WindowPage { get; set; }
        public string UserControl { get; set; }
        public controllerUserControlInputEventArgs(string action, string windowpage, string usercontrol)
        {
            this.Action = action;
            this.WindowPage = windowpage;
            this.UserControl = usercontrol;
        }
    }
}