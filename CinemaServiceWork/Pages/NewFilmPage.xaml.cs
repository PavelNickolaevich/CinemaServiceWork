
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
using CinemaServiceWork.Utils.Converters;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Xceed.Wpf.Toolkit;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for NewFilmPage.xaml
    /// </summary>
    public partial class NewFilmPage : Page
    {

        private string _posterImageData;
        private CinemaEntities _context = new CinemaEntities();
        private Movies _movie;
        private Users _user;


        public NewFilmPage(Users user)
        {
            InitializeComponent();
            LoadLists();
            SetDefaultPoster();
            _user = user;
        }


        public NewFilmPage(Movies movie)
        {
            InitializeComponent();
            LoadLists();
          //  _context = new CinemaEntities();
         //   _movie = _context.Movies.Find(movie.MovieID);

            foreach (var actor in listActors.Items)
            {
                if (actor is Actors a && movie.MoviesActors.Any(ma => ma.ActorsID == a.ActorID))
                {
                    listActors.SelectedItems.Add(actor);
                }
            }

            foreach (var director in listDirectors.Items)
            {
                if (director is Directors d && movie.MoviesDirectors.Any(md => md.DirectorsID == d.DirectorID))
                {
                    listDirectors.SelectedItems.Add(director);
                }
            }

            foreach (var genre in listGenre.Items)
            {
                if (genre is Genres g && movie.MoviesGenres.Any(mg => mg.GenreID == g.GenreID))
                {
                    listGenre.SelectedItems.Add(genre);
                }
            }

            DataContext = movie;
            _posterImageData = movie.Poster;
            _movie = movie;
       

            btnSaveFilm.Content = movie != null ? "Изменить" : "Создать";
        }

        private void LoadLists()
        {
            listGenre.ItemsSource = _context.Genres.ToList();
            listActors.ItemsSource = _context.Actors.ToList();
            listDirectors.ItemsSource = _context.Directors.ToList();
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
            using (var transaction = _context.Database.BeginTransaction())
                try
            {
                // Валидация данных
                if (!ValidateInput())
                    return;

                // Обработка постера
                if (_posterImageData == null)
                {
                    SetDefaultPoster();
                }

                // Сохранение фильма
                if (_movie == null)
                {
                    CreateNewMovie();
                }
                else
                {
                    UpdateExistingMovie();
                }
              
                _context.SaveChanges();
                    transaction.Commit();
                    System.Windows.MessageBox.Show("Фильм успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Произошла ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtDescription.Text) ||
                string.IsNullOrWhiteSpace(txtYear.Text) ||
                string.IsNullOrWhiteSpace(txtDuration.Text) ||
                string.IsNullOrWhiteSpace(txtRating.Text) ||
                listGenre.SelectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("Пожалуйста, заполните все поля и выберите хотя бы один жанр.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(txtYear.Text, out int year) || year < 1888 || year > DateTime.Now.Year + 5)
            {
                System.Windows.MessageBox.Show($"Год выпуска должен быть числом от 1888 до {DateTime.Now.Year + 5}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(txtDuration.Text, out int duration) || duration <= 0 || duration > 1000)
            {
                System.Windows.MessageBox.Show("Длительность должна быть положительным числом (в минутах) не более 1000", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(txtRating.Text, out decimal rating) || rating < 0 || rating > 10)
            {
                System.Windows.MessageBox.Show("Рейтинг должен быть числом от 0 до 10", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void SetDefaultPoster()
        {
            var imageUri = "/Images/poster-blank.png";
            Uri uri = new Uri("pack://application:,,," + imageUri, UriKind.Absolute);

            var streamInfo = System.Windows.Application.GetResourceStream(uri);
            if (streamInfo == null)
            {
                throw new FileNotFoundException($"Resource not found: {imageUri}");
            }

            using (var memoryStream = new MemoryStream())
            {
                streamInfo.Stream.CopyTo(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                _posterImageData = Convert.ToBase64String(imageBytes);
            }
        }

        private void CreateNewMovie()
        {
            var newFilm = new Movies
            {
                Title = txtName.Text,
                Description = txtDescription.Text,
                Release_year = Convert.ToInt32(txtYear.Text),
                Duration = Convert.ToInt32(txtDuration.Text),
                Rating = Convert.ToInt32(txtRating.Text),
                Poster = _posterImageData as string
            };

            AddRelatedEntities(newFilm);
            _context.Movies.Add(newFilm);
        }

        private void UpdateExistingMovie()
        {
            _movie.Title = txtName.Text;
            _movie.Description = txtDescription.Text;
            _movie.Release_year = Convert.ToInt32(txtYear.Text);
            _movie.Duration = Convert.ToInt32(txtDuration.Text);
            _movie.Rating = Convert.ToInt32(txtRating.Text);
            _movie.Poster = _posterImageData as string;

            _context.Movies.AddOrUpdate(_movie);
            UpdateRelatedEntities(_movie);

        }

        private void AddRelatedEntities(Movies movie)
        {
            foreach (Genres selectedGenre in listGenre.SelectedItems)
            {
                movie.MoviesGenres.Add(new MoviesGenres
                {
                    MovieID = movie.MovieID,
                    GenreID = selectedGenre.GenreID
                });
            }

            foreach (Actors selectedActors in listActors.SelectedItems)
            {
                movie.MoviesActors.Add(new MoviesActors
                {
                    MoviesID = movie.MovieID,
                    ActorsID = selectedActors.ActorID
                });
            }

            foreach (Directors selectedDirectors in listDirectors.SelectedItems)
            {
                movie.MoviesDirectors.Add(new MoviesDirectors
                {
                    MoviesID = movie.MovieID,
                    DirectorsID = selectedDirectors.DirectorID
                });
            }
        }

        private void UpdateRelatedEntities(Movies movie)
        {
            var selectedGenreIds = listGenre.SelectedItems.Cast<Genres>().Select(g => g.GenreID).ToList();
            var existingGenres = _context.MoviesGenres.Where(mg => mg.MovieID == movie.MovieID).ToList();
            var selectedActorsIds = listActors.SelectedItems.Cast<Actors>().Select(g => g.ActorID).ToList();
            var existingActors = _context.MoviesActors.Where(ma => ma.MoviesID == movie.MovieID).ToList();
            var selectedDirectorsIds = listDirectors.SelectedItems.Cast<Directors>().Select(g => g.DirectorID).ToList();
            var existingDirectors = _context.MoviesDirectors.Where(md => md.MoviesID == movie.MovieID).ToList();

            
            // Удаляем невыбранные
            foreach (var genre in existingGenres.Where(g => !selectedGenreIds.Contains(g.GenreID)))
            {
                _context.MoviesGenres.Remove(genre);
            }

            // Добавляем новые
            foreach (var genreId in selectedGenreIds.Except(existingGenres.Select(g => g.GenreID)))
            {
                _context.MoviesGenres.AddOrUpdate(new MoviesGenres
                {
                    MovieID = movie.MovieID,
                    GenreID = genreId
                });
            }

            // актеры
            foreach (var actor in existingActors.Where(a => !selectedActorsIds.Contains(a.ActorsID)))
            {
                _context.MoviesActors.Remove(actor);
            }

            foreach (var actorId in selectedActorsIds.Except(existingActors.Select(a => a.ActorsID)))
            {
                _context.MoviesActors.AddOrUpdate(new MoviesActors
                {
                    MoviesID = movie.MovieID,
                    ActorsID = actorId
                });
            }

            // режиссеры
            foreach (var director in existingDirectors.Where(d => !selectedDirectorsIds.Contains(d.DirectorsID)))
            {
                _context.MoviesDirectors.Remove(director);
            }

            foreach (var directorId in selectedDirectorsIds.Except(existingDirectors.Select(d => d.DirectorsID)))
            {
                _context.MoviesDirectors.AddOrUpdate(new MoviesDirectors
                {
                    MoviesID = movie.MovieID,
                    DirectorsID = directorId
                });
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

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
