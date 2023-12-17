using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
using Microsoft.Win32;
using RTSSSharedMemoryNET;
using RyzenSmu;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Scripts.AMD_Backend;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility_Handheld.Properties;
using Universal_x86_Tuning_Utility_Handheld.Scripts;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Adaptive;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Fan_Control;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Intel;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Universal_x86_Tuning_Utility_Handheld.Services;
using Universal_x86_Tuning_Utility_Handheld.Views.Pages;
using Windows.Devices.Power;
using Windows.Devices.Radios;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Universal_x86_Tuning_Utility_Handheld.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public ViewModels.MainWindowViewModel ViewModel
        {
            get; set;
        }
        public ViewModels.AdvancedViewModel AdViewModel
        {
            get; set;
        }

        private DispatcherTimer timer;

        private readonly INavigationService _navigationService;
        public static string mbo = "";
        static int RadeonCount = 0;
        public MainWindow(ViewModels.MainWindowViewModel viewModel, ViewModels.AdvancedViewModel adViewModel, IPageService pageService, INavigationService navigationService)
        {
            ViewModel = viewModel;
            AdViewModel = adViewModel;
            DataContext = this;

            InitializeComponent();
            SetPageService(pageService);

            _ = Tablet.TabletDevices;

            navigationService.SetNavigationControl(RootNavigation);
            SetWindowPosition();
            Loaded += MainWindow_Loaded;
            Global._mainWindowNav = RootNavigation;

            //set up timer for key combo system
            DispatcherTimer checkKeyInput = new DispatcherTimer();
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.14);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();

            DispatcherTimer adaptiveFPS = new DispatcherTimer();
            adaptiveFPS.Interval = TimeSpan.FromSeconds(0.2);
            adaptiveFPS.Tick += AdaptiveFPS_Tick;
            adaptiveFPS.Start();

            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Tick += Mouse_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(1000 / MouseControl.RefreshRate);
            _timer.Start();

            DispatcherTimer _sensor = new DispatcherTimer();
            _sensor.Tick += Sensor_Tick;
            _sensor.Interval = TimeSpan.FromSeconds(0.2);
            _sensor.Start();

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            UpdateInfo();
            GetWifi();
            getBatteryTime();
            ApplyController();
            _navigationService = navigationService;

            Controller_Event.start_Controller_Management();

            if (Settings.Default.isASUS == true) App.wmi.SubscribeToEvents(WatcherEventArrived);

            UpdatePreset("Default");

            //Detect if an AYA Neo is being used
            ManagementObjectSearcher baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject queryObj in baseboardSearcher.Get())
            {
                mbo = queryObj["Manufacturer"].ToString();
                mbo = mbo.ToLower();
            }

            PowerPlans.HideAttribute("sub_processor", "PROCFREQMAX");
            PowerPlans.HideAttribute("sub_processor", "PROCFREQMAX1");
            PowerPlans.HideAttribute("sub_processor", "PERFEPP");
            PowerPlans.HideAttribute("sub_processor", "PERFEPP1");
            PowerPlans.HideAttribute("sub_processor", "CPMINCORES");
            PowerPlans.HideAttribute("sub_processor", "CPMAXCORES");

            RadeonCount = GetRadeonGPUCount();
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            SetWindowPosition(true);
            SetWindowPosition(true);
            this.UpdateLayout();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            await Task.Run(() => Display.setUpLists());
            WindowStartupLocation = WindowStartupLocation.Manual;
            SetWindowPosition();
            Wpf.Ui.Appearance.Watcher.Watch(
    this,
    Wpf.Ui.Appearance.BackgroundType.Mica,
    true
    );
            if (Settings.Default.StartMini) this.Visibility = Visibility.Hidden;
            if (Settings.Default.DisplayBatHz < 0 || Settings.Default.DisplayBatHz > Display.uniqueRefreshRates.Count - 1 || Settings.Default.DisplayPlugHz < 0 || Settings.Default.DisplayPlugHz > Display.uniqueRefreshRates.Count - 1 || Settings.Default.DisplayRes < 0 || Settings.Default.DisplayRes > Display.uniqueResolutions.Count - 1)
            {
                Settings.Default.DisplayBatHz = -1;
                Settings.Default.DisplayPlugHz = -1;
                Settings.Default.DisplayRes = -1;
                Settings.Default.Save();
            }

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            Fan_Control.UpdateAddresses();

            //ControllerControl.isStarted = true;
            //ControllerControl.SetUp();
        }

        int i = 0;
        int update = 0;
        bool updatedTB = false;
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!updatedTB)
            {
                HideFromTaskbar();
                updatedTB = true;
            }
            else
            {
                if (Visibility == Visibility.Visible && update >= 4)
                {
                    UpdateInfo();
                    GetWifi();
                    getBatteryTime();
                }
                if (update > 2) update = -1;

                ApplySettings();

                update++;
            }
        }

        bool setFPS = false;
        bool setFan = false;
        string lastFan = "";
        int perfMode = -1;

        bool lastEpp = false;
        bool lastMaxClock = false;
        bool lastCores = false;
        bool lastRSR = false;
        int lastEppVal = -1;
        int lastMaxClockVal = -1;
        int lastCoresVal = -1;
        int lastRSRVal = -1;
        private async void ApplySettings()
        {
            try
            {
                await Task.Run(() =>
                {
                    string commandString = "";

                    if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                    {
                        if (AdViewModel.IsTemp == true && AdViewModel.IsAdaptiveTDP == false) commandString = $"--tctl-temp={AdViewModel.TempLimit} --skin-temp-limit={AdViewModel.TempLimit} ";
                        if (AdViewModel.IsPower == true && AdViewModel.IsAdaptiveTDP == false) commandString = commandString + $"--stapm-limit={AdViewModel.PowerLimit * 1000} --slow-limit={AdViewModel.PowerLimit * 1000} --fast-limit={AdViewModel.PowerLimit * 1000} --vrm-current={(AdViewModel.PowerLimit * 1000) * 2} --vrmmax-current={(AdViewModel.PowerLimit * 1000) * 2} ";
                        if (AdViewModel.IsUndervolt == true)
                        {
                            if (AdViewModel.UnderVolt >= 0) commandString = commandString + $"--set-coall={AdViewModel.UnderVolt} ";
                            if (AdViewModel.UnderVolt < 0) commandString = commandString + $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * AdViewModel.UnderVolt))} ";
                        }
                        if (AdViewModel.IsIGPUClock == true && AdViewModel.IsAdaptiveiGPU == false) commandString = commandString + $"--gfx-clk={AdViewModel.IGPUClock} ";

                        if (commandString != null && commandString != "") RyzenAdj_To_UXTU.Translate(commandString);
                    }
                    if (Family.TYPE == Family.ProcessorType.Intel && AdViewModel.IsAdaptiveTDP == false)
                    {
                        TDP_Management.changeTDP(AdViewModel.PowerLimit, AdViewModel.PowerLimit);
                    }

                    if (AdViewModel.IsAdaptiveTDP == true) adaptiveTDP_iGPU();
                    if (lastMaxClock != AdViewModel.IsMaxClock || lastMaxClockVal != AdViewModel.MaxClock)
                    {
                        if (AdViewModel.IsMaxClock == true)
                        {

                            // Set the AC and DC values for PROCFREQMAX
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX", (uint)AdViewModel.MaxClock, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX", (uint)AdViewModel.MaxClock, false);

                            // Set the AC and DC values for PROCFREQMAX1
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX1", (uint)AdViewModel.MaxClock, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX1", (uint)AdViewModel.MaxClock, false);

                            // Activate the current power scheme
                            PowerPlans.SetActiveScheme("scheme_current");
                        }
                        else
                        {
                            // Set the AC and DC values for PROCFREQMAX
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX", 0, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX", 0, false);

                            // Set the AC and DC values for PROCFREQMAX1
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX1", 0, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX1", 0, false);

                            // Activate the current power scheme
                            PowerPlans.SetActiveScheme("scheme_current");
                        }

                        lastMaxClock = AdViewModel.IsMaxClock;
                        lastMaxClockVal = AdViewModel.MaxClock;
                    }
                    if (lastEpp != AdViewModel.IsEPP || lastEppVal != AdViewModel.EPP)
                    {
                        if (AdViewModel.IsEPP == true)
                        {

                            // Set the AC and DC values for PROCFREQMAX
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP", (uint)AdViewModel.EPP, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP", (uint)AdViewModel.EPP, false);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", (uint)AdViewModel.EPP, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", (uint)AdViewModel.EPP, false);

                            // Activate the current power scheme
                            PowerPlans.SetActiveScheme("scheme_current");
                        }
                        else
                        {

                            // Set the AC and DC values for EPP
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP ", 40, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP ", 40, false);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", 25, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", 25, false);

                            // Activate the current power scheme
                            PowerPlans.SetActiveScheme("scheme_current");
                        }

                        lastEpp = AdViewModel.IsEPP;
                        lastEppVal = AdViewModel.EPP;
                    }
                    if (lastCores != AdViewModel.IsCoreCount || lastCoresVal != AdViewModel.CoreCount)
                    {
                        if (AdViewModel.IsCoreCount == true)
                        {
                            int MaxCoreCount = 0;
                            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get()) { MaxCoreCount = Convert.ToInt32(item["NumberOfCores"]); }

                            MaxCoreCount = (int)((100 / MaxCoreCount) * AdViewModel.CoreCount);

                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMINCORES ", (uint)MaxCoreCount, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMINCORES ", (uint)MaxCoreCount, false);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMINCORES ", (uint)MaxCoreCount, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMINCORES ", (uint)MaxCoreCount, false);

                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMAXCORES  ", (uint)MaxCoreCount, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMAXCORES  ", (uint)MaxCoreCount, false);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMAXCORES  ", (uint)MaxCoreCount, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMAXCORES  ", (uint)MaxCoreCount, false);

                            // Activate the current power scheme
                            PowerPlans.SetActiveScheme("scheme_current");
                        }
                        else
                        {
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMINCORES ", 100, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMINCORES ", 100, false);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMINCORES ", 100, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMINCORES ", 100, false);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMAXCORES  ", 100, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMAXCORES  ", 100, false);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMAXCORES  ", 100, true);
                            PowerPlans.SetPowerValue("scheme_current", "sub_processor", "CPMAXCORES  ", 100, false);

                            // Activate the current power scheme
                            PowerPlans.SetActiveScheme("scheme_current");
                        }

                        lastCores = AdViewModel.IsCoreCount;
                        lastCoresVal = AdViewModel.CoreCount;
                    }

                    if (lastRSR != AdViewModel.IsRSR == true || lastRSRVal != AdViewModel.RSR)
                    {
                        if (AdViewModel.IsRSR == true)
                        {
                            ADLXBackend.SetRSR(true);
                            ADLXBackend.SetRSRSharpness(AdViewModel.RSR);
                        }
                        else ADLXBackend.SetRSR(false);
                        lastRSR = AdViewModel.IsRSR;
                        lastRSRVal = AdViewModel.RSR;
                    }

                    if (AdViewModel.IsFPS == true && AdViewModel.IsAdaptiveFPS == false)
                    {
                        try
                        {
                            if (RTSS.RTSSRunning() == true)
                            {
                                RTSS.getRTSSFPSLimit();
                                if (RTSS.fps != AdViewModel.Fps) RTSS.setRTSSFPSLimit(AdViewModel.Fps);
                                setFPS = true;
                            }
                            else
                            {
                                RTSS.startRTSS();
                                RTSS.getRTSSFPSLimit();
                                if (RTSS.fps != AdViewModel.Fps) RTSS.setRTSSFPSLimit(AdViewModel.Fps);
                                setFPS = true;
                            }
                        }
                        catch { }
                    }
                    else if (AdViewModel.IsFPS == false && setFPS && AdViewModel.IsAdaptiveFPS == false)
                    {
                        RTSS.setRTSSFPSLimit(0);
                        setFPS = false;
                    }


                    if (Settings.Default.isFanContol)
                    {
                        if (!Settings.Default.isASUS && Fan_Control.isSupported)
                        {
                            setFan = true;
                            string speedString = Settings.Default.fanCurve;
                            string[] speedStringArray = speedString.Split('-');
                            int[] speeds = Array.ConvertAll(speedStringArray, int.Parse);
                            Fan_Control.UpdateFanCurve(speeds);
                            Fan_Control.enableFanControl();
                        }
                        else if (Settings.Default.isASUS && lastFan != Settings.Default.fanCurve || Settings.Default.isASUS && perfMode != Settings.Default.acMode)
                        {
                            string speedString = Settings.Default.fanCurve;
                            string[] speedStringArray = speedString.Split('-');
                            int[] speeds = Array.ConvertAll(speedStringArray, int.Parse);

                            byte[] curve = new byte[16];
                            curve[0] = (byte)25;
                            curve[1] = (byte)35;
                            curve[2] = (byte)45;
                            curve[3] = (byte)55;
                            curve[4] = (byte)65;
                            curve[5] = (byte)75;
                            curve[6] = (byte)85;
                            curve[7] = (byte)95;
                            curve[8] = (byte)speeds[0];
                            curve[9] = (byte)speeds[1];
                            curve[10] = (byte)speeds[2];
                            curve[11] = (byte)speeds[3];
                            curve[12] = (byte)speeds[4];
                            curve[13] = (byte)speeds[5];
                            curve[14] = (byte)speeds[6];
                            curve[15] = (byte)speeds[7];
                            string bitCurve = BitConverter.ToString(curve);
                            Debug.WriteLine(bitCurve);

                            App.wmi.SetFanCurve(0, curve);
                            App.wmi.SetFanCurve(1, curve);

                            lastFan = speedString;
                            perfMode = Settings.Default.acMode;
                        }
                    }
                    else if (setFan) Fan_Control.disableFanControl();

                });
            }
            catch { }
        }

        private async void UpdateInfo()
        {
            if (Visibility == Visibility.Visible)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        PowerStatus powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;
                        int batteryLifePercent = (int)(powerStatus.BatteryLifePercent * 100);
                        ViewModel.Battery = batteryLifePercent;
                        ViewModel.Time = DateTime.Now;
                        bool isBatteryCharging = powerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online;
                        if (!isBatteryCharging)
                        {
                            if (batteryLifePercent >= 100) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery1024;
                            else if (batteryLifePercent >= 90) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery924;
                            else if (batteryLifePercent >= 80) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery824;
                            else if (batteryLifePercent >= 70) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery724;
                            else if (batteryLifePercent >= 60) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery624;
                            else if (batteryLifePercent >= 50) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery524;
                            else if (batteryLifePercent >= 40) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery424;
                            else if (batteryLifePercent >= 30) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery324;
                            else if (batteryLifePercent >= 20) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery224;
                            else if (batteryLifePercent >= 10) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery124;
                            else if (batteryLifePercent >= 0) ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery024;
                            else ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.Battery024;
                        }
                        else ViewModel.BatteryIcon = Wpf.Ui.Common.SymbolRegular.BatteryCharge24;

                        GetWifi();

                        Global.wifi = ViewModel.WifiIcon;
                        Global.battery = ViewModel.BatteryIcon;
                        Global.batteryPer = ViewModel.Battery;
                    });
                }
                catch { }
            }
        }

        private void HideFromTaskbar()
        {
            ShowInTaskbar = false;
        }

        private void SetWindowPosition(bool isDisplayChange = false)
        {
            this.Topmost = true;
            this.MaxWidth = 520;
            this.MinWidth = 520;
            this.Width = 520;

            // Get the primary screen dimensions
            var primaryScreen = System.Windows.SystemParameters.WorkArea;
            var screenWidth = primaryScreen.Width;
            var screenHeight = primaryScreen.Height;

            this.Left = screenWidth - this.Width - 12;
            this.Top = primaryScreen.Top + 12;

            this.Height = 0;
            this.MaxHeight = 0;
            this.MinHeight = 0;

            this.Height = screenHeight - 24;
            this.MaxHeight = screenHeight - 24;
            this.MinHeight = screenHeight - 24;

            this.WindowStyle = WindowStyle.None;

            this.UpdateLayout();
            this.Activate();
            this.Focus();
            Wpf.Ui.Appearance.Watcher.Watch(this, Wpf.Ui.Appearance.BackgroundType.Mica, true);
            Global._appVis = this.Visibility;
        }

        #region INavigationWindow methods

        public Frame GetFrame()
            => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            isControllerEmu = false;
            ControllerControl.isStarted = false;
            ControllerControl.Stop();
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void UiWindow_LocationChanged(object sender, EventArgs e)
        {
            SetWindowPosition();
            UpdateLayout();
        }

        private IntPtr Handle => new WindowInteropHelper(this).Handle;

        private void NotifyIcon_LeftClick(Wpf.Ui.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Visibility = Visibility.Hidden;
            }
            else
            {
                Visibility = Visibility.Visible;
                this.Activate();
                this.Focus();
            }
            SetWindowPosition();
            UpdateLayout();

            Global._appVis = this.Visibility;
        }

        private void ApplyController()
        {
            //if (Settings.Default.isVib == true)
            //{
            //    // Create an instance of the XInput controller
            //    Controller controller = new Controller(UserIndex.One);

            //    // Check if the controller is connected
            //    if (controller.IsConnected)
            //    {
            //        // Set the vibration parameters
            //        Vibration vibration = new Vibration
            //        {
            //            LeftMotorSpeed = (ushort)((Settings.Default.LeftMotor / 100.0) * 65535),
            //            RightMotorSpeed = (ushort)((Settings.Default.RightMotor / 100.0) * 65535)
            //        };

            //        // Apply the vibration
            //        controller.SetVibration(vibration);
            //    }
            //}



        }

        private void UiWindow_StateChanged(object sender, EventArgs e)
        {
            SetWindowPosition();
            UpdateLayout();
        }

        private void TitleBar_MinimizeClicked(object sender, RoutedEventArgs e)
        {
        }

        public static int inactiveFPS = 60;
        public static int activeFPS = 165;
        bool isAnyKeyHeldDown;
        bool isControllerOne;
        bool isControllerTwo;
        bool isActive = false;
        public static int minimise = 0;
        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            ControllerInput(UserIndex.One);
            ControllerInput(UserIndex.Two);

            try
            {
                if (AdViewModel.IsAdaptiveFPS)
                {
                    inactiveFPS = AdViewModel.MinFps;
                    activeFPS = AdViewModel.MaxFps;

                    isAnyKeyHeldDown = UserActivityDetector.IsAnyKeyDown();
                    isControllerOne = UserActivityDetector.IsAnyControllerButtonPressed(UserIndex.One);
                    isControllerTwo = UserActivityDetector.IsAnyControllerButtonPressed(UserIndex.Two);
                    if (Win32.GetIdleTime() > 200 && !isAnyKeyHeldDown && !isControllerOne && !isControllerTwo) isActive = false;
                    else isActive = true;
                }
            }
            catch { }
        }

        async void AdaptiveFPS_Tick(object sender, EventArgs e)
        {
            try
            {
                if (AdViewModel.IsAdaptiveFPS)
                {
                    if (RTSS.RTSSRunning() != true) RTSS.startRTSS();
                    RTSS.getRTSSFPSLimit();
                    if (!isActive && RTSS.fps != inactiveFPS) RTSS.setRTSSFPSLimit(inactiveFPS);
                    if (isActive && RTSS.fps != activeFPS) RTSS.setRTSSFPSLimit(activeFPS);

                    setFPS = true;
                }
            }
            catch { }
        }

        private static Controller controller;
        private void ControllerInput(UserIndex controllerNo)
        {
            try
            {
                controller = new Controller(controllerNo);

                bool connected = controller.IsConnected;

                if (minimise == 1)
                {
                    if (Visibility == Visibility.Visible)
                    {
                        Visibility = Visibility.Hidden;

                        SetWindowPosition();
                        UpdateLayout();
                    }

                    minimise = 0;
                }

                if (connected)
                {
                    //get controller state
                    var state = controller.GetState();

                    //detect if keyboard or controller combo is being activated
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                    {
                        Global.shortCut = true;
                    }
                    else if (Global.shortCut == true) Global.shortCut = false;


                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                    {
                        if (Visibility == Visibility.Visible)
                        {
                            Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            Visibility = Visibility.Visible;
                            this.Activate();
                            this.Focus();
                            SetWindowPosition();
                            UpdateLayout();
                        }
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && this.Visibility != Visibility.Hidden && Global.shortCut == false)
                    {
                        int current = RootNavigation.SelectedPageIndex;
                        current--;
                        if (current == 0) _navigationService.Navigate(typeof(Views.Pages.DashboardPage));
                        else if (current == 1) _navigationService.Navigate(typeof(Views.Pages.AdvancedPage));
                        else if (current == 2) _navigationService.Navigate(typeof(Views.Pages.DisplaySettings));
                        else if (current == 3) _navigationService.Navigate(typeof(Views.Pages.FanControl));
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) && this.Visibility != Visibility.Hidden && Global.shortCut == false)
                    {
                        int current = RootNavigation.SelectedPageIndex;
                        current++;
                        if (current == 0) _navigationService.Navigate(typeof(Views.Pages.DashboardPage));
                        else if (current == 1) _navigationService.Navigate(typeof(Views.Pages.AdvancedPage));
                        else if (current == 2) _navigationService.Navigate(typeof(Views.Pages.DisplaySettings));
                        else if (current == 3) _navigationService.Navigate(typeof(Views.Pages.FanControl));
                    }
                }

                if (mbo.Contains("aya") && controllerNo == UserIndex.One)
                {
                    //detect if keyboard or controller combo is being activated
                    if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.F12) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.F12) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.LWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.LWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.RWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.RWin) & KeyStates.Down) > 0)
                    {
                        if (Visibility == Visibility.Visible)
                        {
                            Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            Visibility = Visibility.Visible;
                            this.Activate();
                            this.Focus();
                            SetWindowPosition();
                            UpdateLayout();
                        }
                    }
                }

                Global._appVis = this.Visibility;
            }
            catch { }
        }

        void WatcherEventArrived(object sender, EventArrivedEventArgs e)
        {

            var collection = (ManagementEventWatcher)sender;

            if (e.NewEvent is null) return;

            int EventID = int.Parse(e.NewEvent["EventID"].ToString());

            switch (EventID)
            {
                case 166:
                case 56:    // Rog button
                    if (Visibility == Visibility.Visible)
                    {
                        Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        Visibility = Visibility.Visible;
                        this.Activate();
                        this.Focus();
                        SetWindowPosition();
                        UpdateLayout();
                    }

                    UpdateLayout();
                    break;
                case 174:   // FN+F5
                    break;
                case 179:   // FN+F4
                    break;
            }
        }
        bool isControllerEmu = false;
        async void Mouse_Tick(object sender, EventArgs e)
        {
            if (Settings.Default.isMouse == true)
            {
                try
                {
                    controller = new Controller(UserIndex.One);

                    bool connected = controller.IsConnected;

                    if (connected)
                    {
                        if (controller.IsConnected && Settings.Default.isMouse == true && this.Visibility == Visibility.Hidden)
                        {
                            controller.GetState(out var state);
                            MouseControl.Movement(state);
                            MouseControl.Scroll(state);
                            MouseControl.LeftButton(state);
                            MouseControl.RightButton(state);
                        }
                    }
                }
                catch { }
            }
        }
        static double lastWifi = 0;
        public static async Task<double> RetrieveSignalStrengthAsync()
        {
            try
            {
                var adapters = await WiFiAdapter.FindAllAdaptersAsync();
                foreach (var adapter in adapters)
                {
                    foreach (var network in adapter.NetworkReport.AvailableNetworks)
                    {
                        lastWifi = network.SignalBars;
                        return network.SignalBars;
                    }
                    return 0;
                }
                return 0;
            }
            catch
            {
                return lastWifi;
            }
        }

        public async void GetWifi()
        {
            if (Visibility == Visibility.Visible)
            {
                string wifiURL = "";
                double wifi = await Task.Run(() => RetrieveSignalStrengthAsync().Result);
                wifi = await Task.Run(() => RetrieveSignalStrengthAsync().Result);

                var wifiRadios = await Radio.GetRadiosAsync();
                var wifiRadio = await Task.Run(() => wifiRadios.FirstOrDefault(r => r.Kind == RadioKind.WiFi));
                var internetConnectionProfile = await Task.Run(() => NetworkInformation.GetInternetConnectionProfile());

                if (wifiRadio != null && wifiRadio.State == RadioState.On)
                {
                    if (wifi == 4) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.Wifi124;
                    else if (wifi == 3) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.Wifi224;
                    else if (wifi == 2) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.Wifi324;
                    else if (wifi == 1) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.Wifi424;
                    else ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.GlobeProhibited20;
                }
                else if (CheckEthernetConnection()) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.UsbPlug24;
                else if (wifiRadio != null && wifiRadio.State == RadioState.Off) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.WifiOff24;
                else ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.GlobeProhibited20;
            }
        }

        static bool CheckEthernetConnection()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    return true;
                }
            }

            return false;
        }

        public async void getBatteryTime()
        {
            if (Visibility == Visibility.Visible)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        PowerStatus pwr = System.Windows.Forms.SystemInformation.PowerStatus;
                        //Get battery life

                        var batTime = (float)pwr.BatteryLifeRemaining;

                        bool isCharging = false;

                        if (ViewModel.BatteryIcon == Wpf.Ui.Common.SymbolRegular.BatteryCharge24)
                        {
                            batTime = 0;
                            isCharging = true;
                        }

                        var time = TimeSpan.FromSeconds(batTime);

                        decimal batRate = GetSystemInfo.GetBatteryRate() / 1000;
                        if (isCharging == true && ViewModel.Battery < 100 || !isCharging) AdViewModel.IsDischarge = true;
                        AdViewModel.ChargeRate = $"{batRate.ToString("0.##")}W Charge Rate";

                        if (isCharging == true)
                        {
                            decimal batDesignCap = (GetSystemInfo.ReadFullChargeCapacity() / 1000) / 100 * Settings.Default.chargeLimit;
                            decimal batCurrentCap = GetSystemInfo.ReadRemainingChargeCapacity() / 1000;
                            if (batDesignCap > 0 && batCurrentCap > 0)
                            {
                                //System.Windows.MessageBox.Show(((batDesignCap - batCurrentCap) / batRate).ToString());

                                if (isCharging == true && ViewModel.Battery >= 99)
                                {
                                    AdViewModel.BatteryTime = $"Battery Charged";
                                    AdViewModel.IsDischarge = false;
                                }
                                else if (batRate > 0)
                                {
                                    decimal batteryChargeTime = CalculateBatteryChargeTime(batDesignCap, batCurrentCap, batRate);
                                    AdViewModel.BatteryTime = $"{FormatTime(batteryChargeTime)}";
                                }
                                else AdViewModel.BatteryTime = "Calculating";
                            }
                        }

                        if (isCharging == false) AdViewModel.BatteryTime = $"{time:%h} Hours {time:%m} Minutes Remaining";
                        if (AdViewModel.BatteryTime == "0 Hours 0 Minutes Remaining" && isCharging == true) AdViewModel.BatteryTime = "Battery Charging";
                        if (AdViewModel.BatteryTime == "0 Hours 0 Minutes Remaining" && isCharging == false) AdViewModel.BatteryTime = "Calculating";
                    });
                }
                catch
                { }
            }
        }

        static string FormatTime(decimal hours)
        {
            int totalMinutes = (int)Math.Round(hours * 60);
            int formattedHours = totalMinutes / 60;
            int formattedMinutes = totalMinutes % 60;
            return $"{formattedHours} Hours {formattedMinutes} Minutes Until Charged";
        }

        static decimal CalculateBatteryChargeTime(decimal designCapacity, decimal currentCapacity, decimal chargeRate)
        {
            return (designCapacity - currentCapacity) / chargeRate;
        }


        private static AdaptivePresetManager adaptivePresetManager = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
        private void UpdatePreset(string presetName)
        {
            Global.updatingPreset = true;
            try
            {
                Global.presetName = presetName;

                adaptivePresetManager = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
                AdaptivePreset myPreset = adaptivePresetManager.GetPreset(presetName);

                int MaxCoreCount = 0;
                foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get()) { MaxCoreCount = Convert.ToInt32(item["NumberOfCores"]); }

                AdViewModel.MaxCoreCount = MaxCoreCount;

                if (myPreset != null)
                {
                    AdViewModel.IsTemp = myPreset._isTemp;
                    AdViewModel.TempLimit = myPreset.tempLimit;
                    AdViewModel.IsPower = myPreset._isPower;
                    AdViewModel.PowerLimit = myPreset.powerLimit;
                    AdViewModel.IsUndervolt = myPreset._isUndervolt;
                    AdViewModel.UnderVolt = myPreset.underVolt;
                    AdViewModel.IsMaxClock = myPreset._isMaxClock;
                    AdViewModel.MaxClock = myPreset.maxClock;
                    AdViewModel.IsIGPUClock = myPreset._isIGPUClock;
                    AdViewModel.IGPUClock = myPreset.iGPUClock;
                    AdViewModel.IsEPP = myPreset._isEPP;
                    AdViewModel.EPP = myPreset._EPP;
                    AdViewModel.IsRSR = myPreset._isRSR;
                    AdViewModel.RSR = myPreset._RSR;
                    AdViewModel.IsCoreCount = myPreset._isCoreCount;
                    AdViewModel.CoreCount = myPreset._CoreCount;
                    AdViewModel.IsFPS = myPreset._isFPS;
                    AdViewModel.Fps = myPreset._fps;
                    AdViewModel.IsAdaptiveFPS = myPreset._isAdaptiveFPS;
                    AdViewModel.MinFps = myPreset._minFps;
                    AdViewModel.MaxFps = myPreset._maxFps;
                    AdViewModel.IsAdaptiveTDP = myPreset._isAdaptiveTDP;
                    AdViewModel.IsAdaptiveiGPU = myPreset._isAdaptiveiGPU;
                    AdViewModel.MaxTDP = myPreset._maxTDP;
                    AdViewModel.MaxTemp = myPreset._maxTemp;
                    AdViewModel.MiniGPU = myPreset._miniGPU;
                    AdViewModel.MaxiGPU = myPreset._maxiGPU;
                    AdViewModel.IsAdaptivePerf = myPreset._isAdaptivePerf;
                }

                if (AdViewModel.CoreCount > AdViewModel.MaxCoreCount) AdViewModel.CoreCount = MaxCoreCount;
                if (AdViewModel.MaxTDP < 5) AdViewModel.MaxTDP = 15;
                if (AdViewModel.MaxTemp < 5) AdViewModel.MaxTemp = 95;
                if (AdViewModel.MaxiGPU < 5) AdViewModel.MaxiGPU = 1800;
                if (AdViewModel.MiniGPU < 5) AdViewModel.MiniGPU = 400;
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }

            Global.updatingPreset = false;
        }
        string lastCPU = "";
        string lastCO = "";
        string lastiGPU = "";
        public static int CPUTemp, CPULoad, CPUClock, CPUPower, GPULoad = 99, GPUClock = 800, GPUMemClock = 800, coreCount = 0, newMinCPUClock = 2300;

        private void UiWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                UpdateInfo();
                GetWifi();
                getBatteryTime();
            }

            Garbage.Garbage_Collect();
        }

        private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Family.TYPE != Family.ProcessorType.Intel) SMUCommands.RyzenAccess.Deinitialize();
            if (Fan_Control.isSupported) Fan_Control.disableFanControl();
        }

        bool started = false;
        int runs = 0;

        static Queue<double> fpsQueue = new Queue<double>();
        static int maxQueueSize = 8;
        static bool tdp, iGPU;
        static int minCPUClock = 2150;
        static int fps = 0;
        static int fpsLimit = 0;
        static int minTDP = -1;
        static double averageFps = 0;
        int numRuns = 0;
        bool updateAdaptive = true;
        private async void Sensor_Tick(object sender, EventArgs e)
        {
            try
            {
                tdp = AdViewModel.IsAdaptiveTDP;
                iGPU = AdViewModel.IsAdaptiveiGPU;
                coreCount = AdViewModel.MaxCoreCount;

                if (tdp)
                {
                    await Task.Run(() =>
                    {
                        if (started == false) GetSensor.openSensor();
                        //GetSensor.updateSensor();
                        GetSensor.updateCPU = true;
                        GetSensor.updateAMDGPU = true;
                        if (Family.TYPE == Family.ProcessorType.Intel) CPUTemp = (int)GetSensor.getCPUInfo(SensorType.Temperature, "Package");
                        else CPUTemp = (int)GetSensor.getCPUInfo(SensorType.Temperature, "Core");
                        CPULoad = (int)GetSensor.getCPUInfo(SensorType.Load, "Total");

                        int core = 1;
                        do
                        {
                            if (core <= coreCount) CPUClock = CPUClock + (int)GetSensor.getCPUInfo(SensorType.Clock, $"Core #{core}");
                            core++;
                        }
                        while (core <= coreCount);

                        CPUClock = (int)(CPUClock / coreCount);

                        if (CPULoad < (100 / coreCount) + 5) newMinCPUClock = minCPUClock + 500;
                        else newMinCPUClock = minCPUClock;

                        //MessageBox.Show(CPUClock.ToString());

                        //CPUPower = (int)GetSensor.getCPUInfo(SensorType.Power, "Package");
                    });


                    if (runs < 4)
                    {
                        CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, AdViewModel.MaxTDP, (int)(AdViewModel.MaxTDP / 2), AdViewModel.MaxTemp);
                        CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, AdViewModel.MaxTDP, (int)(AdViewModel.MaxTDP / 2), AdViewModel.MaxTemp);
                        CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, AdViewModel.MaxTDP, (int)(AdViewModel.MaxTDP / 2), AdViewModel.MaxTemp);
                        runs++;
                    }
                    else
                    {

                        if (i > 6)
                        {
                            GPULoad = ADLXBackend.GetGPUMetrics(0, 7);
                            GPUClock = ADLXBackend.GetGPUMetrics(0, 0);
                            GPUMemClock = ADLXBackend.GetGPUMetrics(0, 1);
                            i = -1;
                        }

                        if (RTSS.directoryRTSSExists() == true && RTSS.RTSSRunning() == true && AdViewModel.IsAdaptivePerf == true)
                        {
                            bool exists = true;

                            await Task.Run(() =>
                            {
                                var osdEntries = OSD.GetOSDEntries();

                                fpsLimit = RTSS.fps;

                                foreach (var flag in RunningGames.appFlags)
                                {
                                    var appEntries = OSD.GetAppEntries(flag).Where(x => (x.Flags & AppFlags.MASK) != AppFlags.None).ToArray();
                                    foreach (var app in appEntries)
                                    {
                                        fps = (int)app.InstantaneousFrames;
                                    }
                                }

                                // Enqueue the current FPS into the queue
                                fpsQueue.Enqueue(fps);

                                // If the queue size exceeds the maximum, dequeue the oldest value
                                if (fpsQueue.Count > maxQueueSize)
                                {
                                    fpsQueue.Dequeue();
                                }

                                // Calculate the average FPS over the time window
                                averageFps = CalculateAverageFps(fpsQueue);

                                if (averageFps == fpsLimit && fpsQueue.Count == (maxQueueSize - 1))
                                {
                                    fpsLimit--;
                                    if (fpsQueue.Count == maxQueueSize) fpsQueue.Clear();
                                }

                                //exists = IsProcessRunning(appId);
                            });

                            if (CPUControl._currentPowerLimit >= 45) minCPUClock = 3600;
                            else if (CPUControl._currentPowerLimit >= 38) minCPUClock = 3200;
                            else if (CPUControl._currentPowerLimit >= 33) minCPUClock = 3000;
                            else if (CPUControl._currentPowerLimit >= 29 || CPUControl._currentPowerLimit <= 32) minCPUClock = 2800;
                            else if (CPUControl._currentPowerLimit >= 26 || CPUControl._currentPowerLimit <= 28) minCPUClock = 2650;
                            else if (CPUControl._currentPowerLimit >= 23 || CPUControl._currentPowerLimit <= 25) minCPUClock = 2500;
                            else if (CPUControl._currentPowerLimit >= 21 || CPUControl._currentPowerLimit <= 22) minCPUClock = 2350;
                            else if (CPUControl._currentPowerLimit >= 18 || CPUControl._currentPowerLimit <= 20) minCPUClock = 2200;
                            else if (CPUControl._currentPowerLimit >= 15 || CPUControl._currentPowerLimit <= 17) minCPUClock = 1950;
                            else if (CPUControl._currentPowerLimit >= 12 || CPUControl._currentPowerLimit <= 14) minCPUClock = 1750;
                            else if (CPUControl._currentPowerLimit >= 8 || CPUControl._currentPowerLimit <= 11) minCPUClock = 1650;

                            if (AdViewModel.MaxTDP >= 35) minTDP = AdViewModel.MaxTDP - 12;
                            else if (AdViewModel.MaxTDP >= 25) minTDP = AdViewModel.MaxTDP - 9;
                            else if (AdViewModel.MaxTDP > 21) minTDP = AdViewModel.MaxTDP - 7;
                            else if (AdViewModel.MaxTDP >= 15 && AdViewModel.MaxTDP <= 20) minTDP = AdViewModel.MaxTDP - 6;
                            else if (AdViewModel.MaxTDP >= 10) minTDP = AdViewModel.MaxTDP - 5;
                            if (minTDP < 5) minTDP = 5;

                            if (updateAdaptive || fps < fpsLimit)
                            {
                                if (exists && AdViewModel.IsAdaptiveFPS == true || exists && AdViewModel.IsFPS == true)
                                {
                                    if (averageFps != fpsLimit) CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, AdViewModel.MaxTDP, minTDP, AdViewModel.MaxTemp, fps, fpsLimit);

                                    if (iGPU) iGPUControl.UpdateiGPUClock(AdViewModel.MaxiGPU, AdViewModel.MiniGPU, AdViewModel.MaxTemp, CPUPower, CPUTemp, GPUClock, GPULoad, GPUMemClock, CPUClock, newMinCPUClock, fps, fpsLimit);
                                }
                                else
                                {
                                    CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, AdViewModel.MaxTDP, minTDP, AdViewModel.MaxTemp);

                                    if (iGPU) iGPUControl.UpdateiGPUClock(AdViewModel.MaxiGPU, AdViewModel.MiniGPU, AdViewModel.MaxTemp, CPUPower, CPUTemp, GPUClock, GPULoad, GPUMemClock, CPUClock, newMinCPUClock, -1, -1);
                                }

                                updateAdaptive = false;
                            }
                            else updateAdaptive = true;
                        }
                        if(AdViewModel.IsAdaptivePerf == false)
                        {
                            if (updateAdaptive)
                            {
                                CPUControl.UpdatePowerLimit(CPUTemp, CPULoad, AdViewModel.MaxTDP, minTDP, AdViewModel.MaxTemp);

                                if (iGPU) iGPUControl.UpdateiGPUClock(AdViewModel.MaxiGPU, AdViewModel.MiniGPU, AdViewModel.MaxTemp, CPUPower, CPUTemp, GPUClock, GPULoad, GPUMemClock, CPUClock, newMinCPUClock, -1, -1);
                                
                                updateAdaptive = false;
                            }
                            else updateAdaptive = true;
                        }
                    }
                }
                else if (started && !tdp)
                {
                    GetSensor.closeSensor();
                    started = false;
                }
                i++;
            }
            catch (Exception ex) { }
        }

        private async void adaptiveTDP_iGPU()
        {
            try
            {
                if (tdp)
                {
                    string commandString = "";
                    if (CPUControl.cpuCommand != lastCPU && Family.TYPE != Family.ProcessorType.Intel)
                    {
                        commandString = commandString + CPUControl.cpuCommand;
                        lastCPU = CPUControl.cpuCommand;
                    }

                    if (iGPUControl.commmand != null && iGPUControl.commmand != "" && iGPU && iGPUControl.commmand != lastiGPU)
                    {
                        commandString = commandString + iGPUControl.commmand;
                        lastiGPU = iGPUControl.commmand;
                    }

                    if (commandString != null && commandString != "" && Family.TYPE != Family.ProcessorType.Intel) RyzenAdj_To_UXTU.Translate(commandString);
                }

            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
        }

        static double CalculateAverageFps(Queue<double> fpsQueue)
        {
            double sumFps = 0.0;

            foreach (double fps in fpsQueue)
            {
                sumFps += fps;
            }

            return sumFps / fpsQueue.Count;
        }

        static bool IsProcessRunning(int processId)
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.Id == processId)
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetRadeonGPUCount()
        {
            int count = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"] as string;
                    if (name != null && name.Contains("Radeon"))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static int GetNVIDIAGPUCount()
        {
            int count = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string name = obj["Name"] as string;
                    if (name != null && name.Contains("NVIDIA"))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {

            if (e.Mode == PowerModes.StatusChange)
            {
                await Task.Run(() => getBattery());
                await Task.Run(() => getBattery());

                if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                {
                    if (Settings.Default.DisplayRes >= 0) Display.ApplySettings(Display.uniqueResolutions[Settings.Default.DisplayRes], Display.uniqueRefreshRates[Settings.Default.DisplayPlugHz]);
                }
                else
                {
                    if (Settings.Default.DisplayRes >= 0) Display.ApplySettings(Display.uniqueResolutions[Settings.Default.DisplayRes], Display.uniqueRefreshRates[Settings.Default.DisplayBatHz]);
                }
            }

        }

        static UInt16 statuscode = 0;
        public static void getBattery()
        {
            int i = 0;
            do
            {
                try
                {
                    ManagementClass wmi = new ManagementClass("Win32_Battery");
                    ManagementObjectCollection allBatteries = wmi.GetInstances();

                    //Get battery level from each system battery detected
                    foreach (var battery in allBatteries)
                    {
                        statuscode = (UInt16)battery["BatteryStatus"];
                    }

                    i++;
                }
                catch (Exception ex)
                {

                }
            } while (i < 2);
        }
    }
}