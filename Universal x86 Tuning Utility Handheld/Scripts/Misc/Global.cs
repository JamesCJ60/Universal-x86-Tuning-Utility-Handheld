using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Wpf.Ui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Common;

namespace Universal_x86_Tuning_Utility_Handheld.Scripts.Misc
{
    internal class Global
    {
        public static NavigationStore _mainWindowNav = null;

        public static Visibility _appVis = Visibility.Visible;

        public static bool shortCut = false;

        public static SymbolRegular wifi = SymbolRegular.Empty;

        public static SymbolRegular battery = SymbolRegular.Empty;
        public static int batteryPer = 0;

        public static string presetName = "Default";
        public static bool updatingPreset = false;
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;

                T foundChild = FindVisualChild<T>(child);

                if (foundChild != null)
                    return foundChild;
            }

            return null;
        }
    }
}
