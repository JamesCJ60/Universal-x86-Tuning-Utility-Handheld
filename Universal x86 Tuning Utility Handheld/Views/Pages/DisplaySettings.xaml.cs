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
using System.Diagnostics;
using Universal_x86_Tuning_Utility_Handheld.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Wpf.Ui.Mvvm.Interfaces;
using System.Windows.Forms;
using Application = System.Windows.Application;
using Windows.Graphics;

namespace Universal_x86_Tuning_Utility_Handheld.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DisplaySettings : INavigableView<ViewModels.DisplaySettingsViewModel>
    {
        private Brush normalBorderBrush;
        private Brush selectedBorderBrush = Brushes.White;
        Thickness normalThickness = new Thickness(1);
        Thickness selectedThickness = new Thickness(2.5);
        public ViewModels.DisplaySettingsViewModel ViewModel
        {
            get; set;
        }
        private DispatcherTimer checkInput = new DispatcherTimer();

        public DisplaySettings(ViewModels.DisplaySettingsViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            _ = Tablet.TabletDevices;

            normalBorderBrush = ccSection3.BorderBrush;
            checkInput.Interval = TimeSpan.FromSeconds(0.12);
            checkInput.Tick += checkInput_Tick;
            checkInput.Start();

            setupGUI();

            Garbage.Garbage_Collect();
        }

        async void setupGUI()
        {
            ViewModel.BatMaxHz = Display.uniqueRefreshRates.Count - 1;
            ViewModel.PlugMaxHz = Display.uniqueRefreshRates.Count - 1;
            ViewModel.DisplayMaxRes = Display.uniqueResolutions.Count - 1;

            if(Settings.Default.DisplayBatHz < 0 || Settings.Default.DisplayBatHz > Display.uniqueRefreshRates.Count - 1) ViewModel.BatCurrentHz = Display.uniqueRefreshRates.Count - 1;
            else ViewModel.BatCurrentHz = Settings.Default.DisplayBatHz;
            if (Settings.Default.DisplayPlugHz < 0 || Settings.Default.DisplayPlugHz > Display.uniqueRefreshRates.Count - 1) ViewModel.PlugCurrentHz = Display.uniqueRefreshRates.Count - 1;
            else ViewModel.PlugCurrentHz = Settings.Default.DisplayPlugHz;
            if (Settings.Default.DisplayRes < 0 || Settings.Default.DisplayRes > Display.uniqueResolutions.Count - 1) ViewModel.DisplayCurrentRes = Display.uniqueResolutions.Count - 1;
            else ViewModel.DisplayCurrentRes = Settings.Default.DisplayRes;

            setup = true;

            ViewModel.DisplayRes = $"{Display.uniqueResolutions[ViewModel.DisplayCurrentRes]}";
            ViewModel.BatHz = $"{Display.uniqueRefreshRates[ViewModel.BatCurrentHz]}Hz";
            ViewModel.PlugHz = $"{Display.uniqueRefreshRates[ViewModel.PlugCurrentHz]}Hz";
        }

        int selected = 0, lastSelected = 0;
        bool setup = false;
        async void checkInput_Tick(object sender, EventArgs e)
        {
            if (Global._mainWindowNav.SelectedPageIndex == 2 && Global._appVis == Visibility.Visible && Global.shortCut == false)
            {
                UpdateGUI(UserIndex.One);
                UpdateGUI(UserIndex.Two);

                var foregroundBrush = (Brush)Application.Current.FindResource("TextFillColorPrimaryBrush");
                selectedBorderBrush = foregroundBrush;
            }
        }

        private static Controller controller;

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (setup)
            {
                ViewModel.DisplayRes = $"{Display.uniqueResolutions[ViewModel.DisplayCurrentRes]}";
                ViewModel.BatHz = $"{Display.uniqueRefreshRates[ViewModel.BatCurrentHz]}Hz";
                ViewModel.PlugHz = $"{Display.uniqueRefreshRates[ViewModel.PlugCurrentHz]}Hz";
            }
        }

        private void Slider_TouchDown(object sender, TouchEventArgs e)
        {
            if (setup)
            {
                ViewModel.DisplayRes = $"{Display.uniqueResolutions[ViewModel.DisplayCurrentRes]}";
                ViewModel.BatHz = $"{Display.uniqueRefreshRates[ViewModel.BatCurrentHz]}Hz";
                ViewModel.PlugHz = $"{Display.uniqueRefreshRates[ViewModel.PlugCurrentHz]}Hz";
            }
        }

        private void UpdateGUI(UserIndex controllerNo)
        {
            try
            {
                CardControl[] cards = { ccSection1, ccSection2, ccSection3, ccApply };
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

                        GeneralTransform transform = cards[selected].TransformToVisual(svMain);
                        Point topPosition = transform.Transform(new Point(0, 0));
                        Point bottomPosition = transform.Transform(new Point(0, cards[selected].ActualHeight));

                        if (topPosition.Y < 0 || bottomPosition.Y > svMain.ActualHeight)
                        {
                            double targetOffset = svMain.VerticalOffset + topPosition.Y - 12;
                            svMain.ScrollToVerticalOffset(targetOffset);
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

                        GeneralTransform transform = cards[selected].TransformToVisual(svMain);
                        Point topPosition = transform.Transform(new Point(0, 0));
                        Point bottomPosition = transform.Transform(new Point(0, cards[selected].ActualHeight));

                        if (topPosition.Y < 0 || bottomPosition.Y > svMain.ActualHeight)
                        {
                            double targetOffset = svMain.VerticalOffset + bottomPosition.Y - svMain.ActualHeight + 12;
                            svMain.ScrollToVerticalOffset(targetOffset);
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

                        if (cards[selected] == ccApply)
                        {
                            PowerStatus powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;
                            bool isBatteryCharging = powerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online;
                            if (isBatteryCharging) Display.ApplySettings(Display.uniqueResolutions[ViewModel.DisplayCurrentRes], Display.uniqueRefreshRates[ViewModel.PlugCurrentHz]);
                            else Display.ApplySettings(Display.uniqueResolutions[ViewModel.DisplayCurrentRes], Display.uniqueRefreshRates[ViewModel.BatCurrentHz]);

                            Settings.Default.DisplayBatHz = ViewModel.BatCurrentHz;
                            Settings.Default.DisplayPlugHz = ViewModel.PlugCurrentHz;
                            Settings.Default.DisplayRes = ViewModel.DisplayCurrentRes;
                            Settings.Default.Save();
                        }
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

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            PowerStatus powerStatus = System.Windows.Forms.SystemInformation.PowerStatus;
            bool isBatteryCharging = powerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online;
            if(isBatteryCharging) Display.ApplySettings(Display.uniqueResolutions[ViewModel.DisplayCurrentRes], Display.uniqueRefreshRates[ViewModel.PlugCurrentHz]);
            else Display.ApplySettings(Display.uniqueResolutions[ViewModel.DisplayCurrentRes], Display.uniqueRefreshRates[ViewModel.BatCurrentHz]);

            Settings.Default.DisplayBatHz = ViewModel.BatCurrentHz;
            Settings.Default.DisplayPlugHz = ViewModel.PlugCurrentHz;
            Settings.Default.DisplayRes = ViewModel.DisplayCurrentRes;
            Settings.Default.Save();
        }
    }
}