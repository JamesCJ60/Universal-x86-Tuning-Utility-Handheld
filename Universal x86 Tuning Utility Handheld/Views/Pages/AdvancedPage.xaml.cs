using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;
using Universal_x86_Tuning_Utility_Handheld.Properties;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Universal_x86_Tuning_Utility_Handheld.Services;
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

        private static CardControl[] cards = new CardControl[1];

        public AdvancedPage(ViewModels.AdvancedViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            _ = Tablet.TabletDevices;

            normalBorderBrush = ccSection12.BorderBrush;

            checkInput.Interval = TimeSpan.FromSeconds(0.5);
            checkInput.Tick += checkInput_Tick;
            checkInput.Start();

            updateGUI.Interval = TimeSpan.FromSeconds(2.2);
            updateGUI.Tick += updateGUI_Tick;
            updateGUI.Start();

            ViewModel.Battery = Global.batteryPer;
            ViewModel.BatteryIcon = Global.battery;

            Garbage.Garbage_Collect();

            Controller_Event.buttonEvents.controllerInput += handleControllerInputs;

            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                CardControl[] cardsTemp = { ccSection1, ccSection2, ccSection3, ccSection4, ccSection41, ccSection42, ccSection43, ccSection431, ccSection44, ccSection45, ccSection46, ccSection5, ccSection6, ccSection7, ccSection8, ccSection81, ccSection82, ccSection9, ccSection10, ccSection101, ccSection102, ccSection103, ccSection104, ccSection105, ccSection11, ccSection12, ccSection13, ccSection14 };
                cards = cardsTemp;
            }
            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                ccSection1.Visibility = Visibility.Collapsed;
                ccSection2.Visibility = Visibility.Collapsed;
                ccSection5.Visibility = Visibility.Collapsed;
                ccSection9.Visibility = Visibility.Collapsed;
                ccSection13.Visibility = Visibility.Collapsed;
                CardControl[] cardsTemp = { ccSection3, ccSection4, ccSection41, ccSection42, ccSection43, ccSection431, ccSection44, ccSection45, ccSection46, ccSection7, ccSection8, ccSection81, ccSection82, ccSection101, ccSection102, ccSection103, ccSection104, ccSection105, ccSection11, ccSection12 };
                cards = cardsTemp;
            }

            if (ViewModel.IsAdaptiveTDP == true) { if (Family.FAM == Family.RyzenFamily.Renoir || Family.FAM == Family.RyzenFamily.Mendocino || Family.FAM == Family.RyzenFamily.Rembrandt || Family.FAM == Family.RyzenFamily.PhoenixPoint) ViewModel.ShowAdaptiveiGPU = true; }
            else ViewModel.ShowAdaptiveiGPU = false;
        }

        int selected = 0, lastSelected = -1;
        bool wasMini = true;
        async void checkInput_Tick(object sender, EventArgs e)
        {
            if (Global._mainWindowNav.SelectedPageIndex == 1 && Global._appVis == Visibility.Visible && Global.shortCut == false)
            {
                if (!Controller_Event.controller.IsConnected)
                {
                    foreach (var card in cards)
                    {
                        card.BorderBrush = normalBorderBrush;
                        card.BorderThickness = normalThickness;
                    }
                    lastSelected = -1;
                }
                else
                {
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

                if (ViewModel.IsAdaptiveTDP == false && ViewModel.IsAdaptiveiGPU == true) ViewModel.IsAdaptiveiGPU = false;

                var foregroundBrush = (Brush)System.Windows.Application.Current.FindResource("TextFillColorPrimaryBrush");
                selectedBorderBrush = foregroundBrush;
            }
        }

        async void updateGUI_Tick(object sender, EventArgs e)
        {
            ViewModel.Battery = Global.batteryPer;
            ViewModel.BatteryIcon = Global.battery;
        }

        private void handleControllerInputs(object sender, EventArgs e)
        {
            try
            {
                if (Global._mainWindowNav.SelectedPageIndex == 1 && Global._appVis == Visibility.Visible && Global.shortCut == false)
                {
                    ScrollViewer svMain = Global.FindVisualChild<ScrollViewer>(this);
                    Universal_x86_Tuning_Utility_Handheld.Scripts.Misc.controllerInputEventArgs args = (Universal_x86_Tuning_Utility_Handheld.Scripts.Misc.controllerInputEventArgs)e;
                    if (ViewModel.IsAdaptiveTDP == false && ViewModel.IsAdaptiveiGPU == true) ViewModel.IsAdaptiveiGPU = false;

                    if (args.Action == "Up")
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

                    if (args.Action == "Down")
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

                    if (args.Action == "Left")
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

                    if (args.Action == "Right")
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

                    if (args.Action == "A")
                    {
                        ToggleSwitch toggleSwitch = Global.FindVisualChild<ToggleSwitch>(cards[selected]);

                        if (toggleSwitch != null)
                        {
                            if (toggleSwitch.IsChecked == true) toggleSwitch.IsChecked = false;
                            else toggleSwitch.IsChecked = true;
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
                        if (selected > cards.Length / 2)
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
            }
            catch
            {

            }
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (Global.updatingPreset == false) savePreset();

            CardControl[] cards = { ccSection1, ccSection2, ccSection3, ccSection4, ccSection5, ccSection6, ccSection7, ccSection8, ccSection81, ccSection82, ccSection9, ccSection10, ccSection101, ccSection102, ccSection103, ccSection104, ccSection105, ccSection11, ccSection12, ccSection13, ccSection14 };

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
                    _CoreCount = ViewModel.CoreCount,
                    _isFPS = ViewModel.IsFPS,
                    _fps = ViewModel.Fps,
                    _isAdaptiveFPS = ViewModel.IsAdaptiveFPS,
                    _minFps = ViewModel.MinFps,
                    _maxFps = ViewModel.MaxFps,
                    _isAdaptiveTDP = ViewModel.IsAdaptiveTDP,
                    _isAdaptiveiGPU = ViewModel.IsAdaptiveiGPU,
                    _maxTDP = ViewModel.MaxTDP,
                    _maxTemp = ViewModel.MaxTemp,
                    _maxiGPU = ViewModel.MaxiGPU,
                    _miniGPU = ViewModel.MiniGPU,
                    _isAdaptivePerf = ViewModel.IsAdaptivePerf,
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