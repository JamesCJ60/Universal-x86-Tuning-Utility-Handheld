using SharpDX.XInput;
using System;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Universal_x86_Tuning_Utility_Handheld.Views.Windows;
using Windows.Devices.Radios;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;
using NAudio.CoreAudioApi;
using System.Threading.Tasks;
using System.Media;
using System.Linq;
using Universal_x86_Tuning_Utility_Handheld.Properties;
using Microsoft.Win32.TaskScheduler;
using Task = System.Threading.Tasks.Task;
using Wpf.Ui.Common;
using Universal_x86_Tuning_Utility_Handheld.Scripts;
using Universal_x86_Tuning_Utility.Scripts.Misc;

namespace Universal_x86_Tuning_Utility_Handheld.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>
    {
        private Brush normalBorderBrush;
        private Brush selectedBorderBrush = Brushes.White;
        Thickness normalThickness = new Thickness(1);
        Thickness selectedThickness = new Thickness(2.5);
        public ViewModels.DashboardViewModel ViewModel
        {
            get; set;
        }
        private DispatcherTimer checkInput = new DispatcherTimer();
        public DashboardPage(ViewModels.DashboardViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            _ = Tablet.TabletDevices;

            getBrightness();
            getVol();
            getWifi();
            getBluetooth();
            UpdateASUS();
            SetRecordingDeviceState(ViewModel.Recording);

            normalBorderBrush = ccSection8.BorderBrush;
            checkInput.Interval = TimeSpan.FromSeconds(0.12);
            checkInput.Tick += checkInput_Tick;
            checkInput.Start();

            Garbage.Garbage_Collect();
        }

        int selected = 0, lastSelected = 0;
        bool wasMini = true;
        async void checkInput_Tick(object sender, EventArgs e)
        {
            if (Global._mainWindowNav.SelectedPageIndex == 0 && Global._appVis == Visibility.Visible && Global.shortCut == false)
            {
                UpdateGUI(UserIndex.One);
                UpdateGUI(UserIndex.Two);

                var foregroundBrush = (Brush)Application.Current.FindResource("TextFillColorPrimaryBrush");
                selectedBorderBrush = foregroundBrush;
            }
            else wasMini = true;
        }

        private static Controller controller;
        private void UpdateGUI(UserIndex controllerNo)
        {
            try
            {
                if (wasMini)
                {
                    getBrightness();
                    getVol();
                    getWifi();
                    getBluetooth();
                    wasMini = false;
                }

                CardControl[] cards = { ccSection1, ccSection2, ccSection3, ccSection4, ccSection5, ccSection6, ccSection7, ccSection8, ccSection9, ccSection10, ccClose };
                controller = new Controller(controllerNo);
                bool connected = controller.IsConnected;

                if (connected)
                {
                    //get controller state
                    var state = controller.GetState();
                    SharpDX.XInput.Gamepad gamepad = controller.GetState().Gamepad;
                    float tx = gamepad.LeftThumbX;
                    float ty = gamepad.LeftThumbY;

                    ScrollViewer svMain = Global.FindVisualChild<ScrollViewer>(this);

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) || ty > 18000)
                    {
                        if (selected > 0) selected--;
                        else selected = 0;

                        if (cards[selected].Visibility == Visibility.Collapsed)
                        {
                            do selected--;
                            while (cards[selected].Visibility == Visibility.Collapsed);

                            if (selected < 0) selected = lastSelected;
                        }

                        GeneralTransform transform = cards[selected].TransformToAncestor(svMain);
                        System.Windows.Point topPosition = transform.Transform(new System.Windows.Point(0, 0));
                        System.Windows.Point bottomPosition = transform.Transform(new System.Windows.Point(0, cards[selected].ActualHeight));

                        // Check if the border is not fully visible in the current viewport
                        if (topPosition.Y < svMain.VerticalOffset || bottomPosition.Y > svMain.VerticalOffset + svMain.ViewportHeight)
                        {
                            // Scroll to the position of the top of the border
                            svMain.ScrollToVerticalOffset(topPosition.Y);
                        }

                        if (selected <= 1) svMain.ScrollToTop();
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) || ty < -18000)
                    {
                        if (selected < cards.Length - 1) selected++;
                        else selected = cards.Length - 1;

                        if (cards[selected].Visibility == Visibility.Collapsed)
                        {
                            do selected++;
                            while (cards[selected].Visibility == Visibility.Collapsed);

                            if (selected > cards.Length - 1) selected = lastSelected;
                        }

                        GeneralTransform transform = cards[selected].TransformToAncestor(svMain);
                        System.Windows.Point topPosition = transform.Transform(new System.Windows.Point(0, 0));
                        System.Windows.Point bottomPosition = transform.Transform(new System.Windows.Point(0, cards[selected].ActualHeight));

                        // Check if the border is not fully visible in the current viewport
                        if (topPosition.Y < svMain.VerticalOffset || bottomPosition.Y > svMain.VerticalOffset + svMain.ViewportHeight)
                        {
                            // Scroll to the position of the top of the border
                            svMain.ScrollToVerticalOffset(bottomPosition.Y);
                        }

                        if (selected >= cards.Length - 2) svMain.ScrollToBottom();
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || tx < -26000)
                    {
                        Slider slider = Global.FindVisualChild<Slider>(cards[selected]);

                        if (slider != null)
                        {
                            int currentValue = (int)slider.Value;
                            currentValue--;
                            if (currentValue < slider.Minimum) currentValue = (int)slider.Minimum;
                            if (currentValue > slider.Maximum) currentValue = (int)slider.Maximum;
                            slider.Value = currentValue;
                        }

                        ToggleSwitch toggleSwitch = Global.FindVisualChild<ToggleSwitch>(cards[selected]);

                        if (toggleSwitch != null)
                        {
                            toggleSwitch.IsChecked = false;
                        }
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
                    {
                        ToggleSwitch toggleSwitch = Global.FindVisualChild<ToggleSwitch>(cards[selected]);

                        if (toggleSwitch != null)
                        {
                            if(toggleSwitch.IsChecked == true) toggleSwitch.IsChecked = false;
                            else toggleSwitch.IsChecked = true;
                        }

                        if (cards[selected] == ccClose) Environment.Exit(0);
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || tx > 26000)
                    {
                        Slider slider = Global.FindVisualChild<Slider>(cards[selected]);

                        if (slider != null)
                        {
                            int currentValue = (int)slider.Value;
                            currentValue++;
                            if (currentValue < slider.Minimum) currentValue = (int)slider.Minimum;
                            if (currentValue > slider.Maximum) currentValue = (int)slider.Maximum;
                            slider.Value = currentValue;
                        }

                        ToggleSwitch toggleSwitch = Global.FindVisualChild<ToggleSwitch>(cards[selected]);

                        if (toggleSwitch != null)
                        {
                            toggleSwitch.IsChecked = true;
                        }
                    }

                    if (selected != lastSelected && cards[selected].Visibility != Visibility.Collapsed && cards[selected].BorderBrush != selectedBorderBrush)
                    {
                        if (selected < 0) selected = 0;
                        if (selected > cards.Length - 1) selected = cards.Length - 1;

                        foreach (var card in cards)
                        {
                            card.BorderBrush = normalBorderBrush;
                            card.BorderThickness = normalThickness;
                        }

                        cards[selected].BorderBrush = selectedBorderBrush;
                        cards[selected].BorderThickness = selectedThickness;
                        lastSelected = selected;
                    }
                    else if (selected == lastSelected && cards[selected].Visibility == Visibility.Collapsed)
                    {
                        if(selected > cards.Length / 2)
                        {
                            if (cards[selected].Visibility == Visibility.Collapsed)
                            {
                                do selected--;
                                while (cards[selected].Visibility == Visibility.Collapsed);

                                if (selected < 0) selected = lastSelected;
                            }
                        }
                        else
                        {
                            if (cards[selected].Visibility == Visibility.Collapsed)
                            {
                                do selected++;
                                while (cards[selected].Visibility == Visibility.Collapsed);

                                if (selected > cards.Length - 1) selected = lastSelected;
                            }
                        }
                    }
                }
                else if (controllerNo == UserIndex.One && !connected)
                {
                    foreach (var card in cards)
                    {
                        card.BorderBrush = normalBorderBrush;
                        card.BorderThickness = normalThickness;
                    }
                    lastSelected = -1;
                }
            } catch { }
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            SetWifiEnabled();
            SetBluetoothEnabled();
            SetRecordingDeviceState(ViewModel.Recording);

            using (TaskService ts = new TaskService())
            {
                if (ts.RootFolder.AllTasks.Any(t => t.Name == "UXTU Handheld"))
                {
                    // Remove the task we just created
                    ts.RootFolder.DeleteTask("UXTU Handheld");
                }
            }

            if (ViewModel.StartOnBoot == true)
            {
                // Get the service on the local machine
                using (TaskService ts = new TaskService())
                {
                    if (!ts.RootFolder.AllTasks.Any(t => t.Name == "UXTU Handheld"))
                    {
                        // Create a new task definition and assign properties
                        TaskDefinition td = ts.NewTask();
                        td.Principal.RunLevel = TaskRunLevel.Highest;
                        td.RegistrationInfo.Description = "Start UXTU Handheld";
                        td.Settings.DisallowStartIfOnBatteries = false;

                        // Create a trigger that will fire the task at this time every other day
                        td.Triggers.Add(new LogonTrigger());

                        string path = System.Reflection.Assembly.GetEntryAssembly().Location;
                        path = path.Replace("Universal x86 Tuning Utility Handheld.dll", "Universal x86 Tuning Utility Handheld.exe");

                        // Create an action that will launch app
                        td.Actions.Add(path);

                        // Register the task in the root folder
                        ts.RootFolder.RegisterTaskDefinition(@"UXTU Handheld", td);
                    }
                }
            }

            Settings.Default.isMute = ViewModel.Recording;
            Settings.Default.isRTSS = ViewModel.Overlay;
            Settings.Default.isMouse = ViewModel.Mouse;
            Settings.Default.StartOnBoot = ViewModel.StartOnBoot;
            Settings.Default.StartMini = ViewModel.StartMini;
            Settings.Default.Save();
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            SetWifiEnabled();
            SetBluetoothEnabled();
            SetRecordingDeviceState(ViewModel.Recording);

            using (TaskService ts = new TaskService())
            {
                if (ts.RootFolder.AllTasks.Any(t => t.Name == "UXTU Handheld"))
                {
                    // Remove the task we just created
                    ts.RootFolder.DeleteTask("UXTU Handheld");
                }
            }

            Settings.Default.isMute = ViewModel.Recording;
            Settings.Default.isRTSS = ViewModel.Overlay;
            Settings.Default.isMouse = ViewModel.Mouse;
            Settings.Default.StartOnBoot = ViewModel.StartOnBoot;
            Settings.Default.StartMini = ViewModel.StartMini;
            Settings.Default.Save();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateBrightness(ViewModel.Brightness);
            updateVolume(ViewModel.Volume);
            UpdateASUS();
        }

        private async void UpdateASUS()
        {
            await Task.Run(() =>
            {
                if (ViewModel.AcMode == 0)
                {
                    ViewModel.ACMode = "Silent Mode";
                    ViewModel.AcModeIcon = SymbolRegular.LeafTwo24;
                    if (Settings.Default.isASUS) App.wmi.DeviceSet(ASUSWmi.PerformanceMode, ASUSWmi.PerformanceSilent);
                }
                if (ViewModel.AcMode == 1)
                {
                    ViewModel.ACMode = "Perf Mode";
                    ViewModel.AcModeIcon = SymbolRegular.Scales24;
                    if (Settings.Default.isASUS) App.wmi.DeviceSet(ASUSWmi.PerformanceMode, ASUSWmi.PerformanceBalanced);
                }
                if (ViewModel.AcMode == 2)
                {
                    ViewModel.ACMode = "Turbo Mode";
                    ViewModel.AcModeIcon = SymbolRegular.Gauge24;
                    if (Settings.Default.isASUS) App.wmi.DeviceSet(ASUSWmi.PerformanceMode, ASUSWmi.PerformanceTurbo);
                }
            });

            Settings.Default.acMode = ViewModel.AcMode;
            Settings.Default.Save();
        }

        async void SetRecordingDeviceState(bool mute)
        {
            try
            {
                await Task.Run(() =>
                {
                    using var enumerator = new MMDeviceEnumerator();
                    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

                    foreach (var device in devices)
                    {
                        device.AudioEndpointVolume.Mute = mute;
                    }

                    if(mute == true) ViewModel.MicIcon = SymbolRegular.MicOff24;
                    else ViewModel.MicIcon = SymbolRegular.Mic24;
                });
            }
            catch { }
        }

        public async void updateBrightness(int newBirghtness)
        {
            try
            {
                await Task.Run(() =>
                {
                    var mclass = new ManagementClass("WmiMonitorBrightnessMethods")
                    {
                        Scope = new ManagementScope(@"\\.\root\wmi")
                    };
                    var instances = mclass.GetInstances();
                    var args = new object[] { 1, newBirghtness };
                    foreach (ManagementObject instance in instances)
                    {
                        instance.InvokeMethod("WmiSetBrightness", args);
                    }
                });

                if (ViewModel.Brightness >= 50) ViewModel.BrightIcon = SymbolRegular.BrightnessHigh24;
                else ViewModel.BrightIcon = SymbolRegular.BrightnessLow24;
            }
            catch { }
        }

        public async void updateVolume(int newVolume)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Get the default audio playback device
                    MMDevice defaultDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);

                    //Set volume of current sound device
                    defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)newVolume / 100.0f;
                });

                if (ViewModel.Volume >= 50) ViewModel.VolIcon = SymbolRegular.Speaker224;
                else if (ViewModel.Volume > 0) ViewModel.VolIcon = SymbolRegular.Speaker124;
                else ViewModel.VolIcon = SymbolRegular.SpeakerMute24;
            }
            catch { }
        }

        private async Task SetWifiEnabled()
        {
            try
            {
                // Check if Wi-Fi is enabled
                var wifiRadios = await Radio.GetRadiosAsync();
                var wifiRadio = wifiRadios.FirstOrDefault(r => r.Kind == RadioKind.WiFi);
                await wifiRadio.SetStateAsync(ViewModel.Wifi ? RadioState.On : RadioState.Off);

                if (ViewModel.Wifi) ViewModel.WifiIcon = SymbolRegular.Wifi120;
                else ViewModel.WifiIcon = SymbolRegular.WifiOff24;
            }
            catch { }
        }

        private async Task SetBluetoothEnabled()
        {
            try
            {
                // Check if Bluetooth is enabled
                var bluetoothRadios = await Radio.GetRadiosAsync();
                var bluetoothRadio = bluetoothRadios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
                await bluetoothRadio.SetStateAsync(ViewModel.Bluetooth ? RadioState.On : RadioState.Off);
                if (ViewModel.Bluetooth) ViewModel.BlueIcon = SymbolRegular.Bluetooth24;
                else ViewModel.BlueIcon = SymbolRegular.BluetoothDisabled24;
            }
            catch { }
        }

        private async void getWifi()
        {
            try
            {
                // Check if Wi-Fi is enabled
                var wifiRadios = await Radio.GetRadiosAsync();
                var wifiRadio = wifiRadios.FirstOrDefault(r => r.Kind == RadioKind.WiFi);
                bool isWifiEnabled = (wifiRadio != null && wifiRadio.State == RadioState.On);

                ViewModel.Wifi = isWifiEnabled;
                if (ViewModel.Wifi) ViewModel.WifiIcon = SymbolRegular.Wifi120;
                else ViewModel.WifiIcon = SymbolRegular.WifiOff24;
            }
            catch { }
        }

        private async void getBluetooth()
        {
            try
            {
                // Check if Bluetooth is enabled
                var bluetoothRadios = await Radio.GetRadiosAsync();
                var bluetoothRadio = bluetoothRadios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
                bool isBluetoothEnabled = (bluetoothRadio != null && bluetoothRadio.State == RadioState.On);
                ViewModel.Bluetooth = isBluetoothEnabled;
                if (ViewModel.Bluetooth) ViewModel.BlueIcon = SymbolRegular.Bluetooth24;
                else ViewModel.BlueIcon = SymbolRegular.BluetoothDisabled24;
            }
            catch { }
        }

        public async void getBrightness()
        {
            try
            {
                await Task.Run(() =>
                {
                    using var mclass = new ManagementClass("WmiMonitorBrightness")
                    {
                        Scope = new ManagementScope(@"\\.\root\wmi")
                    };
                    using var instances = mclass.GetInstances();
                    foreach (ManagementObject instance in instances)
                    {
                        ViewModel.Brightness = (byte)instance.GetPropertyValue("CurrentBrightness");
                    }
                });

                if (ViewModel.Brightness >= 50) ViewModel.BrightIcon = SymbolRegular.BrightnessHigh24;
                else ViewModel.BrightIcon = SymbolRegular.BrightnessLow24;
            }
            catch { }
        }

        public async void getVol()
        {
            try
            {
                await Task.Run(() =>
                {
                    // Get the default audio playback device
                    MMDevice defaultDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);

                    // Get the current volume level of the device as an integer between 0 and 100
                    ViewModel.Volume = (int)Math.Round(defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0);
                });

                if (ViewModel.Volume >= 50) ViewModel.VolIcon = SymbolRegular.Speaker224;
                else if (ViewModel.Volume > 0) ViewModel.VolIcon = SymbolRegular.Speaker124;
                else ViewModel.VolIcon = SymbolRegular.SpeakerMute24;
            }
            catch { }
        }

        private void SizeSlider_TouchDown(object sender, TouchEventArgs e)
        {
            // Mark event as handled
            e.Handled = true;
        }
    }
}