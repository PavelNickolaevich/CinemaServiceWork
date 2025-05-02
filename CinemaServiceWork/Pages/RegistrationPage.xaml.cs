using CinemaServiceWork.ApplicationData;
using CinemaServiceWork.Utils;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

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
                    Password = txtPassword.Password, // Исправлено: было txtEmail.Text
                    Email = txtEmail.Text
                };

                AppConnect.cinemaEntities.Users.Add(user);
                AppConnect.cinemaEntities.SaveChanges();

                // Генерируем данные для QR-кода (можно использовать ID, email или комбинацию данных)
                string qrData = $"UserID:{user.UserID};Email:{user.Email}";
                ShowQRCode(qrData);

                MessageBox.Show(
                    "Данные успешно добавлены. Ваш QR-код сгенерирован ниже.",
                    "Уведомление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
                // Не возвращаемся сразу назад, чтобы пользователь мог увидеть QR-код
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

        // In your registration button click handler or after successful registration
        private void ShowQRCode(string userData)
        {
            // Generate QR code
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 300,
                    Margin = 0
                }
            };

            var bitmap = writer.Write(userData);

            // Convert to BitmapImage
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                qrCodeImage.Source = bitmapImage;
            }

            qrCodeText.Text = userData; // Or any other text you want to display
            qrCodeBorder.Visibility = Visibility.Visible;
        }


    }
}
