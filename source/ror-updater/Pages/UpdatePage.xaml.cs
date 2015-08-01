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
using System.ComponentModel;

namespace ror_updater
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class UpdatePage : UserControl, ISwitchable
    {
        App mainApp;
       

        public UpdatePage(App MainThread)
        {
            InitializeComponent();
            mainApp = MainThread;

            mainApp.ProcessUpdateWorker = new BackgroundWorker();

            mainApp.ProcessUpdateWorker.WorkerReportsProgress = true;
            mainApp.ProcessUpdateWorker.WorkerSupportsCancellation = true;

            mainApp.ProcessUpdateWorker.ProgressChanged += worker_ProgressChanged;
            mainApp.ProcessUpdateWorker.DoWork += worker_DoWork;
            mainApp.ProcessUpdateWorker.RunWorkerCompleted += worker_RunWorkerCompleted;

            mainApp.ProcessUpdateWorker.RunWorkerAsync();

            MainProgress.Maximum = mainApp.listCount;
            MainProgress.Value = 0;
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to stop the update?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                mainApp.ProcessUpdateWorker.CancelAsync();
                killWorker();
                PageManager.Switch(new ChoisePage(mainApp));
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here
            mainApp.ProcessUpdate();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Move to next page
            killWorker();
            PageManager.Switch(new UpdateDonePage(mainApp));
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //update ui
            MainProgress.Value = e.ProgressPercentage;
            ProgressLabel.Content = e.ProgressPercentage.ToString() + "/" + (mainApp.listCount + (int)1);
        }

#region ISwitchable Members
        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }
#endregion

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            //Nothing here
        }

        private void killWorker()
        {
            mainApp.ProcessUpdateWorker.Dispose();
            mainApp.ProcessUpdateWorker = null;
        }
    }
}
