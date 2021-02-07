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

//Taken from here: https://azerdark.wordpress.com/2010/04/23/multi-page-application-in-wpf/

namespace ror_updater
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PageSwitcher : Window
    {
        private UserControl _currPage;

        public PageSwitcher()
        {
            InitializeComponent();
            PageManager.pageSwitcher = this;
            PageManager.Switch(new MainPage());
        }

        public void Navigate(UserControl nextPage)
        {
            Content = _currPage = nextPage;
        }

        public void Navigate(UserControl nextPage, object state)
        {
            Content = nextPage;
            if (nextPage is ISwitchable s)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! " + nextPage.Name);
        }

        public void Quit()
        {
            Close();
        }

        public UserControl getCurrPage()
        {
            return _currPage;
        }
    }
}