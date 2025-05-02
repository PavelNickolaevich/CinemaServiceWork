
using CinemaServiceWork.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using CinemaServiceWork;
using CinemaServiceWork.ApplicationData;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for AuthorizationPage.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        public object WindowState { get; private set; }

        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = AppConnect.cinemaEntities.Users.FirstOrDefault(u => u.Nickname == txtUser.Text && u.Password == txtPass.Password);

            if (String.IsNullOrEmpty(txtUser.Text) || String.IsNullOrEmpty(txtPass.Password))
            {
                MessageBox.Show(
               "Введены не все данные",
               "Уведомление",
               MessageBoxButton.OK,
               MessageBoxImage.Information
             );

            }

            else if (AppConnect.cinemaEntities.Users.FirstOrDefault(u => u.Nickname == txtUser.Text) == null)
            {
                MessageBox.Show(
                "Такого пользовтаеля не существует",
                "Уведомление",
                MessageBoxButton.OK,
                MessageBoxImage.Information
              );

            }

            else if (AppConnect.cinemaEntities.Users.FirstOrDefault(u => u.Password == txtPass.Password) == null)
            {
                MessageBox.Show(
                "Введен неверный пароль",
                "Уведомление",
                MessageBoxButton.OK,
                MessageBoxImage.Information
              );

            }

            else if (user == null)
            {
                MessageBox.Show(
                "Введен неверный пароль или ник",
                "Уведомление",
                MessageBoxButton.OK,
                MessageBoxImage.Information
              );

            }
            else
            {
                MessageBox.Show(
               "Добро пожаловать",
               "Уведомление",
               MessageBoxButton.OK,
               MessageBoxImage.Information
             );
                NavigationService.Navigate(new Pages.UserFilmsPage(user));

            }
        }



        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Pages.RegistrationPage());
        }

        private void txtUser_TextChanged(object sender, TextChangedEventArgs e)
        {

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
