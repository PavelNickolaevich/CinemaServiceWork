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
    /// Interaction logic for NewActorPage.xaml
    /// </summary>
    public partial class NewActorPage : Page
    {
        private CinemaEntities _context = new CinemaEntities();
        private Users _user;
        public NewActorPage(Users user)
        {
            InitializeComponent();
            this.DataContext = AppConnect.cinemaEntities;
            listCountries.ItemsSource = AppConnect.cinemaEntities.Countries.ToList();
            _user = user;
        }

        private void btnAddActor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(txtName.Text) ||
                    string.IsNullOrWhiteSpace(txtSurname.Text) ||
                    dateBirthPicker.SelectedDate == null ||
                    listCountries.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, заполните имя, фамилию, дату рождения и выберите страну.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var selectedCountry = listCountries.SelectedItem as Countries;
                if (selectedCountry == null)
                {
                    MessageBox.Show("Выберите корректную страну.", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка на существующего актера
                string firstName = txtName.Text.Trim();
                string lastName = txtSurname.Text.Trim();
                string patronymic = string.IsNullOrWhiteSpace(txtPatronymic.Text) ? null : txtPatronymic.Text.Trim();
                DateTime birthDate = dateBirthPicker.SelectedDate.Value;

                bool actorExists = _context.Actors.Any(a =>
                    a.First_name.Equals(firstName, StringComparison.CurrentCultureIgnoreCase) &&
                    a.Last_name.Equals(lastName, StringComparison.CurrentCultureIgnoreCase) &&
                    (a.Patronymic == null && patronymic == null ||
                     a.Patronymic != null && a.Patronymic.Equals(patronymic, StringComparison.CurrentCultureIgnoreCase)) &&
                    a.Birth_of_date == birthDate &&
                    a.CountryID == selectedCountry.CountryID);

                if (actorExists)
                {
                    MessageBox.Show("Актер с такими данными уже существует в базе.",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                var newActor = new Actors
                {
                    First_name = firstName,
                    Last_name = lastName,
                    Patronymic = patronymic,
                    Birth_of_date = birthDate,
                    CountryID = selectedCountry.CountryID
                };

                _context.Actors.Add(newActor);
                _context.SaveChanges();
                ClearFields();

                MessageBox.Show($"{newActor.First_name} {newActor.Last_name} успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClearActor_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {

            txtName.Text = string.Empty;
            txtPatronymic.Text = string.Empty;
            txtSurname.Text = string.Empty;
            dateBirthPicker.Text = string.Empty;
            listCountries.Text = string.Empty;

        }

    }
}
