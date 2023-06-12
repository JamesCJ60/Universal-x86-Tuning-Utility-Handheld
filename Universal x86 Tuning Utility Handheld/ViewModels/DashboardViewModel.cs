using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Universal_x86_Tuning_Utility_Handheld.Properties;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Wpf.Ui.Common.Interfaces;

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

        public void OnNavigatedTo()
        {
        }

        public void OnNavigatedFrom()
        {
        }

        //[RelayCommand]
        //private void OnCounterIncrement()
        //{
        //    Counter++;
        //}
    }
}
