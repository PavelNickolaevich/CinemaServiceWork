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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Button = System.Windows.Controls.Button;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for MyFilmsPage.xaml
    /// </summary>
    public partial class MyFilmsPage : Page
    {
       
        private List<Movies> _films;


        public MyFilmsPage()
        {
            this.DataContext = AppConnect.cinemaEntities;
            InitializeComponent();
            LoadFilms();
            UpdateFilmsListVisibility();
            
        }

        private void LoadFilms()
        {
            _films = AppConnect.cinemaEntities.Movies.ToList();

            foreach (var film in _films)
           {
                //  Получаем список жанров для текущего фильма
                film.Genres = string.Join(", ", film.MoviesGenres.Select(mg => mg.Genres.Name));
           }

            filmsListView.ItemsSource = _films;
           
        }

        private void AddFilm_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new NewFilmPage());
           
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void DeleteFilm_Click(object sender, RoutedEventArgs e)
        {
            
            var result = MessageBox.Show("Удалить этот фильм?", "Подтверждение",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            var film = filmsListView.SelectedItem as Movies;
          //  var filmId = (int)((Button)sender).Tag;
        //    var movie = AppConnect.cinemaEntities.Movies.FirstOrDefault(film => film.MovieID == filmId);

            if (result == MessageBoxResult.Yes)
            {
                AppConnect.cinemaEntities.Movies.Remove(film);
                AppConnect.cinemaEntities.SaveChanges();
                LoadFilms();
            }

            UpdateFilmsListVisibility();
        }

        private void EditFilm_Click(object sender, RoutedEventArgs e)
        {
   
            var movie = filmsListView.SelectedItem as Movies;
            if (movie != null)
            {
                NavigationService.Navigate(new Pages.NewFilmPage(movie));
            }
        }

        private void PublishFilms_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var filmId = (int)((Button)sender).Tag;
         //   var film = _films.FirstOrDefault(f => f.Id == filmId);

     //      if (film != null)
            {
         //       NavigationService.Navigate(new FilmDetailsPage(film));
            }
        }

        private void UpdateFilmsListVisibility()
        {
            if (filmsListView.Items.Count == 0)
            {
                noFilmsText.Visibility = Visibility.Visible;
                btnAddFilms.Visibility = Visibility.Visible;
                filmsListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                noFilmsText.Visibility = Visibility.Collapsed;
                btnAddFilms.Visibility = Visibility.Collapsed;
                filmsListView.Visibility = Visibility.Visible;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Отключаем кнопки у всех элементов
            foreach (var item in filmsListView.Items)
            {
                var listViewItem = filmsListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (listViewItem != null)
                {
                    var fullViewBtn = FindVisualChild<Button>(listViewItem, "fullViewBtn");
                    var publishBtn = FindVisualChild<Button>(listViewItem, "publishBtn");
                    var editBtn = FindVisualChild<Button>(listViewItem, "editBtn");
                    var deleteBtn = FindVisualChild<Button>(listViewItem, "deleteBtn");
                    if (fullViewBtn != null)
                    {
                        fullViewBtn.IsEnabled = false;
                         publishBtn.IsEnabled = false;
                        editBtn.IsEnabled = false;
                        deleteBtn.IsEnabled = false;
                    }
                }
            }

            // Включаем кнопку только у выбранного элемента
            if (filmsListView.SelectedItem != null)
            {
                var selectedListViewItem = filmsListView.ItemContainerGenerator.ContainerFromItem(filmsListView.SelectedItem) as ListViewItem;
                if (selectedListViewItem != null)
                {
                    var fullViewBtn = FindVisualChild<Button>(selectedListViewItem, "fullViewBtn");
                    var publishBtn = FindVisualChild<Button>(selectedListViewItem, "publishBtn");
                    var editBtn = FindVisualChild<Button>(selectedListViewItem, "editBtn");
                    var deleteBtn = FindVisualChild<Button>(selectedListViewItem, "deleteBtn");
                   
                    if (fullViewBtn != null)
                    {
                        fullViewBtn.IsEnabled = true;
                        publishBtn.IsEnabled = true;
                        editBtn.IsEnabled = true;
                        deleteBtn.IsEnabled = true;
                    }
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (child != null && child is T && child.Name == childName)
                {
                    return (T)child;
                }

                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void btnAddFilm_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new Pages.NewFilmPage());
        }

        private void imageBtn_Click(object sender, RoutedEventArgs e)
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
        }
}
