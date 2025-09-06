using CinemaServiceWork.ApplicationData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using CinemaServiceWork.Utils.Converters;
using Paragraph = iTextSharp.text.Paragraph;
using Path = System.IO.Path;
using Image = System.Windows.Controls.Image;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Controls.Primitives;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Diagnostics;
using Control = System.Windows.Forms.Control;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for MyFilmsPage.xaml
    /// </summary>
    public partial class MyFilmsPage : Page
    {

        private ObservableCollection<Movies> _films = new ObservableCollection<Movies>();
        private CinemaEntities _context = new CinemaEntities();
        private bool _sortByNameAscending = true;
        private bool _sortByRatingAscending = true;
        private bool isInitFilter = false;
        private Users _user;


        public MyFilmsPage(Users user)
        {
            _user = user;
            _films = new ObservableCollection<Movies>(AppConnect.cinemaEntities.Movies.ToList());
            InitializeComponent();
            LoadFilms();
            ForceRefresh();
            UpdateFilmsListVisibility();
            isInitFilter = true;
            AddGenres();
            AddYears();
            LoadFavorite();

        }

        private ObservableCollection<Movies> Movies()
        {
            return new ObservableCollection<Movies>(AppConnect.cinemaEntities.Movies.ToList());
        }

        private void AddGenres()
        {
            genreFilterComboBox.Items.Add("Все жанры");

            foreach (var item in AppConnect.cinemaEntities.Genres)
            {
                genreFilterComboBox.Items.Add(item.Name);
            }

            genreFilterComboBox.SelectedIndex = 0;

        }

        private void AddYears()
        {
            yearFilterComboBox.Items.Add("Все года");
            yearFilterComboBox.Items.Add("2020-2026");
            yearFilterComboBox.Items.Add("2010-2019");
            yearFilterComboBox.Items.Add("2000-2009");
            yearFilterComboBox.Items.Add("1990-1999");
            yearFilterComboBox.Items.Add("1980-1989");
            yearFilterComboBox.Items.Add("1970-1979");
            yearFilterComboBox.Items.Add("1960-1969");
            yearFilterComboBox.Items.Add("1950-1959");
            yearFilterComboBox.Items.Add("1940-1949");
            yearFilterComboBox.Items.Add("1930-1939");

            yearFilterComboBox.SelectedIndex = 0;
        }

        private void LoadFilms()
        {
       
            foreach (var film in this._films)
           {
                film.Genres = string.Join(", ", film.MoviesGenres.Select(mg => mg.Genres.Name));

                film.Directors = string.Join(", ",
                  film.MoviesDirectors?
                      .Where(md => md?.Directors != null)
                      .Select(md =>
                      {
                          var nameParts = new List<string>();
                          if (!string.IsNullOrWhiteSpace(md.Directors.First_name))
                              nameParts.Add(md.Directors.First_name);
                          if (!string.IsNullOrWhiteSpace(md.Directors.Last_name))
                              nameParts.Add(md.Directors.Last_name);
                          if (!string.IsNullOrWhiteSpace(md.Directors.Patronymic))
                              nameParts.Add(md.Directors.Patronymic);

                          string fullName = nameParts.Count > 0
                              ? string.Join(" ", nameParts)
                              : "Режиссер не указан";

                          // Получаем страны режиссера
                          string countries = md.Directors.Countries != null
                                              ? md.Directors.Countries.Name
                                              : "Страна не указана";

                          return $"{fullName} ({countries})";
                      })
                      .DefaultIfEmpty("Режиссер не указан"));

                film.Actors = string.Join(", ",
                  film.MoviesActors?
                      .Where(md => md?.Actors != null)
                      .Select(md =>
                      {
                          var nameParts = new List<string>();
                          if (!string.IsNullOrWhiteSpace(md.Actors.First_name))
                              nameParts.Add(md.Actors.First_name);
                          if (!string.IsNullOrWhiteSpace(md.Actors.Last_name))
                              nameParts.Add(md.Actors.Last_name);
                          if (!string.IsNullOrWhiteSpace(md.Actors.Patronymic))
                              nameParts.Add(md.Actors.Patronymic);

                          string fullName = nameParts.Count > 0
                              ? string.Join(" ", nameParts)
                              : "Актер не указан";

                          string countries = md.Actors.Countries != null
                                              ? md.Actors.Countries.Name
                                              : "Страна не указана";

                          return $"{fullName} ({countries})";
                      })
                      .DefaultIfEmpty("Режиссер не указан"));
            }

            filmsListView.ItemsSource = this._films;

        }


       private void LoadFavorite()
        {

            var favoriteMovieIds = _context.Favorites
                   .Where(f => f.UserID == _user.UserID)
                   .Select(f => f.MovieID)
                   .ToList();

            foreach (var film in _films)
            {
                if (favoriteMovieIds.Contains(film.MovieID))
                {
                    // Если фильм в избранном, показываем заполненное сердечко
                    UpdateHeartButtons(film, true);
                    
                }
            }

        }

        private void UpdateHeartButtons(Movies film, bool isFavorite)
        {
            if (filmsListView == null) return;

            // Ждем, пока элементы будут готовы
            if (filmsListView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                filmsListView.ItemContainerGenerator.StatusChanged += (s, e) =>
                {
                    if (filmsListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    {
                        UpdateHeartButtons(film, isFavorite);
                    }
                };
                return;
            }

            // Получаем контейнер элемента ListView
            var container = filmsListView.ItemContainerGenerator.ContainerFromItem(film) as ListViewItem;
            if (container == null) return;

            // Ищем StackPanel в визуальном дереве
            var stackPanel = FindVisualChild<StackPanel>(container);
            if (stackPanel == null) return;

            // Находим кнопки
            var emptyHeart = stackPanel.FindName("emptyHeartBtn") as Button;
            var fullHeart = stackPanel.FindName("fullHeartBtn") as Button;

            if (emptyHeart != null && fullHeart != null)
            {
                emptyHeart.Visibility = isFavorite ? Visibility.Collapsed : Visibility.Visible;
                fullHeart.Visibility = isFavorite ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // Вспомогательный метод для поиска дочерних элементов
        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }


        public void ForceRefresh()
        {
            AppConnect.cinemaEntities = new CinemaEntities();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            filmsListView.ItemsSource = _films.Where(f => f.Title.ToLower().Contains(searchTextBox.Text.ToLower())).ToList();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                IEnumerable<Movies> filteredFilms = _films;

                if (isInitFilter)
                {
                    if (genreFilterComboBox.SelectedItem != null)
                    {
                        string selectedGenre = genreFilterComboBox.SelectedItem.ToString();

                        if (selectedGenre != "Все жанры")
                        {
                            filteredFilms = filteredFilms
                                .Where(f => f.MoviesGenres != null &&
                                           f.MoviesGenres.Any(mg => mg.Genres != null &&
                                                                   mg.Genres.Name == selectedGenre));
                        }
                    }

                    if (yearFilterComboBox.SelectedItem != null)
                    {
                        string selectedYears = yearFilterComboBox.SelectedItem.ToString();

                        if (selectedYears != "Все года")
                        {
                            var years = selectedYears.Split('-');
                            if (years.Length == 2 && int.TryParse(years[0], out int startYear) && int.TryParse(years[1], out int endYear))
                            {
                                filteredFilms = filteredFilms.Where(f => f.Release_year >= startYear && f.Release_year <= endYear);
                            }
                        }
                    }


                        if (ratingFilterComboBox.SelectedItem.ToString() != "Любой")
                        {
                            var rating = ratingFilterComboBox.SelectedItem.ToString().Split(' ');
                            if (rating.Length == 3 && int.TryParse(rating[1], out int rate))
                            {
                                filteredFilms = filteredFilms.Where(f => f.Rating == rate);
                            }
                        }
                   

                    filmsListView.ItemsSource = filteredFilms;
                    noFilmsText.Visibility = filteredFilms.Any() ? Visibility.Collapsed : Visibility.Visible;
                }

            }
            
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}");
            }
        }

        private void EmptyHeartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var emptyHeartBtn = (Button)sender;
                var film = (Movies)emptyHeartBtn.DataContext;

                emptyHeartBtn.Visibility = Visibility.Collapsed;

                var parent = (FrameworkElement)emptyHeartBtn.Parent;
                var fullHeartBtn = (Button)parent.FindName("fullHeartBtn");
                fullHeartBtn.Visibility = Visibility.Visible;

                var newFavorite = new Favorites
                {
                    UserID = _user.UserID,
                    MovieID = film.MovieID,
                };

                _context.Favorites.Add(newFavorite);
                _context.SaveChanges();
            } 
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FullHeartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fullHeartBtn = (Button)sender;
                var film = (Movies)fullHeartBtn.DataContext;

                fullHeartBtn.Visibility = Visibility.Collapsed;
                var parent = (FrameworkElement)fullHeartBtn.Parent;
                var emptyHeartBtn = (Button)parent.FindName("emptyHeartBtn");
                emptyHeartBtn.Visibility = Visibility.Visible;
       
                var favorite = _context.Favorites
                    .FirstOrDefault(f => f.UserID == _user.UserID && f.MovieID == film.MovieID);

                if (favorite != null)
                {
                    _context.Favorites.Remove(favorite);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

    }

        private void SortByName_Click(object sender, RoutedEventArgs e)

        {
            if (_sortByNameAscending)
            {
                filmsListView.ItemsSource = _films.Where(f => f.Title != null)
                                                 .OrderBy(f => f.Title)
                                                 .ToList();
                // Поворачиваем стрелку вверх
                nameSortArrow.RenderTransform = new RotateTransform(180);
            }
            else
            {
                filmsListView.ItemsSource = _films.Where(f => f.Title != null)
                                                 .OrderByDescending(f => f.Title)
                                                 .ToList();
                // Поворачиваем стрелку вниз
                nameSortArrow.RenderTransform = new RotateTransform(0);
            }

            _sortByNameAscending = !_sortByNameAscending;

            // Сбрасываем сортировку по рейтингу
            ratingSortArrow.RenderTransform = new RotateTransform(0);
        }

        private void SortByRating_Click(object sender, RoutedEventArgs e)
        {
            if (_sortByRatingAscending)
            {
                filmsListView.ItemsSource = _films.Where(f => f.Rating != null)
                                                 .OrderBy(f => f.Rating)
                                                 .ToList();
                // Поворачиваем стрелку вверх
                ratingSortArrow.RenderTransform = new RotateTransform(180);
            }
            else
            {
                filmsListView.ItemsSource = _films.Where(f => f.Rating != null)
                                                 .OrderByDescending(f => f.Rating)
                                                 .ToList();
                // Поворачиваем стрелку вниз
                ratingSortArrow.RenderTransform = new RotateTransform(0);
            }

            _sortByRatingAscending = !_sortByRatingAscending;

            // Сбрасываем сортировку по названию
            nameSortArrow.RenderTransform = new RotateTransform(0);
        }

        private void AddFilm_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new NewFilmPage(_user));
           
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void DeleteFilm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
  
                var film = filmsListView.SelectedItem as Movies;

                if (film == null)
                {
                    MessageBox.Show("Фильм не выбран", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Вы действительно хотите удалить фильм \"{film.Title}\"?",
                                            "Подтверждение удаления",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Question,
                                            MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
 
                    using (var context = new CinemaEntities())
                    {
                        var filmToDelete = context.Movies.Find(film.MovieID);

                        if (filmToDelete != null)
                        {
                            context.MoviesGenres.RemoveRange(filmToDelete.MoviesGenres);
                            context.MoviesActors.RemoveRange(filmToDelete.MoviesActors);
                            context.MoviesDirectors.RemoveRange(filmToDelete.MoviesDirectors);

                            context.Movies.Remove(filmToDelete);
                            context.SaveChanges();

                            _films.Remove(film);
                            filmsListView.Items.Refresh();

                            MessageBox.Show("Фильм успешно удален", "Успех",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
            {
                MessageBox.Show("Ошибка при удалении фильма. Возможно, есть связанные записи.\n" +
                               dbEx.InnerException?.Message,
                               "Ошибка базы данных",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении фильма: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UpdateFilmsListVisibility();
            }
        }

        private void EditFilm_Click(object sender, RoutedEventArgs e)
        {
            var movie = filmsListView.SelectedItem as Movies;
            if (movie != null)
            {
                NavigationService.Navigate(new Pages.NewFilmPage(movie));
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedFilm = filmsListView.SelectedItem as Movies;
           
                if(selectedFilm != null)
                {
                    DataContext = selectedFilm;
                    fullInfoPopup.IsOpen = true;
                }
                else
                {
                    MessageBox.Show("Фильм не найден", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseInfoPopup_Click(object sender, RoutedEventArgs e)
        {
            fullInfoPopup.IsOpen = false;
        }

        private void SaveToPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedFilm = filmsListView.SelectedItem as Movies;
                if (selectedFilm == null)
                {
                    MessageBox.Show("Фильм не выбран", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF файлы (*.pdf)|*.pdf",
                    FileName = $"{selectedFilm.Title.Replace(" ", "_")}_info.pdf",
                    DefaultExt = ".pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Указываем путь к шрифту с поддержкой кириллицы (Arial Unicode MS или другой)
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");

                    // Если шрифт не найден в системной папке, пробуем найти в папке приложения
                    if (!File.Exists(fontPath))
                    {
                        fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "arial.ttf");
                    }

                    // Создаем базовый шрифт с поддержкой кириллицы
                    BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font titleFont = new Font(baseFont, 18, Font.BOLD);
                    Font boldFont = new Font(baseFont, 12, Font.BOLD);
                    Font normalFont = new Font(baseFont, 12, Font.NORMAL);

                    // Создаем PDF документ
                    Document document = new Document();
                    PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    document.Open();

                    // Добавляем заголовок
                    document.Add(new Paragraph(selectedFilm.Title, titleFont));
                    document.Add(Chunk.NEWLINE);

                    // Добавляем постер, если есть
                    if (!string.IsNullOrEmpty(selectedFilm.Poster))
                    {
                        try
                        {
                            var converter = new Base64ToImageConverter();
                            var imageSource = (ImageSource)converter.Convert(selectedFilm.Poster, null, null, null);
                            var bitmapSource = imageSource as BitmapSource;

                            if (bitmapSource != null)
                            {
                                string tempImagePath = Path.GetTempFileName() + ".png";
                                using (var fileStream = new FileStream(tempImagePath, FileMode.Create))
                                {
                                    BitmapEncoder encoder = new PngBitmapEncoder();
                                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                                    encoder.Save(fileStream);
                                }

                                iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(tempImagePath);
                                pdfImage.ScaleToFit(200f, 200f);
                                pdfImage.Alignment = Element.ALIGN_CENTER;
                                document.Add(pdfImage);
                                document.Add(Chunk.NEWLINE);

                                File.Delete(tempImagePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении изображения в PDF: {ex.Message}");
                        }
                    }

                    // Добавляем детали о фильме
                    AddDetail(document, "Год выпуска:", selectedFilm.Release_year.ToString(), boldFont, normalFont);
                    AddDetail(document, "Длительность:", $"{selectedFilm.Duration} мин.", boldFont, normalFont);
                    AddDetail(document, "Рейтинг:", $"{selectedFilm.Rating}/10", boldFont, normalFont);
                    AddDetail(document, "Жанры:", selectedFilm.Genres, boldFont, normalFont);
                    AddDetail(document, "Режиссер:", selectedFilm.Directors, boldFont, normalFont);
                    AddDetail(document, "Актеры:", selectedFilm.Actors, boldFont, normalFont);
                    document.Add(Chunk.NEWLINE);

                    // Добавляем описание
                    if (!string.IsNullOrEmpty(selectedFilm.Description))
                    {
                        document.Add(new Paragraph("Описание:", boldFont));
                        document.Add(new Paragraph(selectedFilm.Description, normalFont));
                    }

                    document.Close();

                    MessageBox.Show($"Информация о фильме сохранена в PDF: {saveFileDialog.FileName}",
                                  "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в PDF: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                fullInfoPopup.IsOpen = false;
            }
        }

        private void AddDetail(Document document, string label, string value, Font labelFont, Font valueFont)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Phrase labelPhrase = new Phrase(label + " ", labelFont);
                Phrase valuePhrase = new Phrase(value, valueFont);

                Paragraph paragraph = new Paragraph();
                paragraph.Add(labelPhrase);
                paragraph.Add(valuePhrase);

                document.Add(paragraph);
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
            AppFrame.contentFrame.Navigate(new Pages.NewFilmPage(_user));
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

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                searchTextBox.Text = string.Empty;

                genreFilterComboBox.SelectedIndex = 0;
                yearFilterComboBox.SelectedIndex = 0;
                ratingFilterComboBox.SelectedIndex = 0;

                _sortByNameAscending = true;
                _sortByRatingAscending = true;

                nameSortArrow.RenderTransform = new RotateTransform(0);
                ratingSortArrow.RenderTransform = new RotateTransform(0);

                filmsListView.ItemsSource = _films;

                UpdateFilmsListVisibility();

                searchTextBox.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelPublishFilm_Click(object sender, RoutedEventArgs e)
        {
            PublishFilmPopup.IsOpen = false;
            txtTitle.Text = string.Empty;
            txtMessage.Text = string.Empty;
        }

        private void PublishFilms_Click(object sender, RoutedEventArgs e)
        {
            var film = filmsListView.SelectedItem as Movies;

            if (film != null)
            {
                PublishFilmPopup.IsOpen = true;
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите фильм для публикации",
                              "Предупреждение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void PublishFilm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    errorTxt.Visibility = Visibility.Visible;
                    errorTxt.Text = "Пожалуйста, заполните заголовок";
                    errorTxt.Foreground = Brushes.Red;

                    txtTitle.Focus();
                    return;
                }

                var film = filmsListView.SelectedItem as Movies;

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                       
                        var filmPublishing = new FilmPublishings
                        {
                            MovieID = film.MovieID,
                            Status = "Published",
                            PublishDate = DateTime.Now
                        };

                        _context.FilmPublishings.Add(filmPublishing);
                        _context.SaveChanges();

                        var discussion = new Discussions
                        {
                            PublishID = filmPublishing.PublishID,
                            UserID = _user.UserID,
                            Title = txtTitle.Text,
                            Message = txtMessage.Text,
                        };

                        _context.Discussions.Add(discussion);
                        _context.SaveChanges();

                        transaction.Commit();

                        PublishFilmPopup.IsOpen = false;
                        txtTitle.Text = string.Empty;
                        txtMessage.Text = string.Empty;
                        errorTxt.Visibility = Visibility.Collapsed;

                        MessageBox.Show($"Фильм '{film.Title}' успешно опубликован с обсуждением!",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ShowError($"Ошибка при сохранении данных: {ex.Message}");
                        Debug.WriteLine($"Transaction error: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Неожиданная ошибка: {ex.Message}");
                Debug.WriteLine($"Publish error: {ex}");
            }
        }

        private void ShowError(string message, Control focusControl = null)
        {
            errorTxt.Visibility = Visibility.Visible;
            errorTxt.Text = message;
            errorTxt.Foreground = Brushes.Red;

            if (focusControl != null)
            {
                focusControl.Focus();
            }
        }
    }
}
