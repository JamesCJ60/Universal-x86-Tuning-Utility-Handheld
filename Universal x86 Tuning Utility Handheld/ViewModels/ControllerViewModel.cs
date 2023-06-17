using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Universal_x86_Tuning_Utility_Handheld.Properties;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Wpf.Ui.Common;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Controls;

namespace Universal_x86_Tuning_Utility_Handheld.ViewModels
{
    public partial class ControllerViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private int _counter = 0;
        [ObservableProperty]
        private bool _isVib = Settings.Default.isVib;
        [ObservableProperty]
        private int _leftVib = Settings.Default.LeftMotor;
        [ObservableProperty]
        private int _rightVib = Settings.Default.RightMotor;

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

        }

        //[RelayCommand]
        //private void OnCounterIncrement()
        //{
        //    Counter++;
        //}
    }
}
