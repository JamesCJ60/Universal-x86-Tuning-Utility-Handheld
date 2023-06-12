using Microsoft.Win32;
using SharpDX.XInput;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility_Handheld.Properties;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Windows.Devices.Power;
using Windows.Devices.Radios;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Application = System.Windows.Application;

namespace Universal_x86_Tuning_Utility_Handheld.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public ViewModels.MainWindowViewModel ViewModel
        {
            get;
        }

        private DispatcherTimer timer;

        public MainWindow(ViewModels.MainWindowViewModel viewModel, IPageService pageService, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            SetPageService(pageService);

            navigationService.SetNavigationControl(RootNavigation);
            SetWindowPosition();
            Loaded += MainWindow_Loaded;
            Global._mainWindowNav = RootNavigation;

            //set up timer for key combo system
            DispatcherTimer checkKeyInput = new DispatcherTimer();
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.14);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();

            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Tick += Mouse_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(1000 / MouseControl.RefreshRate);
            _timer.Start();

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            UpdateInfo();
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            SetWindowPosition();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            Wpf.Ui.Appearance.Watcher.Watch(this, Wpf.Ui.Appearance.BackgroundType.Acrylic, true);                                   
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timer.Interval == TimeSpan.FromSeconds(1))
            {
                timer.Stop();
                HideFromTaskbar();
                //MessageBox.Show(RootNavigation.Current.PageTag);

                timer.Interval = TimeSpan.FromSeconds(2.2);
                timer.Start();
            }
            else
            {
                UpdateInfo();
            }
        }

        public void UpdateInfo()
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            int batteryLifePercent = (int)(powerStatus.BatteryLifePercent * 100);
            ViewModel.Battery = batteryLifePercent;
            ViewModel.Time = DateTime.Now;
            bool isBatteryCharging = powerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online && powerStatus.BatteryChargeStatus.HasFlag(BatteryChargeStatus.Charging);

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
        }

        private void HideFromTaskbar()
        {
            ShowInTaskbar = false;
        }

        private void SetWindowPosition()
        {
            this.MinWidth = 525;
            this.Width = 525;

            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            var workArea = screen.WorkingArea;
            this.Left = workArea.Right - this.Width - 12;
            this.Top = workArea.Top + 12;
            this.Height = workArea.Height - 24;
            this.ResizeMode = ResizeMode.NoResize;

            // Force a layout update
            this.UpdateLayout();
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
            Application.Current.Shutdown();
        }

        private void UiWindow_LocationChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal) this.Visibility = Visibility.Visible;

            if (this.WindowState == WindowState.Minimized) this.Visibility = Visibility.Hidden;

            SetWindowPosition();
        }

        private void NotifyIcon_LeftClick(Wpf.Ui.Controls.NotifyIcon sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                this.WindowState = WindowState.Minimized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
        }

        private void UiWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
            else
            {
                this.ShowInTaskbar = true;
            }

            Global._appState = this.WindowState;
        }

        private void TitleBar_MinimizeClicked(object sender, RoutedEventArgs e)
        {
            this.WindowStyle = WindowStyle.None;
        }

        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            ControllerInput(UserIndex.One);
            ControllerInput(UserIndex.Two);
        }

        private static Controller controller;
        private void ControllerInput(UserIndex controllerNo)
        {
            try
            {
                controller = new Controller(controllerNo);

                bool connected = controller.IsConnected;

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
                        if (this.WindowState != WindowState.Minimized)
                        {
                            this.WindowState = WindowState.Minimized;
                        }
                        else
                        {
                            this.WindowState = WindowState.Normal;
                            this.Activate();
                        }
                    }
                }

                if (App.mbo.Contains("aya") && controllerNo == UserIndex.One)
                {
                    //detect if keyboard or controller combo is being activated
                    if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.F12) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.F12) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.LWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.LWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.RWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.RWin) & KeyStates.Down) > 0)
                    {
                        if (this.WindowState != WindowState.Minimized)
                        {
                            this.WindowState = WindowState.Minimized;
                        }
                        else
                        {
                            this.WindowState = WindowState.Normal;
                            this.Activate();
                        }
                    }
                }
            }
            catch { }
        }

        async void Mouse_Tick(object sender, EventArgs e)
        {
            try
            {
                controller = new Controller(UserIndex.One);

                bool connected = controller.IsConnected;

                if (connected)
                {
                    if (controller.IsConnected && Settings.Default.isMouse == true && this.WindowState == WindowState.Minimized)
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
        public static async Task<double> RetrieveSignalStrengthAsync()
        {
            try
            {
                var adapters = await WiFiAdapter.FindAllAdaptersAsync();
                foreach (var adapter in adapters)
                {
                    foreach (var network in adapter.NetworkReport.AvailableNetworks)
                    {
                        return network.SignalBars;
                    }
                    return 0;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async void GetWifi()
        {
            string wifiURL = "";
            double wifi = await Task.Run(() => RetrieveSignalStrengthAsync().Result);

            var wifiRadios = await Radio.GetRadiosAsync();
            var wifiRadio = await Task.Run(() => wifiRadios.FirstOrDefault(r => r.Kind == RadioKind.WiFi));
            var internetConnectionProfile = await Task.Run(() => NetworkInformation.GetInternetConnectionProfile());

            if (wifiRadio != null && wifiRadio.State == RadioState.On)
            {
                if (wifi >= 4) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.Wifi124;
                else if (wifi >= 3) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.Wifi224;
                else if (wifi >= 2) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.Wifi324;
                else if (wifi == 1) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.Wifi424;
                else ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.GlobeProhibited20;

            }
            else if (wifiRadio != null && wifiRadio.State == RadioState.Off) ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.WifiOff24;
            else if (internetConnectionProfile != null)
            {
                var interfaceType = internetConnectionProfile.NetworkAdapter.IanaInterfaceType;

                if (interfaceType == 71)
                {
                    ViewModel.WifiIcon = Wpf.Ui.Common.SymbolRegular.UsbPlug24;
                }
            }

        }
    }
}