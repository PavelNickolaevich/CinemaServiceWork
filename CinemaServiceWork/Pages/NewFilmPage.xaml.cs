
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CinemaServiceWork.Pages
{
    /// <summary>
    /// Interaction logic for NewFilmPage.xaml
    /// </summary>
    public partial class NewFilmPage : Page
    {
        public NewFilmPage()
        {
            InitializeComponent();
            this.DataContext = AppConnect.cinemaEntities;
            genreBox.ItemsSource = AppConnect.cinemaEntities.Genres.ToList();
        }



        private void genreBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedGenres = AppConnect.cinemaEntities.Genres.Where(d => d.IsSelected).Select(d => d.Name).ToList();
            MessageBox.Show(string.Join(", ", selectedGenres));
        }
    }
}
