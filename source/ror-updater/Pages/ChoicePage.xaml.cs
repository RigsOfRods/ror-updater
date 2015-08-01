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
    public partial class ChoicePage : UserControl, ISwitchable
    {
        App mainApp;
        public ChoicePage(App MainThread)
        {
            InitializeComponent();
            mainApp = MainThread;
            mainApp.LOG("Info| Choise menu opened.");

            //TODO: Install button is disabled for the moment.
            //Repair game is also update game, both do the same, both do their work.
            if (mainApp.str_local_version != mainApp.str_online_version)
            {
                button_choise1.IsEnabled = true;
                button_choise2.IsEnabled = false;
                button_choise3.IsEnabled = false;
            }
            else
            {
                info_label.Content = "Your game is up to date!";
                button_choise3.IsEnabled = true;
                button_choise1.IsEnabled = false;
                button_choise2.IsEnabled = false;
            }
        }


#region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
#endregion

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Switch(new MainPage(mainApp));
        }

        private void button_choise1_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Switch(new UpdatePage(mainApp));
            mainApp.LOG("Info| Selected update.");
        }

    }
}
