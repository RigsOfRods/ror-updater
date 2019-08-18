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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class UpdatePage : UserControl, ISwitchable
    {
        private readonly WebClient _webClient;

        public UpdatePage()
        {
            InitializeComponent();
            ((INotifyCollectionChanged) LogWindow.Items).CollectionChanged += ListView_CollectionChanged;

            _webClient = new WebClient();

            OverallProgress.Maximum = App.FilesInfo.Count;
            _webClient.DownloadProgressChanged += ProgressChanged;

            RunFileUpdate();
        }

        private async void RunFileUpdate()
        {
            // The Progress<T> constructor captures our UI context,
            //  so the lambda will be run on the UI thread.
            // https://blog.stephencleary.com/2012/02/reporting-progress-from-async-tasks.html
            var progress = new Progress<int>(fileid =>
            {
                OverallProgress.Value = fileid;
                ProgressLabel.Content = fileid + "/" + App.FilesInfo.Count;
            });

            // DoProcessing is run on the thread pool.
            switch (App.Choice)
            {
                case UpdateChoice.INSTALL:
                    Welcome_Label.Content = "Installing Rigs of Rods";
                    await Task.Run(() => InstallGame(progress));
                    break;
                case UpdateChoice.UPDATE:
                    Welcome_Label.Content = "Updating Rigs of Rods";
                    await Task.Run(() => UpdateGame(progress));
                    break;
                case UpdateChoice.REPAIR:
                    Welcome_Label.Content = "Repairing Rigs of Rods";
                    await Task.Run(() => UpdateGame(progress));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InstallGame(IProgress<int> progress)
        {
            Utils.LOG("Info| Installing Game...");

            for (var i = 0; i < App.FilesInfo.Count; i++)
            {
                var file = App.FilesInfo[i];
                AddToLogFile($"Downloading file: {file.directory.TrimStart('.')}/{file.fileName}");
                DownloadFile(file.dlLink, file.directory, file.fileName);
                progress?.Report(i);
            }

            Utils.LOG("Info| Done.");
            NextPage();
        }

        private void UpdateGame(IProgress<int> progress)
        {
            Utils.LOG("Info| Updating Game...");

            var _fileStatus = new List<FileStatus>();

            AddToLogFile($"Checking for outdated files...");
            for (var i = 0; i < App.FilesInfo.Count; i++)
            {
                var file = App.FilesInfo[i];
                var fs = HashFile(file);
                AddToLogFile($"Checking file: {file.directory.TrimStart('.')}/{file.fileName}");
                _fileStatus.Add(new FileStatus {File = file, Status = fs});
                progress?.Report(i);
            }

            AddToLogFile($"Done, updating outdated files now...");

            for (var i = 0; i < _fileStatus.Count; i++)
            {
                var item = _fileStatus[i];
                progress?.Report(i);

                switch (item.Status)
                {
                    case HashResult.UPTODATE:
                        Utils.LOG($"Info| file up to date:{item.File.fileName}");
                        AddToLogFile($"File up to date: {item.File.directory.TrimStart('.')}/{item.File.fileName}");
                        break;
                    case HashResult.OUTOFDATE:
                        AddToLogFile($"File out of date: {item.File.directory.TrimStart('.')}/{item.File.fileName}");
                        Utils.LOG($"Info| File out of date:{item.File.fileName}");
                        DownloadFile(item.File.dlLink, item.File.directory, item.File.fileName);
                        break;
                    case HashResult.NOT_FOUND:
                        Utils.LOG($"Info| File doesnt exits:{item.File.fileName}");
                        AddToLogFile(
                            $"Downloading new file: {item.File.directory.TrimStart('.')}/{item.File.fileName}");
                        DownloadFile(item.File.dlLink, item.File.directory, item.File.fileName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Utils.LOG("Info| Done.");
            NextPage();
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to stop the update?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            _webClient.CancelAsync();
            PageManager.Switch(new ChoicePage());
        }


        private void AddToLogFile(string s)
        {
            Dispatcher.BeginInvoke(new Action(() => { LogWindow.Items.Add(s); }));
        }

        private void NextPage()
        {
            Dispatcher.BeginInvoke(new Action(() => { PageManager.Switch(new UpdateDonePage()); }));
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //update ui
            DownloadProgress.Value = e.ProgressPercentage;
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

        HashResult HashFile(RoRUpdaterItem item)
        {
            string sFileHash = null;
            var filePath = $"{item.directory}/{item.fileName}";

            Utils.LOG($"Info| Checking file: {item.fileName}");

            if (!File.Exists(filePath)) return HashResult.NOT_FOUND;
            sFileHash = Utils.GetFileHash(filePath);
            Utils.LOG($"Info| {item.fileName} Hash: Local: {sFileHash.ToLower()} Online:{item.fileHash.ToLower()}");
            return sFileHash.ToLower().Equals(item.fileHash.ToLower())
                ? HashResult.UPTODATE
                : HashResult.OUTOFDATE;
        }

        private void DownloadFile(string dlLink, string dir, string file)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Thread.Sleep(100);
            var dest = $"{dir}/{file}";

            try
            {
                Utils.LOG($"Info| ULR: {dlLink}");
                Utils.LOG($"Info| File: {dest}");
                _webClient.DownloadFile(new Uri(dlLink), dest);
            }
            catch (Exception ex)
            {
                Utils.LOG(ex.ToString());
                MessageBox.Show($"Failed to download file:{dest}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private class FileStatus
        {
            public RoRUpdaterItem File;
            public HashResult Status;
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