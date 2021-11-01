﻿// This file is part of ror-updater
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
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Sentry;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class UpdatePage : UserControl, ISwitchable
    {
        private readonly WebClient _webClient;

        private readonly CancellationTokenSource _cancel = new();

        public UpdatePage()
        {
            InitializeComponent();
            ((INotifyCollectionChanged) LogWindow.Items).CollectionChanged += ListView_CollectionChanged;

            _webClient = new WebClient();

            OverallProgress.Maximum = App.Instance.ReleaseInfoData.Filelist.Count;
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
                ProgressLabel.Content = fileid + "/" + App.Instance.ReleaseInfoData.Filelist.Count;
            });

            // DoProcessing is run on the thread pool.
            switch (App.Choice)
            {
                case UpdateChoice.INSTALL:
                    Welcome_Label.Content = "Installing Rigs of Rods";
                    await Task.Run(() => InstallGame(progress), _cancel.Token);
                    break;
                case UpdateChoice.UPDATE:
                    Welcome_Label.Content = "Updating Rigs of Rods";
                    await Task.Run(() => UpdateGame(progress), _cancel.Token);
                    break;
                case UpdateChoice.REPAIR:
                    Welcome_Label.Content = "Repairing Rigs of Rods";
                    await Task.Run(() => UpdateGame(progress), _cancel.Token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PageManager.Switch(new UpdateDonePage());
        }

        private async Task InstallGame(IProgress<int> progress)
        {
            Utils.LOG(Utils.LogPrefix.INFO, "Installing Game...");

            var i = 0;
            foreach (var file in App.Instance.ReleaseInfoData.Filelist)
            {
                if (_cancel.IsCancellationRequested) break;
                AddToLogFile($"Downloading file: {file.Directory.TrimStart('.')}/{file.Name}");
                await DownloadFile(file.Directory, file.Name);
                progress?.Report(i++);
            }

            Utils.LOG(Utils.LogPrefix.INFO, "Done.");
        }

        private async Task UpdateGame(IProgress<int> progress)
        {
            Utils.LOG(Utils.LogPrefix.INFO, "Updating Game...");

            var filesStatus = new List<FileStatus>();

            AddToLogFile("Checking for outdated files...");
            var i = 0;
            foreach (var file in App.Instance.ReleaseInfoData.Filelist)
            {
                if (_cancel.IsCancellationRequested) break;
                var fileStatus = HashFile(file);
                AddToLogFile($"Checking file: {file.Directory.TrimStart('.')}/{file.Name}");
                filesStatus.Add(new FileStatus {File = file, Status = fileStatus});
                progress?.Report(i++);
            }

            AddToLogFile("Done, updating outdated files now...");

            i = 0;
            foreach (var item in filesStatus)
            {
                if (_cancel.IsCancellationRequested) break;
                progress?.Report(i++);

                switch (item.Status)
                {
                    case HashResult.UPTODATE:
                        Utils.LOG(Utils.LogPrefix.INFO, $"file up to date: {item.File.Name}");
                        AddToLogFile($"File up to date: {item.File.Directory.TrimStart('.')}/{item.File.Name}");
                        break;
                    case HashResult.OUTOFDATE:
                        AddToLogFile($"File out of date: {item.File.Directory.TrimStart('.')}/{item.File.Name}");
                        Utils.LOG(Utils.LogPrefix.INFO, $"File out of date: {item.File.Name}");
                        await DownloadFile(item.File.Directory, item.File.Name);
                        break;
                    case HashResult.NOT_FOUND:
                        Utils.LOG(Utils.LogPrefix.INFO, $"File doesnt exits: {item.File.Name}");
                        AddToLogFile(
                            $"Downloading new file: {item.File.Directory.TrimStart('.')}/{item.File.Name}");
                        await DownloadFile(item.File.Directory, item.File.Name);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Utils.LOG(Utils.LogPrefix.INFO, "Done.");
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to stop the update?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            _cancel.Cancel();
            _webClient.CancelAsync();
            _cancel.Dispose();
            Utils.LOG(Utils.LogPrefix.INFO, "Update has been canceled");
            PageManager.Switch(new ChoicePage());
        }
        
        private void AddToLogFile(string s)
        {
            Dispatcher.Invoke(() => { LogWindow.Items.Add(s); });
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() => { DownloadProgress.Value = e.ProgressPercentage; });
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

        HashResult HashFile(PFileInfo item)
        {
            string sFileHash = null;
            var filePath = $"{item.Directory}/{item.Name}";

            Utils.LOG(Utils.LogPrefix.INFO, $"Checking file: {item.Name}");

            if (!File.Exists(filePath)) return HashResult.NOT_FOUND;
            sFileHash = Utils.GetFileHash(filePath);
            Utils.LOG(Utils.LogPrefix.INFO,
                $"{item.Name} Hash: Local: {sFileHash.ToLower()} Online: {item.Hash.ToLower()}");
            return sFileHash.ToLower().Equals(item.Hash.ToLower())
                ? HashResult.UPTODATE
                : HashResult.OUTOFDATE;
        }

        private async Task DownloadFile(string dir, string file)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Thread.Sleep(100);
            var dest = $"{dir}/{file}";
            var path = dir.Replace(".", "");
            var dlLink = $"{App.Instance.CDNUrl}/{path}/{file}";

            try
            {
                Utils.LOG(Utils.LogPrefix.INFO, $"ULR: {dlLink}");
                Utils.LOG(Utils.LogPrefix.INFO, $"File: {dest}");
                await _webClient.DownloadFileTaskAsync(new Uri(dlLink), dest);
            }
            catch (Exception ex)
            {
                Utils.LOG(Utils.LogPrefix.ERROR, ex.ToString());
                MessageBox.Show($"Failed to download file: {dest}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                SentrySdk.CaptureException(ex);
            }
        }

        private class FileStatus
        {
            public PFileInfo File;
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