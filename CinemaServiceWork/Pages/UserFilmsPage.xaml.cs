using CinemaServiceWork.ApplicationData;
using CinemaServiceWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using System.Windows.Forms;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for UserFilmsPage.xaml
    /// </summary>
    public partial class UserFilmsPage : Page
    {
        public UserFilmsPage()
        {
            InitializeComponent();
            AppFrame.menuFrame = MenuFrame;
            AppFrame.contentFrame = ContetntFrame;
            MenuFrame.Navigate(new Pages.Menu());
            ContetntFrame.Navigate(new Pages.MainPage());


        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = (WindowState)FormWindowState.Minimized;

        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
