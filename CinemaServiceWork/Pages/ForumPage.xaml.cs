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
    /// Interaction logic for ForumPage.xaml
    /// </summary>
    public partial class ForumPage : Page
    {
        private Users _user;
        private CinemaEntities _context = new CinemaEntities();
        private Discussions _currentDiscussion;
        private ObservableCollection<Movies> _films = new ObservableCollection<Movies>();

        public ForumPage(Users user)
        {
            InitializeComponent();
            _user = user;
            LoadPublishFilms();
         }

        private void LoadPublishFilms()
        {
            try
            {
                _films.Clear();

                var publishFilms = AppConnect.cinemaEntities.FilmPublishings
       .OrderByDescending(fp => fp.PublishDate)
       .Include(fp => fp.Discussions.Select(d => d.Comments))
       .Include(fp => fp.Movies)
       .Select(fp => new
       {
           Movie = fp.Movies,
           PublishDate = fp.PublishDate,
           Discussions = fp.Discussions.Select(d => new
           {
               Discussion = d,
               CommentCount = d.Comments.Count,
               CurrentUserComCount = d.Comments.Count(c => c.UserID == _user.UserID)
           })
       })
       .ToList();

                foreach (var item in publishFilms)
                {
                    var discussions = item.Discussions
                        .Select(d => new Discussions
                        {
                            Title = d.Discussion.Title,
                            Message = d.Discussion.Message,
                            DiscussionID = d.Discussion.DiscussionID,
                            PublishID = d.Discussion.PublishID,
                            CommentCount = d.CommentCount,
                            CurrentUserComCount = d.CurrentUserComCount,
                            Comments = d.Discussion.Comments.Select(c => new Comments
                            {
                                CommentID = c.CommentID,
                                Contenting = c.Contenting,
                                UserID = c.UserID
                               
                            }).ToList()
                        }).ToList();

                    var filmPublishing = new FilmPublishings
                    {
                        PublishDate = item.PublishDate,
                        MovieID = item.Movie.MovieID,
                        Discussions = new ObservableCollection<Discussions>(discussions)
                    };

                    var film = new Movies
                    {
                        MovieID = item.Movie.MovieID,
                        Title = item.Movie.Title,
                        Poster = item.Movie.Poster,
                        FilmPublishings = new ObservableCollection<FilmPublishings> { filmPublishing }
                    };

                    _films.Add(film);
                }

                filmsListView.ItemsSource = _films;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке опубликованных фильмов: {ex.Message}");
            }
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new NewFilmPage(_user));
        }

        private void CreateDiscussion_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.contentFrame.Navigate(new NewFilmPage(_user));
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filmsListView.SelectedItem is Movies selectedMovie)
            {
                var listViewItem = filmsListView
                    .ItemContainerGenerator
                    .ContainerFromItem(selectedMovie) as ListViewItem;

                if (listViewItem != null)
                {
                    var discussionBtn = FindVisualChild<Button>(listViewItem, "discussionBtn");
                    discussionBtn?.SetValue(Button.IsEnabledProperty, true);
                }
            }

            foreach (var item in e.RemovedItems)
            {
                if (item is Movies previousMovie)
                {
                    var previousItem = filmsListView
                        .ItemContainerGenerator
                        .ContainerFromItem(previousMovie) as ListViewItem;

                    var previousBtn = FindVisualChild<Button>(previousItem, "discussionBtn");
                    previousBtn?.SetValue(Button.IsEnabledProperty, false);
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName)
            where T : FrameworkElement
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (child != null)
                {
                    if (child is T result && child.Name == childName)
                        return result;

                    var foundChild = FindVisualChild<T>(child, childName);
                    if (foundChild != null)
                        return foundChild;
                }
            }
            return null;
        }

        private void DiscussionItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Discussions discussion)
            {
                OpenDiscussionPopup(discussion);
            }
        }

        private void OpenDiscussionPopup(Discussions discussion)
        {

            _currentDiscussion = _context.Discussions
                .Include(d => d.Comments)
                .Include(d => d.Comments.Select(c => c.Users))
                .FirstOrDefault(d => d.DiscussionID == discussion.DiscussionID);

            if (_currentDiscussion != null)
            {
      
                _currentDiscussion.Comments = new ObservableCollection<Comments>(_currentDiscussion.Comments);

    
                discussionPopup.DataContext = _currentDiscussion;
                discussionPopup.IsOpen = true;
                ScrollToBottom();
            }
        }
        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(сommentTextBox.Text))
                {
                    MessageBox.Show("Введите текст комментария");
                    return;
                }

                if (_currentDiscussion == null)
                {
                    MessageBox.Show("Обсуждение не выбрано");
                    return;
                }

                var newComment = new Comments
                {
                    Contenting = сommentTextBox.Text,
                    UserID = _user.UserID,
                    DiscussionID = _currentDiscussion.DiscussionID,
                };

                _context.Comments.Add(newComment);
                _context.SaveChanges();

                сommentTextBox.Text = string.Empty;

                discussionPopup.DataContext = _currentDiscussion;
     

                //    _currentDiscussion.CommentCount = _currentDiscussion.CommentCount;

                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении комментария: {ex.Message}");
                Debug.WriteLine(ex.ToString());
            }
        }

        private void ScrollToBottom()
        {
            // Ждем обновления UI
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }


        private void CloseDiscussionPopup_Click(object sender, RoutedEventArgs e)
        {
            discussionPopup.IsOpen = false;
            LoadPublishFilms();
        }

    }
}
