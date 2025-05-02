using CinemaServiceWork.ApplicationData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for FavoritePage.xaml
    /// </summary>
    public partial class FavoritePage : Page
    {

        private Users _user;
        private ObservableCollection<Movies> _films = new ObservableCollection<Movies>();

        public FavoritePage(Users user)
        {
            InitializeComponent();
            _user = user;
            _films = new ObservableCollection<Movies>(AppConnect.cinemaEntities.Movies.ToList());
        }


        private void LoadFavoriteFilms()
        {
            // Очищаем текущую коллекцию
           // _films.Clear();

            // Получаем избранные фильмы для текущего пользователя
            var favoriteMovies = AppConnect.cinemaEntities.Favorites
                .Where(f => f.UserID == _user.UserID)
                .Select(f => f.Movies)
                .ToList();

            // Добавляем в коллекцию
            foreach (var movie in favoriteMovies)
            {
                _films.Add(movie);
            }

            // Привязываем коллекцию к ListBox (или другому контролу)
            filmsListView.ItemsSource = _films;
        }

        private void ImageBtn_Click(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;
            Image imagePoster = (Image)button.Content;


            var zoomWindow = new Window
            {
                Title = "Увеличенный постер",
                Width = 400,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };


            var zoomImage = new Image
            {
                Source = imagePoster.Source,
                Stretch = Stretch.Uniform,
                //   HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            zoomWindow.Content = zoomImage;
            zoomWindow.ShowDialog();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
        }

    }

}
