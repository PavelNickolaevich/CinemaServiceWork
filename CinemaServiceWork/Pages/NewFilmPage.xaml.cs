
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using CinemaServiceWork.Utils;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for NewFilmPage.xaml
    /// </summary>
    public partial class NewFilmPage : Page
    {

        private string _posterImageData;
        private CinemaEntities _context = new CinemaEntities();
        public NewFilmPage()
        {
            InitializeComponent();
            this.DataContext = AppConnect.cinemaEntities;
            listGenre.ItemsSource = AppConnect.cinemaEntities.Genres.ToList();
        }



        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         
        }

        /*     private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
             {
                 var openFileDialog = new OpenFileDialog
                 {
                     Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                     Title = "Выберите постер фильма"
                 };

                 if (openFileDialog.ShowDialog() == DialogResult.OK)
                 {
                     txtImagePath.Text = openFileDialog.FileName;

                     // Показываем превью
                     var bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                     imgPoster.Source = bitmap;
                 }

             }*/

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string imagePath = openFileDialog.FileName;

                // Конвертируем изображение в Base64 строку
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string base64String = Convert.ToBase64String(imageBytes);

                // Сохраняем строку (можно сохранять сразу в свойство newFilm.Poster при сохранении)
                _posterImageData = base64String;

                // Показываем изображение в интерфейсе
                imgPoster.Source = new BitmapImage(new Uri(imagePath));
                imgPoster.Visibility = Visibility.Visible;
                txtImagePath.Text = System.IO.Path.GetFileName(imagePath);
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(txtName.Text) ||
                    string.IsNullOrWhiteSpace(txtDescription.Text) ||
                    string.IsNullOrWhiteSpace(txtYear.Text) ||
                    string.IsNullOrWhiteSpace(txtDuration.Text) ||
                    string.IsNullOrWhiteSpace(txtRating.Text) ||
               //    _posterImageData == null ||
                    listGenre.SelectedItems.Count == 0)
                {
                    System.Windows.MessageBox.Show("Пожалуйста, заполните все поля и выберите хотя бы один жанр.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка числовых полей
          /*      if (!int.TryParse(txtYear.Text, out int year) ||
                    !int.TryParse(txtDuration.Text, out int duration) ||
                    !double.TryParse(txtRating.Text, out double rating) ||
                    rating < 0 || rating > 10)
                {
                    System.Windows.MessageBox.Show("Пожалуйста, проверьте правильность ввода числовых полей. Рейтинг должен быть от 0 до 10.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }*/

                // Создание нового фильма
                var newFilm = new Movies
                {
                    Title = txtName.Text,
                    Description = txtDescription.Text,
                    Release_year = Convert.ToInt32(txtYear.Text),
                    Duration = Convert.ToInt32(txtDuration.Text),
                    Rating = Convert.ToDecimal(txtRating.Text),
                    Poster = _posterImageData as string
                };

                //Добавление жанров
                foreach (Genres selectedGenre in listGenre.SelectedItems)
                {
                    newFilm.MoviesGenres.Add(new MoviesGenres
                    {
                        MovieID = newFilm.MovieID,
                        GenreID = selectedGenre.GenreID
                    });
                }

                _context.Movies.Add(newFilm);
                _context.SaveChanges();

                System.Windows.MessageBox.Show("Фильм успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Произошла ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnViewPoster_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_posterImageData as string))
            {
                var viewer = new Window
                {
                    Title = "Просмотр постера",
                    Content = new Image { Source = Utils.UtilsMethods.LoadImageFromBase64(_posterImageData as string) },
                    SizeToContent = SizeToContent.WidthAndHeight
                };
                viewer.ShowDialog();
            }
        }


        private void ClearFields()
        {
            txtName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtYear.Text = string.Empty;
            txtDuration.Text = string.Empty;
            txtRating.Text = string.Empty;
            listGenre.SelectedItems.Clear();
            imgPoster.Source = null;
            imgPoster.Visibility = Visibility.Collapsed;
            txtImagePath.Text = string.Empty;
            _posterImageData = null;
        }
    }
}
