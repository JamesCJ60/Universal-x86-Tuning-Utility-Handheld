using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Universal_x86_Tuning_Utility_Handheld.Properties;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Wpf.Ui.Common;
using Wpf.Ui.Common.Interfaces;

namespace Universal_x86_Tuning_Utility_Handheld.ViewModels
{
    public partial class AdvancedViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private int _counter = 0;

        [ObservableProperty]
        private int battery = 100;

        [ObservableProperty]
        private string batteryTime = "1H";

        [ObservableProperty]
        private string chargeRate = "1H";

        [ObservableProperty]
        private bool isDischarge = true;

        [ObservableProperty]
        private SymbolRegular batteryIcon = SymbolRegular.Battery924;

        [ObservableProperty]
        private bool _isTemp = false;

        [ObservableProperty]
        private int tempLimit = 95;

        [ObservableProperty]
        private bool _isPower = false;

        [ObservableProperty]
        private int powerLimit = 15;

        [ObservableProperty]
        private bool _isUndervolt = false;

        [ObservableProperty]
        private int underVolt = 0;

        [ObservableProperty]
        private bool _isMaxClock = false;

        [ObservableProperty]
        private int maxClock = 3000;

        [ObservableProperty]
        private bool _isIGPUClock = false;

        [ObservableProperty]
        private int iGPUClock = 1500;

        [ObservableProperty]
        private bool _isEPP = false;

        [ObservableProperty]
        private int _EPP = 25;

        public void OnNavigatedTo()
        {
        }

        public void OnNavigatedFrom()
        {
        }
    }
}
