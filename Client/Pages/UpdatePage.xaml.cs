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
using ror_updater.Tasks;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class UpdatePage : UserControl, ISwitchable
    {
        private RunUpdate _updateTask;

        public UpdatePage()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)LogWindow.Items).CollectionChanged += ListView_CollectionChanged;

            var wc = new WebClient();

            OverallProgress.Maximum = App.Instance.ReleaseInfoData.Filelist.Count;
            wc.DownloadProgressChanged += ProgressChanged;
            // The Progress<T> constructor captures our UI context,
            //  so the lambda will be run on the UI thread.
            // https://blog.stephencleary.com/2012/02/reporting-progress-from-async-tasks.html
            var progress = new Progress<int>(fileid =>
            {
                OverallProgress.Value = fileid;
                ProgressLabel.Content = fileid + "/" + App.Instance.ReleaseInfoData.Filelist.Count;
            });
            
            _updateTask = new RunUpdate(AddToLogFile, progress, wc);

            RunFileUpdate();
        }

        private async void RunFileUpdate()
        {
            // DoProcessing is run on the thread pool.
            switch (App.Choice)
            {
                case UpdateChoice.INSTALL:
                    Welcome_Label.Content = "Installing Rigs of Rods";
                    await _updateTask.InstallGame();
                    break;
                case UpdateChoice.UPDATE:
                    Welcome_Label.Content = "Updating Rigs of Rods";
                    await _updateTask.UpdateGame();
                    break;
                case UpdateChoice.REPAIR:
                    Welcome_Label.Content = "Repairing Rigs of Rods";
                    await _updateTask.UpdateGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PageManager.Switch(new UpdateDonePage());
        }


        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to stop the update?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            CancelOperation();
        }

        private void CancelOperation()
        {
            _updateTask.Cancel();
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