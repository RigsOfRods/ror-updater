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
using System.Windows;
using System.Windows.Controls;

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for MainPage.xaml
    /// </summary>
    public partial class ChoicePage : UserControl, ISwitchable
    {
        private readonly App _mainApp;

        public ChoicePage(App mainThread)
        {
            InitializeComponent();
            _mainApp = mainThread;
            Utils.LOG("Info| Choise menu opened.");

            if (App.BDevBuilds)
                info_label.Content =
                    "You have dev builds enabled! You'll be updated to unstable test builds if you continue!";

            //Repair game is also update game, both do the same, both do their work.

            if (_mainApp.StrLocalVersion == "unknown")
            {
                Update_button.IsEnabled = false;
                Repair_button.IsEnabled = false;
                Install_button.IsEnabled = true;
            }
            else if (_mainApp.StrLocalVersion != _mainApp.StrOnlineVersion)
            {
                Update_button.IsEnabled = true;
                Repair_button.IsEnabled = false;
                Install_button.IsEnabled = false;
            }
            else
            {
                info_label.Content = "Your game is up to date!";
                Repair_button.IsEnabled = true;
                Update_button.IsEnabled = false;
                Install_button.IsEnabled = false;
            }
        }

        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void recvData(string[] str, int[] num)
        {
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Switch(new MainPage(_mainApp));
        }

        private void Install_button_Click(object sender, RoutedEventArgs e)
        {
            Utils.LOG("Info| Selected Install.");
            App.Choise = UpdateChoise.INSTALL;
            PageManager.Switch(new UpdatePage(_mainApp));
        }

        private void Repair_button_Click(object sender, RoutedEventArgs e)
        {
            Utils.LOG("Info| Selected Repair.");
            App.Choise = UpdateChoise.REPAIR;
            PageManager.Switch(new UpdatePage(_mainApp));
        }

        private void Update_button_Click(object sender, RoutedEventArgs e)
        {
            Utils.LOG("Info| Selected Update.");
            App.Choise = UpdateChoise.UPDATE;
            PageManager.Switch(new UpdatePage(_mainApp));
        }
    }
}