
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
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using Image = System.Windows.Controls.Image;

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
          // this.DataContext = _context;
            listGenre.ItemsSource = AppConnect.cinemaEntities.Genres.ToList();
            listActors.ItemsSource = AppConnect.cinemaEntities.Actors.ToList();
            listDirectors.ItemsSource = AppConnect.cinemaEntities.Directors.ToList();
        }


        public NewFilmPage(Movies movie)
        {
            InitializeComponent();
            txtName.Text = movie.Title;
            listGenre.ItemsSource = AppConnect.cinemaEntities.Genres.ToList();

            // Устанавливаем выбранный жанр
            if (movie.MoviesGenres != null && movie.MoviesGenres.Any())
            {
                // Предполагаем, что у фильма может быть несколько жанров
                // Выбираем все соответствующие жанры в списке
                foreach (var genre in listGenre.Items)
                {
                    if (genre is Genres g && movie.MoviesGenres.Any(mg => mg.GenreID == g.GenreID))
                    {
                        listGenre.SelectedItems.Add(genre);
                    }
                }
            }
        }



            private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         
        }


        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string imagePath = openFileDialog.FileName;
                _posterImageData = LoadToBase64(imagePath);

                // Показываем изображение в интерфейсе
                imgPoster.Source = new BitmapImage(new Uri(imagePath));
                imgPoster.Visibility = Visibility.Visible;
                txtImagePath.Text = System.IO.Path.GetFileName(imagePath);
            }
        }

        private void btnSaveBtnFilm_Click(object sender, RoutedEventArgs e)
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

                if(_posterImageData == null)
                {
                    var imageUri = "/Images/poster-blank.png";
                    Uri uri = new Uri("pack://application:,,," + imageUri, UriKind.Absolute);

                    // Получаем поток ресурса
                    var streamInfo = System.Windows.Application.GetResourceStream(uri);
                    if (streamInfo == null)
                    {
                        throw new FileNotFoundException($"Resource not found: {imageUri}");
                    }

                    // Читаем байты
                    using (var memoryStream = new MemoryStream())
                    {
                        streamInfo.Stream.CopyTo(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        _posterImageData =  Convert.ToBase64String(imageBytes);
                    }
                   
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

                //Добавление актеров
                foreach (Actors selectedActors in listActors.SelectedItems)
                {
                    newFilm.MoviesActors.Add(new MoviesActors
                    {
                        MoviesID = newFilm.MovieID,
                        ActorsID = selectedActors.ActorID
                    });
                }

                //Добавление режиссеров
                foreach (Directors selectedDirectors in listDirectors.SelectedItems)
                {
                    newFilm.MoviesDirectors.Add(new MoviesDirectors
                    {
                        MoviesID = newFilm.MovieID,
                        DirectorsID = selectedDirectors.DirectorID
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }


        private void ClearFields()
        {
            txtName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtYear.Text = string.Empty;
            txtDuration.Text = string.Empty;
            txtRating.Text = string.Empty;
            listGenre.SelectedItems.Clear();
            listActors.SelectedItems.Clear();
            listDirectors.SelectedItems.Clear();
            imgPoster.Source = new BitmapImage(new Uri("/Images/poster-blank.png", UriKind.RelativeOrAbsolute));
            txtImagePath.Text = string.Empty;
            _posterImageData = null;
        }

        private string LoadToBase64(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);

            return base64String;
        }
    }
}
