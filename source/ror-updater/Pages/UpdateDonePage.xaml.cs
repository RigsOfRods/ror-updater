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
    public partial class UpdateDonePage : UserControl, ISwitchable
    {
        App mainApp;
        public UpdateDonePage(App MainThread)
        {
            InitializeComponent();
            mainApp = MainThread;
            mainApp.str_local_version = mainApp.str_online_version;
        }


#region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
#endregion

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Switch(new ChoicePage(mainApp));
        }

    }
}
