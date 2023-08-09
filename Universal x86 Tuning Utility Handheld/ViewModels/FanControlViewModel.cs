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
    public partial class FanControlViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private int option1 = 50;
        [ObservableProperty]
        private int option2 = 50;
        [ObservableProperty]
        private int option3 = 50;
        [ObservableProperty]
        private int option4 = 50;
        [ObservableProperty]
        private int option5 = 50;
        [ObservableProperty]
        private int option6 = 50;
        [ObservableProperty]
        private int option7 = 50;
        [ObservableProperty]
        private int option8 = 50;
        [ObservableProperty]
        private bool isFan = false;

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
