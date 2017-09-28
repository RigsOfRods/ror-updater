// This file is part of ror-updater
// 
// Copyright (c) 2016 AnotherFoxGuy
// 
// ror-updater is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 3, as
// published by the Free Software Foundation.
// 
// ror-updater is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ror-updater. If not, see <http://www.gnu.org/licenses/>.
// 
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class UpdatePage : UserControl, ISwitchable
    {
        private readonly App _mainApp;
        private readonly WebClient _webClient;
        private readonly BackgroundWorker _worker;
        private int _fileid;
        public UpdatePage ThisPage;

        public UpdatePage(App mainThread)
        {
            InitializeComponent();
            ((INotifyCollectionChanged) LogWindow.Items).CollectionChanged += ListView_CollectionChanged;
            _mainApp = mainThread;
            ThisPage = this;

            _worker = new BackgroundWorker();
            _worker.DoWork += WorkerOnDoWork;
            _worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
            _worker.WorkerSupportsCancellation = true;

            OverallProgress.Maximum = App.FilesInfo.Count;

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += ProgressChanged;

            switch (App.Choise)
            {
                case UpdateChoise.INSTALL:
                    _webClient.DownloadFileCompleted += InstallDownloadFileCompleted;
                    Welcome_Label.Content = "Installing Rigs of Rods";
                    InstallGame();
                    break;
                case UpdateChoise.UPDATE:
                    _webClient.DownloadFileCompleted += UpdateDownloadFileCompleted;
                    Welcome_Label.Content = "Updating Rigs of Rods";
                    UpdateGame();
                    break;
                case UpdateChoise.REPAIR:
                    _webClient.DownloadFileCompleted += UpdateDownloadFileCompleted;
                    Welcome_Label.Content = "Repairing Rigs of Rods";
                    UpdateGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InstallGame()
        {
            Utils.LOG("Info| Installing Game...");

            var item = App.FilesInfo[0];
            Utils.LOG("Info| Downloading file:" + item.fileName);
            Utils.DownloadFile(_webClient, item.directory + "/" + item.fileName, item.directory);
            Utils.LOG("Info| Done.");
        }

        private void UpdateGame()
        {
            Utils.LOG("Info| Updating Game...");

            if (!Utils.CheckGameStructureAndFix())
                NextPage();

            _worker.RunWorkerAsync();
        }

        private void InstallDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            Utils.LOG("Info| Done.");
            _fileid++;

            if (_fileid >= App.FilesInfo.Count) NextPage();

            else
            {
                UpdateUi();
                var item = App.FilesInfo[_fileid];

                Utils.LOG("Info| Downloading file:" + item.fileName);
                LogWindow.Items.Add("Downloading file: " + item.directory.TrimStart('.') + "/" + item.fileName);
                Utils.DownloadFile(_webClient, item.directory + "/" + item.fileName, item.directory);
            }
        }

        private void UpdateDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            Update();
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (runWorkerCompletedEventArgs.Error != null)
            {
                Utils.LOG("Worker|Error:" + runWorkerCompletedEventArgs.Error);
            }

            else
            {
                var result = (HashResult) runWorkerCompletedEventArgs.Result;
                var item = App.FilesInfo[_fileid];
                switch (result)
                {
                    case HashResult.UPTODATE:
                        Utils.LOG("Info| file up to date:" + item.fileName);
                        LogWindow.Items.Add("File up to date: " + item.directory.TrimStart('.') + "/" + item.fileName);
                        break;
                    case HashResult.OUTOFDATE:
                        LogWindow.Items.Add("File out of date: " + item.directory.TrimStart('.') + "/" + item.fileName);
                        Utils.LOG("Info| File out of date:" + item.fileName);
                        Utils.DownloadFile(_webClient, item.directory + "/" + item.fileName, item.directory);
                        break;
                    case HashResult.NOT_FOUND:
                        Utils.LOG("Info| File doesnt exits:" + item.fileName);
                        LogWindow.Items.Add("Downloading file: " + item.directory.TrimStart('.') + "/" + item.fileName);
                        Utils.DownloadFile(_webClient, item.directory + "/" + item.fileName, item.directory);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (_webClient.IsBusy)
                return;

            Update();
        }

        private void Update()
        {
            Utils.LOG("Info| Done. ");
            _fileid++;

            if (_fileid >= App.FilesInfo.Count)
                NextPage();

            else
            {
                UpdateUi();
                _worker.RunWorkerAsync();
            }
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var item = App.FilesInfo[_fileid];
            string sFileHash = null;
            var filePath = item.directory + "/" + item.fileName;

            Utils.LOG("Info| Checking file: " + item.fileName);

            if (File.Exists(filePath))
            {
                sFileHash = Utils.GetFileHash(filePath);
                Utils.LOG("Info| " + item.fileName + " Hash: Local: " + sFileHash.ToLower() + " Online:" +
                          item.fileHash.ToLower());
                doWorkEventArgs.Result = sFileHash.ToLower() == item.fileHash.ToLower()
                    ? HashResult.UPTODATE
                    : HashResult.OUTOFDATE;
            }
            else
                doWorkEventArgs.Result = HashResult.NOT_FOUND;
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to stop the update?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _worker.CancelAsync();
                _webClient.CancelAsync();
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
            DownloadProgress.Value = e.ProgressPercentage;
        }

        private void UpdateUi()
        {
            OverallProgress.Value = _fileid;
            ProgressLabel.Content = _fileid + "/" + App.FilesInfo.Count;
        }

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            //Nothing here
        }

        //Stolen from: http://stackoverflow.com/questions/10884031/wpf-raise-an-event-when-item-is-added-in-listview
        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                LogWindow.ScrollIntoView(e.NewItems[0]);
        }

        private enum HashResult
        {
            UPTODATE,
            OUTOFDATE,
            NOT_FOUND
        }

        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        public void recvData(string[] str, int[] num)
        {
        }

        #endregion
    }
}