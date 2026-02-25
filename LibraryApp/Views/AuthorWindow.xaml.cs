using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LibraryApp.Views
{
    public partial class AuthorWindow : Window
    {
        private readonly LibraryDbContext _context;
        private ObservableCollection<Author> _authors;
        private Author _selectedAuthor;

        public AuthorWindow(LibraryDbContext context)
        {
            InitializeComponent();
            _context = context;
            LoadAuthors();
        }

        private void LoadAuthors()
        {
            try
            {
                _authors = new ObservableCollection<Author>(
                    _context.Authors.OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ToList());
                AuthorsGrid.ItemsSource = _authors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки авторов: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAuthor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

                var birthDate = BirthDatePicker.SelectedDate ?? DateTime.Now;

                // Конвертируем в Unspecified для совместимости с timestamp without time zone
                if (birthDate.Kind == DateTimeKind.Utc)
                {
                    birthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Unspecified);
                }

                var author = new Author
                {
                    FirstName = FirstNameBox.Text.Trim(),
                    LastName = LastNameBox.Text.Trim(),
                    BirthDate = birthDate,
                    Country = CountryBox.Text.Trim()
                };

                _context.Authors.Add(author);
                _context.SaveChanges();

                LoadAuthors();
                ClearFields();
                MessageBox.Show("Автор успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при добавлении автора:\n\n{ex.Message}\n\n" +
                    $"Внутренняя ошибка:\n{ex.InnerException?.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAuthor == null)
            {
                MessageBox.Show("Выберите автора из таблицы для обновления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInputs())
                return;

            var birthDate = BirthDatePicker.SelectedDate ?? _selectedAuthor.BirthDate;

            // Конвертируем в Unspecified
            if (birthDate.Kind == DateTimeKind.Utc)
            {
                birthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Unspecified);
            }

            _selectedAuthor.FirstName = FirstNameBox.Text.Trim();
            _selectedAuthor.LastName = LastNameBox.Text.Trim();
            _selectedAuthor.BirthDate = birthDate;
            _selectedAuthor.Country = CountryBox.Text.Trim();

            _context.Entry(_selectedAuthor).State = EntityState.Modified;
            _context.SaveChanges();

            LoadAuthors();
            ClearFields();
            MessageBox.Show("Автор успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAuthor == null)
            {
                MessageBox.Show("Выберите автора для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var booksCount = _context.Books.Count(b => b.AuthorId == _selectedAuthor.Id);
            if (booksCount > 0)
            {
                var result = MessageBox.Show(
                    $"У этого автора есть {booksCount} книг(и). Все они будут удалены.\nПродолжить?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }
            else
            {
                var result = MessageBox.Show(
                    $"Удалить автора \"{_selectedAuthor.LastName} {_selectedAuthor.FirstName}\"?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            _context.Authors.Remove(_selectedAuthor);
            _context.SaveChanges();

            LoadAuthors();
            ClearFields();
            MessageBox.Show("Автор успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AuthorsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AuthorsGrid.SelectedItem is Author author)
            {
                _selectedAuthor = author;
                FirstNameBox.Text = author.FirstName;
                LastNameBox.Text = author.LastName;
                BirthDatePicker.SelectedDate = author.BirthDate;
                CountryBox.Text = author.Country;
            }
        }

        private void ClearFields_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            FirstNameBox.Text = "";
            LastNameBox.Text = "";
            BirthDatePicker.SelectedDate = null;
            CountryBox.Text = "";
            _selectedAuthor = null;
            AuthorsGrid.SelectedItem = null;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                MessageBox.Show("Введите имя автора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                FirstNameBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show("Введите фамилию автора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                LastNameBox.Focus();
                return false;
            }

            if (FirstNameBox.Text.Length > 100 || LastNameBox.Text.Length > 100)
            {
                MessageBox.Show("Имя и фамилия не должны превышать 100 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}