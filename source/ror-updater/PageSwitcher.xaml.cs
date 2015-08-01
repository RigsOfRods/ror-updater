using System;
using System.Windows;
using System.Windows.Controls;

//Taken from here: https://azerdark.wordpress.com/2010/04/23/multi-page-application-in-wpf/

namespace ror_updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PageSwitcher : Window
    {
        App mainApp;
        UserControl currPage;
        public PageSwitcher(App MainThread)
        {
            InitializeComponent();
            PageManager.pageSwitcher = this;
            PageManager.Switch(new MainPage(MainThread));
            mainApp = MainThread;
        }
        public void Navigate(UserControl nextPage)
        {
            this.Content = currPage = nextPage;
        }

        public void Navigate(UserControl nextPage, object state)
        {
            this.Content = nextPage;
            ISwitchable s = nextPage as ISwitchable;

            if (s != null)
                s.UtilizeState(state);
            else
                throw new ArgumentException("NextPage is not ISwitchable! " + nextPage.Name.ToString());
        }

        public void Quit()
        {
            mainApp.Quit();
        }

        public UserControl getCurrPage()
        {
            return currPage;
        }

    }
}
