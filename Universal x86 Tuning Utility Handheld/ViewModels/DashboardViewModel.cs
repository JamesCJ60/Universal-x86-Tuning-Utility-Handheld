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
    public partial class DashboardViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private int _counter = 0;
        [ObservableProperty]
        private bool _wifi = true;
        [ObservableProperty]
        private bool _bluetooth = true;
        [ObservableProperty]
        private int _brightness = 75;
        [ObservableProperty]
        private int _volume = 35;
        [ObservableProperty]
        private bool _recording = Settings.Default.isMute;
        [ObservableProperty]
        private bool _overlay = Settings.Default.isRTSS;
        [ObservableProperty]
        private bool _mouse = Settings.Default.isMouse;
        [ObservableProperty]
        private bool _startOnBoot = Settings.Default.StartOnBoot;
        [ObservableProperty]
        private bool _startMini = Settings.Default.StartMini;
        [ObservableProperty]
        private string _ACMode = "Perf Mode";
        [ObservableProperty]
        private int _acMode = Settings.Default.acMode;
        [ObservableProperty]
        private bool _isASUS = Settings.Default.isASUS;
        [ObservableProperty]
        private SymbolRegular _acModeIcon = SymbolRegular.Scales24;
        [ObservableProperty]
        private SymbolRegular _micIcon = SymbolRegular.Mic24;
        [ObservableProperty]
        private SymbolRegular _wifiIcon = SymbolRegular.Wifi120;
        [ObservableProperty]
        private SymbolRegular _blueIcon = SymbolRegular.Bluetooth24;
        [ObservableProperty]
        private SymbolRegular _volIcon = SymbolRegular.Speaker224;
        [ObservableProperty]
        private SymbolRegular _brightIcon = SymbolRegular.BrightnessHigh24;

        [ObservableProperty]
        private string _xgMobileTag = "Activate ROG XG Mobile";
        [ObservableProperty]
        private bool _isXgMobile = false;
        [ObservableProperty]
        private bool _isXgMobileLED = Settings.Default.xgMobileLED;

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
