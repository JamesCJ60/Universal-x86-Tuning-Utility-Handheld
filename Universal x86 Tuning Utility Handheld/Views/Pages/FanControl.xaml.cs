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
    public partial class FanControl : INavigableView<ViewModels.FanControlViewModel>
    {
        private Brush normalBorderBrush;
        private Brush selectedBorderBrush = Brushes.White;
        Thickness normalThickness = new Thickness(1);
        Thickness selectedThickness = new Thickness(2.5);
        public ViewModels.FanControlViewModel ViewModel
        {
            get; set;
        }
        private DispatcherTimer checkInput = new DispatcherTimer();

        public FanControl(ViewModels.FanControlViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            _ = Tablet.TabletDevices;

            normalBorderBrush = ccApply.BorderBrush;
            checkInput.Interval = TimeSpan.FromSeconds(0.12);
            checkInput.Tick += checkInput_Tick;
            checkInput.Start();

            setupGUI();

            Garbage.Garbage_Collect();
        }

        async void setupGUI()
        {
            ViewModel.IsFan = Settings.Default.isFanContol;
            string speedString = Settings.Default.fanCurve;
            string[] speedStringArray = speedString.Split('-');
            int[] speeds = Array.ConvertAll(speedStringArray, int.Parse);

            ViewModel.Option1 = speeds[0];
            ViewModel.Option2 = speeds[1];
            ViewModel.Option3 = speeds[2];
            ViewModel.Option4 = speeds[3];
            ViewModel.Option5 = speeds[4];
            ViewModel.Option6 = speeds[5];
            ViewModel.Option7 = speeds[6];
            ViewModel.Option8 = speeds[7];
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

        private void UpdateGUI(UserIndex controllerNo)
        {
            try
            {
                CardControl[] cards = {ccSection1, ccSection2, ccSection3, ccSection4, ccSection5, ccSection6, ccSection7, ccSection8, ccSection9,  ccApply };
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
                            Settings.Default.isFanContol = ViewModel.IsFan;
                            Settings.Default.fanCurve = $"{ViewModel.Option1}-{ViewModel.Option2}-{ViewModel.Option3}-{ViewModel.Option4}-{ViewModel.Option5}-{ViewModel.Option6}-{ViewModel.Option7}-{ViewModel.Option8}";
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

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.isFanContol = ViewModel.IsFan;
            Settings.Default.fanCurve = $"{ViewModel.Option1}-{ViewModel.Option2}-{ViewModel.Option3}-{ViewModel.Option4}-{ViewModel.Option5}-{ViewModel.Option6}-{ViewModel.Option7}-{ViewModel.Option8}";
            Settings.Default.Save();
        }
           
    }
}