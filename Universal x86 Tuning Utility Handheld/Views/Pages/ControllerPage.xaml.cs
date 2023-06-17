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
using Newtonsoft.Json.Linq;

namespace Universal_x86_Tuning_Utility_Handheld.Views.Pages
{
    /// <summary>
    /// Interaction logic for ControllerPage.xaml
    /// </summary>
    public partial class ControllerPage : INavigableView<ViewModels.ControllerViewModel>
    {
        private Brush normalBorderBrush;
        private Brush selectedBorderBrush = Brushes.White;
        Thickness normalThickness = new Thickness(1);
        Thickness selectedThickness = new Thickness(2.5);
        public ViewModels.ControllerViewModel ViewModel
        {
            get; set;
        }
        private DispatcherTimer checkInput = new DispatcherTimer();
        public ControllerPage(ViewModels.ControllerViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            _ = Tablet.TabletDevices;

            normalBorderBrush = ccSection2.BorderBrush;
            checkInput.Interval = TimeSpan.FromSeconds(0.12);
            checkInput.Tick += checkInput_Tick;
            checkInput.Start();

            Garbage.Garbage_Collect();
        }

        int selected = 0, lastSelected = 0;
        bool wasMini = true;
        async void checkInput_Tick(object sender, EventArgs e)
        {
            if (Global._mainWindowNav.SelectedPageIndex == 2 && Global._appVis == Visibility.Visible && Global.shortCut == false)
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
                    wasMini = false;
                }

                CardControl[] cards = { ccSection1, ccSection2, ccSection3 };
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
                            if (toggleSwitch.IsChecked == true) toggleSwitch.IsChecked = false;
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

        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyController();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyController();
        }

        private void ApplyController()
        {
            if (ViewModel.IsVib == true)
            {
                // Create an instance of the XInput controller
                Controller controller = new Controller(UserIndex.One);

                // Check if the controller is connected
                if (controller.IsConnected)
                {
                    // Set the vibration parameters
                    Vibration vibration = new Vibration
                    {
                        LeftMotorSpeed = (ushort)((ViewModel.LeftVib / 100.0) * 65535),
                        RightMotorSpeed = (ushort)((ViewModel.RightVib / 100.0) * 65535)
                    };

                    // Apply the vibration
                    controller.SetVibration(vibration);
                }
            }

            Settings.Default.isVib = ViewModel.IsVib;
            Settings.Default.LeftMotor = ViewModel.LeftVib;
            Settings.Default.RightMotor = ViewModel.RightVib;
            Settings.Default.Save();
        }

        private void SizeSlider_TouchDown(object sender, TouchEventArgs e)
        {
            // Mark event as handled
            e.Handled = true;
        }
    }
}