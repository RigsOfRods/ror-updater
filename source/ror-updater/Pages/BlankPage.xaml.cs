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
    public partial class BlankPage : UserControl, ISwitchable
    {
        App mainApp;
        public BlankPage(App MainThread)
        {
            InitializeComponent();
            mainApp = MainThread;
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

        public void recvData(string[] str, int[] num)
        { 

        }

    }
}
