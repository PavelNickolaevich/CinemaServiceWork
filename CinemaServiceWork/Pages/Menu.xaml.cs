
using CinemaServiceWork.ApplicationData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Page
    {
        public Menu()
        {
            InitializeComponent();

        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {

            AppFrame.mainFrame.Navigate(new Pages.AuthorizationPage());

        }

        private void MyPageFilmsBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new Pages.MyFilmsPage());
        }


        private void favoritesBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new Pages.FavoritePage());
        }

        private void AddFilmBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new Pages.NewFilmPage());
        }

        private void AddActorBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new Pages.NewActorPage());
        }

        private void AddDirectorBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new Pages.NewDirectorPage());
        }
    }
}
