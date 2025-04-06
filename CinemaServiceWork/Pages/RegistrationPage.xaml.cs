using CinemaServiceWork.ApplicationData;
using CinemaServiceWork.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.Xml.Linq;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for RegistartionPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.GoBack();
        }
        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {

            if (String.IsNullOrEmpty(txtName.Text) || String.IsNullOrEmpty(txtEmail.Text) || String.IsNullOrEmpty(txtSurname.Text)
                   || String.IsNullOrEmpty(txtRptPassword.Password)
                   || String.IsNullOrWhiteSpace(txtRptPassword.Password)
                   || String.IsNullOrEmpty(txtPassword.Password)
                   || String.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show(
                    "Не заполнены все поля",
                    "Уведомление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
                txtPassword.Focus();
                return;
            }
            else
            {
                Users user = new Users
                {
                    First_name = txtName.Text,
                    Last_name = txtSurname.Text,
                    Nickname = txtNickName.Text,
                    Birth_of_date = datePicker.SelectedDate,
                    Password = txtEmail.Text,
                    Email = txtEmail.Text
                };

                AppConnect.cinemaEntities.Users.Add(user);
                AppConnect.cinemaEntities.SaveChanges();
                MessageBox.Show(
                    "Данные успешно добавлены",
                    "Уведомление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
                AppFrame.mainFrame.GoBack();
            }
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtName.Text))
            {
                txtName.BorderBrush = Brushes.Red;

            }
            else
            {
                txtName.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtEmail.Text) || !ValidateFields.CheckIsValidEmail(txtEmail.Text))
            {
                txtEmail.BorderBrush = Brushes.Red;
                btnRegistration.IsEnabled = false;
                btnRegistration.Opacity = 0.5;


            }
            else if (AppConnect.cinemaEntities.Users.FirstOrDefault(u => u.Email == txtEmail.Text) != null)
            {
                txtEmail.BorderBrush = Brushes.Red;
                MessageBox.Show(
              "Данная почта уже зарегистрирована",
              "Уведомление",
              MessageBoxButton.OK,
              MessageBoxImage.Information
              );
                btnRegistration.IsEnabled = false;
                btnRegistration.Background = Brushes.Red;
                btnRegistration.Opacity = 0.5;
            }
            else
            {
                txtEmail.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                btnRegistration.IsEnabled = true;
                btnRegistration.Opacity = 1;

            }
        }

        private void txtNickName_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (String.IsNullOrEmpty(txtNickName.Text))
            {
                txtNickName.BorderBrush = Brushes.Red;

            }
            else if (AppConnect.cinemaEntities.Users.FirstOrDefault(u => u.Nickname == txtNickName.Text) != null)
            {
                txtNickName.BorderBrush = Brushes.Red;
                MessageBox.Show(
                "Данный никнейм уже зарегистрирован",
                "Уведомление",
                MessageBoxButton.OK,
                MessageBoxImage.Information
              );
            }
            else
            {
                txtNickName.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }

        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {

            if ((txtPassword.Password != txtRptPassword.Password) || (txtRptPasswordHide.Text != txtPasswordHide.Text))
            {
                btnRegistration.IsEnabled = false;
                txtRptPassword.Background = Brushes.LightCoral;
                txtRptPassword.BorderBrush = Brushes.Red;
            }
            else
            {
                btnRegistration.IsEnabled = true;
                txtRptPassword.Background = Brushes.LightGreen;
                txtRptPassword.BorderBrush = Brushes.Green;
            }
        }

        private void txtRptPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if ((txtPassword.Password != txtRptPassword.Password) || (txtRptPasswordHide.Text != txtPasswordHide.Text))
            {
                btnRegistration.IsEnabled = false;
                txtRptPassword.Background = Brushes.LightCoral;
                txtRptPassword.BorderBrush = Brushes.Red;

            }
            else
            {
                btnRegistration.IsEnabled = true;
                txtRptPassword.Background = Brushes.LightGreen;
                txtRptPassword.BorderBrush = Brushes.Green;
            }

        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            datePicker.SelectedDate = DateTime.Now.AddDays(-5);
            if (datePicker.SelectedDate.Value < DateTime.Now.AddYears(18))
            {
                MessageBox.Show(
               "Данный никнейм уже зарегистрирован",
               "Уведомление",
               MessageBoxButton.OK,
               MessageBoxImage.Information
             );

            }
        }

        private void datePicker_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
        {
            MessageBox.Show("Введённая дата некорректна. Пожалуйста, выберите другую дату.",
                   "Ошибка валидации даты",
                   MessageBoxButton.OK,
                   MessageBoxImage.Warning);
        }

        private bool _isHandlingEvents = true;

        private void datePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isHandlingEvents)
            {
                return;
            }

            var selectedDate = datePicker.SelectedDate;

            if (selectedDate.HasValue && selectedDate.Value > DateTime.Now.AddYears(-18))
            {
                _isHandlingEvents = false;
                try
                {
                    MessageBox.Show(
                        "Выбранная дата не соответствует требованиям.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    // Возвращаемся к минимальной допустимой дате
                    datePicker.SelectedDate = DateTime.Now.AddYears(-18).AddDays(1);
                }
                finally
                {
                    _isHandlingEvents = true;
                }
            }
        }

        private void datePicker_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MessageBox.Show(
                      "Выбранная дата не соответствует требованиям.",
                      "Ошибка",
                      MessageBoxButton.OK,
                      MessageBoxImage.Error
                  );
        }

        private void datePicker_SelectedDateChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранную дату
            var datePicker = (DatePicker)sender;
            var selectedDate = datePicker.SelectedDate;


            if (selectedDate.HasValue && selectedDate.Value > DateTime.Now.AddYears(-18))
            {
                // Показываем сообщение пользователю
                MessageBox.Show("Вам должно быть не менее 18 лет, чтобы зарегистрироваться.",
                                "Возрастное ограничение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);


                datePicker.SelectedDate = null;
            }
        }

        private void eyeBtn_Click(object sender, RoutedEventArgs e)
        {

            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordHide.Visibility = Visibility.Visible;

            txtRptPassword.Visibility = Visibility.Collapsed;
            txtRptPasswordHide.Visibility = Visibility.Visible;

            txtPasswordHide.Text = new NetworkCredential(string.Empty, txtPassword.SecurePassword).Password;
            txtRptPasswordHide.Text = new NetworkCredential(string.Empty, txtRptPassword.SecurePassword).Password;

            eyeBtnDis.Visibility = Visibility.Visible;
            eyeBtnDis.IsEnabled = true;
            eyeBtn.Visibility = Visibility.Collapsed;
            eyeBtn.IsEnabled = false;
        }

        private void eyeBtnDis_Click(object sender, RoutedEventArgs e)
        {
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordHide.Visibility = Visibility.Collapsed;

            txtRptPassword.Visibility = Visibility.Visible;
            txtRptPasswordHide.Visibility = Visibility.Collapsed;

            txtPassword.Password = new NetworkCredential(string.Empty, txtPasswordHide.Text).Password;
            txtRptPassword.Password = new NetworkCredential(string.Empty, txtRptPasswordHide.Text).Password;

            txtPasswordHide.Text = "";
            txtRptPasswordHide.Text = "";

            eyeBtnDis.Visibility = Visibility.Collapsed;
            eyeBtnDis.IsEnabled = false;
            eyeBtn.Visibility = Visibility.Visible;
            eyeBtn.IsEnabled = true;

        }
    }
}
