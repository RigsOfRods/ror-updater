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

namespace ror_updater
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : UserControl, ISwitchable
    {
        App mainApp;
        public MainPage(App MainThread)
        {
            InitializeComponent();
            mainApp = MainThread;
            local_version.Content = "Local version: " + mainApp.str_local_version;
            online_version.Content = "Online version: " + mainApp.str_online_version;
        }


#region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
#endregion

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Switch(new ChoicePage(mainApp));
        }

        private void button_quit_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Quit();
        }

    }
}
