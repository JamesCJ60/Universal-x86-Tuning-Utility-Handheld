using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Mvvm.Contracts;

namespace Universal_x86_Tuning_Utility_Handheld.Views.Windows
{
    /// <summary>
    /// Interaction logic for GameLauncher.xaml
    /// </summary>
    public partial class GameLauncher
    {
        public static Frame navFrame;
        public GameLauncher()
        {
            InitializeComponent();
            navFrame = PagesNavigation;
            navFrame.Navigate(new System.Uri("Views\\Pages\\glGUI1.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}
