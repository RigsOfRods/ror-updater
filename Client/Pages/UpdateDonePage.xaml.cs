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
    public partial class UpdateDonePage : UserControl, ISwitchable
    {
        public UpdateDonePage()
        {
            InitializeComponent();
            App.Instance.LocalVersion = App.Instance.ReleaseInfoData.Version;
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

        private void button_quit_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Quit();
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            PageManager.Switch(new ChoicePage());
        }
    }
}