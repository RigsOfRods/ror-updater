using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class InstallPage : UserControl, ISwitchable
    {
        private readonly App _mainApp;
        private WebClient _webClient;
        private bool stop = false;
        int _fileid;

        public InstallPage(App mainThread)
        {
            InitializeComponent();
            _mainApp = mainThread;

            _mainApp.PreUpdate();

            // MainProgress.Maximum = _mainApp.ListCount;
            //MainProgress.Value = 0;

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += ProgressChanged;
            _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;

            
        }

        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
                
        }

        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void recvData(string[] str, int[] num)
        {
            MainProgress.Value = int.Parse(str[1]);
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to stop the install?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                stop = true;
                PageManager.Switch(new ChoicePage(_mainApp));
            }
        }

        private void NextPage()
        {
            PageManager.Switch(new UpdateDonePage(_mainApp));
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //update ui
            MainProgress.Value = e.ProgressPercentage;
            ProgressLabel.Content = _fileid + "/" + _mainApp.ListCount;
        }

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            //Nothing here
        }    
    }
}