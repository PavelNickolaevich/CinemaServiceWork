
using CinemaServiceWork.ApplicationData;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private Users _user;
        public Menu(Users user)
        {
            InitializeComponent();
            _user = user;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {

            AppFrame.mainFrame.Navigate(new Pages.AuthorizationPage());

        }

        private void MyPageFilmsBtn_Click(object sender, RoutedEventArgs e)
        {
            //try
            {
                AppFrame.contentFrame.Navigate(new MyFilmsPage(_user));
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Ошибка: {ex.Message}");
            //}
        }


        private void FavoritesBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new FavoritePage(_user));
        }

        private void ForumBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new ForumPage(_user));
        }

        private void AddFilmBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new NewFilmPage(_user));
        }

        private void AddActorBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new NewActorPage(_user));
        }

        private void AddDirectorBtn_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new NewDirectorPage(_user));
        }

        private void KinopoiskQRBtn_Click(object sender, RoutedEventArgs e)
        {
            // URL Кинопоиска
            string kinopoiskUrl = "https://www.kinopoisk.ru/";

            // Генерация QR-кода
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(kinopoiskUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            // Получаем изображение как Bitmap
            Bitmap qrCodeImageBitmap = qrCode.GetGraphic(20);

            // Конвертируем в BitmapImage для WPF
            using (MemoryStream memory = new MemoryStream())
            {
                qrCodeImageBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                kinopoiskQRImage.Source = bitmapImage;
            }

            // Открываем попап
            qrCodePopup.IsOpen = true;
        }

        private void CloseQRPopup_Click(object sender, RoutedEventArgs e)
        {
            qrCodePopup.IsOpen = false;
        }
    }
}
