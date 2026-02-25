using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LibraryApp.Views
{
    public partial class GenreWindow : Window
    {
        private readonly LibraryDbContext _context;
        private ObservableCollection<Genre> _genres;
        private Genre _selectedGenre;

        public GenreWindow(LibraryDbContext context)
        {
            InitializeComponent();
            _context = context;
            LoadGenres();
        }

        private void LoadGenres()
        {
            _genres = new ObservableCollection<Genre>(
                _context.Genres.OrderBy(g => g.Name).ToList());
            GenresGrid.ItemsSource = _genres;
        }

        private void AddGenre_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            var genre = new Genre
            {
                Name = NameBox.Text.Trim(),
                Description = DescriptionBox.Text.Trim()
            };

            _context.Genres.Add(genre);
            _context.SaveChanges();

            LoadGenres();
            ClearFields();
            MessageBox.Show("Жанр успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateGenre_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGenre == null)
            {
                MessageBox.Show("Выберите жанр из таблицы для обновления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInputs())
                return;

            _selectedGenre.Name = NameBox.Text.Trim();
            _selectedGenre.Description = DescriptionBox.Text.Trim();

            _context.Entry(_selectedGenre).State = EntityState.Modified;
            _context.SaveChanges();

            LoadGenres();
            ClearFields();
            MessageBox.Show("Жанр успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteGenre_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGenre == null)
            {
                MessageBox.Show("Выберите жанр для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, есть ли книги этого жанра
            var booksCount = _context.Books.Count(b => b.GenreId == _selectedGenre.Id);
            if (booksCount > 0)
            {
                var result = MessageBox.Show(
                    $"В этом жанре есть {booksCount} книг(и). Все они будут удалены.\nПродолжить?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }
            else
            {
                var result = MessageBox.Show(
                    $"Удалить жанр \"{_selectedGenre.Name}\"?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            _context.Genres.Remove(_selectedGenre);
            _context.SaveChanges();

            LoadGenres();
            ClearFields();
            MessageBox.Show("Жанр успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenresGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GenresGrid.SelectedItem is Genre genre)
            {
                _selectedGenre = genre;
                NameBox.Text = genre.Name;
                DescriptionBox.Text = genre.Description;
            }
        }

        private void ClearFields_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            NameBox.Text = "";
            DescriptionBox.Text = "";
            _selectedGenre = null;
            GenresGrid.SelectedItem = null;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название жанра", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                NameBox.Focus();
                return false;
            }

            if (NameBox.Text.Length > 100)
            {
                MessageBox.Show("Название жанра не должно превышать 100 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (DescriptionBox.Text.Length > 500)
            {
                MessageBox.Show("Описание не должно превышать 500 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}