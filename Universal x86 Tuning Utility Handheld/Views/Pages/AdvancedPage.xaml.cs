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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Windows.Forms;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Wpf.Ui.Mvvm.Interfaces;
using Universal_x86_Tuning_Utility_Handheld.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Universal_x86_Tuning_Utility.Scripts;

namespace Universal_x86_Tuning_Utility_Handheld.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class AdvancedPage : INavigableView<ViewModels.AdvancedViewModel>
    {
        private Brush normalBorderBrush;
        private Brush selectedBorderBrush = Brushes.White;
        Thickness normalThickness = new Thickness(1);
        Thickness selectedThickness = new Thickness(2.5);
        public ViewModels.AdvancedViewModel ViewModel
        {
            get; set;
        }
        private DispatcherTimer checkInput = new DispatcherTimer();
        private DispatcherTimer updateGUI = new DispatcherTimer();
        public AdvancedPage(ViewModels.AdvancedViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            _ = Tablet.TabletDevices;

            normalBorderBrush = ccSection12.BorderBrush;

            checkInput.Interval = TimeSpan.FromSeconds(0.12);
            checkInput.Tick += checkInput_Tick;
            checkInput.Start();

            updateGUI.Interval = TimeSpan.FromSeconds(2.2);
            updateGUI.Tick += updateGUI_Tick;
            updateGUI.Start();

            ViewModel.Battery = Global.batteryPer;
            ViewModel.BatteryIcon = Global.battery;

            Garbage.Garbage_Collect();
        }

        int selected = 0, lastSelected = -1;
        bool wasMini = true;
        async void checkInput_Tick(object sender, EventArgs e)
        {
            if (Global._mainWindowNav.SelectedPageIndex == 1 && Global._appVis == Visibility.Visible && Global.shortCut == false)
            {
                UpdateGUI(UserIndex.One);
                UpdateGUI(UserIndex.Two);

                var foregroundBrush = (Brush)System.Windows.Application.Current.FindResource("TextFillColorPrimaryBrush");
                selectedBorderBrush = foregroundBrush;
            }
        }

        async void updateGUI_Tick(object sender, EventArgs e)
        {
            ViewModel.Battery = Global.batteryPer;
            ViewModel.BatteryIcon = Global.battery;
        }

        private static Controller controller;
        private void UpdateGUI(UserIndex controllerNo)
        {
            try
            {
                CardControl[] cards = new CardControl[1];

                if (Family.TYPE == Family.ProcessorType.Amd_Apu)
                {
                    CardControl[] cardsTemp = { ccSection1, ccSection2, ccSection3, ccSection4, ccSection5, ccSection6, ccSection7, ccSection8, ccSection81, ccSection82, ccSection9, ccSection10, ccSection11, ccSection12, ccSection13, ccSection14 };
                    cards = cardsTemp;
                }
                if (Family.TYPE == Family.ProcessorType.Intel)
                {
                    ccSection1.Visibility = Visibility.Collapsed;
                    ccSection2.Visibility = Visibility.Collapsed;
                    ccSection5.Visibility = Visibility.Collapsed;
                    ccSection9.Visibility = Visibility.Collapsed;
                    ccSection13.Visibility = Visibility.Collapsed;
                    CardControl[] cardsTemp = { ccSection3, ccSection4, ccSection7, ccSection8, ccSection81, ccSection82, ccSection11, ccSection12};
                    cards = cardsTemp;
                }

                    controller = new Controller(controllerNo);
                bool connected = controller.IsConnected;

                if (cards[cards.Length - 1].Visibility == Visibility.Visible)
                {
                    cards[cards.Length - 2].Margin = new Thickness(0, 9, 0, 0);
                    cards[cards.Length - 1].Margin = new Thickness(0, 9, 0, 18);
                }
                else
                {
                    cards[cards.Length - 2].Margin = new Thickness(0, 9, 0, 18);
                    cards[cards.Length - 1].Margin = new Thickness(0, 9, 0, 0);
                }

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
                            if (slider.SmallChange > 1) currentValue = currentValue - (int)slider.SmallChange;
                            else currentValue--;

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
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || tx > 26000)
                    {
                        Slider slider = Global.FindVisualChild<Slider>(cards[selected]);

                        if (slider != null)
                        {
                            int currentValue = (int)slider.Value;
                            if (slider.SmallChange > 1) currentValue = currentValue + (int)slider.SmallChange;
                            else currentValue++;

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
            if (Global.updatingPreset == false) savePreset();

            CardControl[] cards = { ccSection1, ccSection2, ccSection3, ccSection4, ccSection5, ccSection6, ccSection7, ccSection8, ccSection9, ccSection10, ccSection11, ccSection12, ccSection13, ccSection14 };

            if (cards[cards.Length - 1].Visibility == Visibility.Visible)
            {
                cards[cards.Length - 2].Margin = new Thickness(0, 9, 0, 0);
                cards[cards.Length - 1].Margin = new Thickness(0, 9, 0, 18);
            }
            else
            {
                cards[cards.Length - 2].Margin = new Thickness(0, 9, 0, 18);
                cards[cards.Length - 1].Margin = new Thickness(0, 9, 0, 0);
            }
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Global.updatingPreset == false) savePreset();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Global.updatingPreset == false) savePreset();
        }

        private static AdaptivePresetManager adaptivePresetManager = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
        private void savePreset()
        {
            adaptivePresetManager = new AdaptivePresetManager(Settings.Default.Path + "adaptivePresets.json");
            AdaptivePreset myPreset = adaptivePresetManager.GetPreset("Default");
            if (myPreset != null)
            {
                AdaptivePreset preset = new AdaptivePreset
                {
                    _isTemp = ViewModel.IsTemp,
                    tempLimit = ViewModel.TempLimit,
                    _isPower = ViewModel.IsPower,
                    powerLimit = ViewModel.PowerLimit,
                    _isUndervolt = ViewModel.IsUndervolt,
                    underVolt = ViewModel.UnderVolt,
                    _isMaxClock = ViewModel.IsMaxClock,
                    maxClock = ViewModel.MaxClock,
                    _isIGPUClock = ViewModel.IsIGPUClock,
                    iGPUClock = ViewModel.IGPUClock,
                    _isEPP = ViewModel.IsEPP,
                    _EPP = ViewModel.EPP,
                    _isRSR = ViewModel.IsRSR,
                    _RSR = ViewModel.RSR,
                    _isCoreCount = ViewModel.IsCoreCount,
                    _CoreCount = ViewModel.CoreCount
                };
                adaptivePresetManager.SavePreset(Global.presetName, preset);
            }
        }

        private void SizeSlider_TouchDown(object sender, TouchEventArgs e)
        {
            // Mark event as handled
            e.Handled = true;
        }
    }
}