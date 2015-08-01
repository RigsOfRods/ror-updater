using System.Windows.Controls;

//Acts like a bridge between pages and the main page/application.
namespace ror_updater
{
    public static class PageManager
    {
        public static PageSwitcher pageSwitcher;

        public static void Switch(UserControl newPage)
        {
            pageSwitcher.Navigate(newPage);
        }

        public static void Switch(UserControl newPage, object state)
        {
            pageSwitcher.Navigate(newPage, state);
        }

        public static void Quit()
        {
            pageSwitcher.Quit();
        }
    }
}
