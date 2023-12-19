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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Universal_x86_Tuning_Utility_Handheld.Scripts.Misc;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Interfaces;

namespace Universal_x86_Tuning_Utility_Handheld.Views.Pages
{
    /// <summary>
    /// Interaction logic for PS_Vita_Like_GUI.xaml
    /// </summary>
    public partial class glGUI1 
    {

        public class MyItem
        {
            public string ImagePath { get; set; }
            public int Width { get; set; }
            public int MaxWidth { get; set; }
            public int Height { get; set; }
            public string Name { get; set; }
            public int FontSize { get; set; }
            public Thickness newMargin { get; set; }
        }

        public glGUI1()
        {
            InitializeComponent();
        }
        void UpdateGUI()
        {
            // Define the whole number
            double whole = 1920;
            // Define the part number
            double part = this.ActualWidth;
            // Calculate the percentage
            double percentage = (part / whole) * 100;
            int dim = Convert.ToInt32(228.0 / 100 * percentage);
            int fontSize = Convert.ToInt32(29.0 / 100 * percentage);
            int margin = Convert.ToInt32(64.0 / 100 * percentage);
            int menuFontSize = Convert.ToInt32(36.0 / 100 * percentage);
            int menuIconSize = Convert.ToInt32(46.0 / 100 * percentage);
            int margin2 = Convert.ToInt32(26.0 / 100 * percentage);

            tbBattery.FontSize = menuFontSize;
            siBattery.FontSize = menuIconSize;
            siWifi.FontSize = menuIconSize;
            siClock.FontSize = menuIconSize;
            tbTime.FontSize = menuFontSize;
            tbTime.Margin = new Thickness(3, -2, margin2, 0);
            siWifi.Margin = new Thickness(0, 0, margin2, 0);
            
            myListBox.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/d6176a1a9d033a84eaf753589672896e.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Resident Evil: Village", newMargin = new Thickness(margin, 0, margin, 0) });
            myListBox.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/66a5f020e93349d62a24aa3afb087e08.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Resident Evil 4", newMargin = new Thickness(margin, 0, margin, 0) });
            myListBox.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/ed2874bde2a0d22eb737669cd18e53a9.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "The Forest", newMargin = new Thickness(margin, 0, margin, 0) });

            myListBox2.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/9a6e826fc22c04013e9dbb31b681b4b6.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Doom Eternal", newMargin = new Thickness(margin, 0, margin, 0) });
            myListBox2.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/05caaa1ee436331375ed0584a9b7e131.png", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Destiny 2", newMargin = new Thickness(margin, 0, margin, 0) });
            myListBox2.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/6746ab73a55e75865ef02de3daf6de89.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Fallout: New Vegas", newMargin = new Thickness(margin, 0, margin, 0) });
            myListBox2.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/bba41d215e8b4f6963298cea5dd939c6.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Fallout 4", newMargin = new Thickness(margin, 0, margin, 0) });

            myListBox3.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/126d52daa5f1f145601605dd68aeb016.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Minecraft", newMargin = new Thickness(margin, 0, margin, 0) });
            myListBox3.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/bbbcc91f3c7288fd9233caf509fc4189.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Terraria", newMargin = new Thickness(margin, 0, margin, 0) });
            myListBox3.Items.Add(new MyItem { ImagePath = $"https://cdn2.steamgriddb.com/thumb/219c124adf995ddd486ec29e9c2d7f4f.jpg", MaxWidth = dim + 38, Width = dim, Height = dim, FontSize = fontSize, Name = "Cyberpunk 2077", newMargin = new Thickness(margin, 0, margin, 0) });

            lbMenus.Items.Add("");
            lbMenus.Items.Add("");
            lbMenus.Items.Add("");
            lbMenus.Items.Add("");
            lbMenus.SelectedIndex = 0;
            myListBox.SelectedIndex = 0;

            DispatcherTimer _sensor = new DispatcherTimer();
            _sensor.Tick += Sensor_Tick;
            _sensor.Interval = TimeSpan.FromSeconds(1);
            _sensor.Start();

            updateDateTime();
        }

        private void Sensor_Tick(object? sender, EventArgs e)
        {
            updateDateTime();
        }

        void updateDateTime()
        {
            tbBattery.Text = $"{Global.batteryPer}%";
            siBattery.Symbol = Global.battery;
            siWifi.Symbol = Global.wifi;
            tbTime.Text = DateTime.Now.ToString("HH:mm");
        }

        bool hasChanged = false;
        private void myListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (myListBox.SelectedIndex > -1 && hasChanged == false)
            {
                hasChanged = true;
                myListBox2.SelectedIndex = -1;
                myListBox3.SelectedIndex = -1;
                hasChanged = false;
            }
        }

        private void myListBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (myListBox2.SelectedIndex > -1 && hasChanged == false)
            {
                hasChanged = true;
                myListBox.SelectedIndex = -1;
                myListBox3.SelectedIndex = -1;
                hasChanged = false;
            }
        }

        private void myListBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (myListBox3.SelectedIndex > -1 && hasChanged == false)
            {
                hasChanged = true;
                myListBox.SelectedIndex = -1;
                myListBox2.SelectedIndex = -1;
                hasChanged = false;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGUI();
        }
    }
}
