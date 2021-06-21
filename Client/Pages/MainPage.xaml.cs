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
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : UserControl, ISwitchable
    {
        public MainPage()
        {
            InitializeComponent();
            var listItems = App.Instance.BranchInfo.Branches
                .Select(p => new ListItem {ID = p.Key, Name = p.Value.Name})
                .ToList();
            BranchesListBox.ItemsSource = listItems;
            local_version.Content = $"Local version: {App.Instance.LocalVersion}";
            online_version.Content = $"Online version: {App.Instance.ReleaseInfoData.Version}";

            try
            {
                BranchesListBox.SelectedItem = listItems.Find(i => i.ID == App.Instance.Settings.Branch);
            }
            catch (Exception ex)
            {
                Utils.LOG(Utils.LogVerb.ERROR, ex.ToString());
            }
        }

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            App.Instance.SaveSettings();
            PageManager.Switch(new ChoicePage());
        }

        private void button_quit_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Quit();
        }

        private void BranchesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Instance.UpdateBranch(((ListItem) BranchesListBox.SelectedItem).ID);
            online_version.Content = $"Online version: {App.Instance.ReleaseInfoData.Version}";
        }

        private class ListItem
        {
            public string ID { get; set; }
            public string Name { get; set; }
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