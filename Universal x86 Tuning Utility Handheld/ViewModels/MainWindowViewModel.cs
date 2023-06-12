using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace Universal_x86_Tuning_Utility_Handheld.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _applicationTitle = String.Empty;

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        [ObservableProperty]
        private int battery = 100;

        [ObservableProperty]
        private DateTime time = DateTime.Now;

        [ObservableProperty]
        private SymbolRegular batteryIcon = SymbolRegular.Battery924;

        [ObservableProperty]
        private SymbolRegular wifiIcon = SymbolRegular.Wifi124;

        public MainWindowViewModel(INavigationService navigationService)
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            ApplicationTitle = "Universal x86 Tuning Utility";

            NavigationItems = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Basic",
                    PageTag = "home",
                    Icon = SymbolRegular.Settings24,
                    PageType = typeof(Views.Pages.DashboardPage)
                },
                new NavigationItem()
                {
                    Content = "Advanced",
                    PageTag = "power",
                    Icon = SymbolRegular.Battery924,
                    PageType = typeof(Views.Pages.DataPage)
                },
                new NavigationItem()
                {
                    Content = "Games",
                    PageTag = "games",
                    Icon = SymbolRegular.Games24,
                    //PageType = typeof(Views.Pages.DataPage)
                },
                new NavigationItem()
                {
                    Content = "Display",
                    PageTag = "display",
                    Icon = SymbolRegular.Desktop24,
                    //PageType = typeof(Views.Pages.DataPage)
                },
                //new NavigationItem()
                //{
                //    Content = "FSR",
                //    PageTag = "fsr",
                //    Icon = SymbolRegular.ZoomIn24,
                //    PageType = typeof(Views.Pages.DataPage)
                //},
                new NavigationItem()
                {
                    Content = "Controller",
                    PageTag = "gamepad",
                    Icon = SymbolRegular.XboxController24,
                    //PageType = typeof(Views.Pages.DataPage)
                }
            };

            //NavigationFooter = new ObservableCollection<INavigationControl>
            //{
            //    new NavigationItem()
            //    {
            //        Content = "Settings",
            //        PageTag = "settings",
            //        Icon = SymbolRegular.Settings24,
            //        PageType = typeof(Views.Pages.SettingsPage)
            //    }
            //};

            TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    Header = "Home",
                    Tag = "tray_home"
                }
            };

            _isInitialized = true;
        }
    }
}
