using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility_Handheld.Properties;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Universal_x86_Tuning_Utility_Handheld.Views.Windows;
using Wpf.Ui.Common;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;

namespace Universal_x86_Tuning_Utility_Handheld.ViewModels
{
    public partial class DisplaySettingsViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private string _displayRes = "1920 x 1080";
        [ObservableProperty]
        private string _batHz = "60Hz";
        [ObservableProperty]
        private string _plugHz = "60Hz";

        [ObservableProperty]
        private int _displayMaxRes = 8;
        [ObservableProperty]
        private int _batMaxHz = 8;
        [ObservableProperty]
        private int _plugMaxHz = 8;

        [ObservableProperty]
        private int _displayCurrentRes = 8;
        [ObservableProperty]
        private int _batCurrentHz = 8;
        [ObservableProperty]
        private int _plugCurrentHz = 8;

        private ICommand _functions;
        public ICommand FunctionCommands => _functions ??= new RelayCommand<string>(Functions);

        public void OnNavigatedTo()
        {
        }

        public void OnNavigatedFrom()
        {
        }

        private void Functions(string parameter)
        {
            switch (parameter)
            {
                case "close":
                    Process.GetCurrentProcess().Kill();
                    return;
                case "minimise":
                    MainWindow.minimise = 1;
                    return;
                case "xg":
                    new XG_Mobile_Prompt(false).Show();
                    return;
            }
        }

        //[RelayCommand]
        //private void OnCounterIncrement()
        //{
        //    Counter++;
        //}
    }
}
