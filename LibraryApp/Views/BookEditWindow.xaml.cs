using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace LibraryApp.Views
{
    public partial class BookEditWindow : Window
    {
        private readonly LibraryDbContext _context;
        private Book _book;

        public BookEditWindow(LibraryDbContext context, Book book = null)
        {
            InitializeComponent();
            _context = context;
            _book = book ?? new Book();

            LoadData();

            if (_book.Id != 0)
                FillFields();
        }

        private void LoadData()
        {
            AuthorCombo.ItemsSource = _context.Authors.ToList();
            GenreCombo.ItemsSource = _context.Genres.ToList();
        }

        private void FillFields()
        {
            TitleBox.Text = _book.Title;
            AuthorCombo.SelectedItem = _context.Authors.Find(_book.AuthorId);
            GenreCombo.SelectedItem = _context.Genres.Find(_book.GenreId);
            YearBox.Text = _book.PublishYear.ToString();
            ISBNBox.Text = _book.ISBN;
            QuantityBox.Text = _book.QuantityInStock.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            _book.Title = TitleBox.Text.Trim();
            _book.AuthorId = ((Author)AuthorCombo.SelectedItem).Id;
            _book.GenreId = ((Genre)GenreCombo.SelectedItem).Id;
            _book.PublishYear = int.Parse(YearBox.Text.Trim());
            _book.ISBN = ISBNBox.Text.Trim();
            _book.QuantityInStock = int.Parse(QuantityBox.Text.Trim());

            if (_book.Id == 0)
                _context.Books.Add(_book);
            else
                _context.Entry(_book).State = EntityState.Modified;

            _context.SaveChanges();
            DialogResult = true;
        }

        private bool ValidateInputs()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название книги", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TitleBox.Focus();
                return false;
            }

            // Проверка автора
            if (AuthorCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите автора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                AuthorCombo.Focus();
                return false;
            }

            // Проверка жанра
            if (GenreCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите жанр", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                GenreCombo.Focus();
                return false;
            }

            // Проверка года
            if (!int.TryParse(YearBox.Text.Trim(), out int year) || year < 1000 || year > 2100)
            {
                MessageBox.Show("Неверный год издания (должен быть от 1000 до 2100)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                YearBox.Focus();
                return false;
            }

            // Проверка ISBN
            var isbn = ISBNBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(isbn))
            {
                MessageBox.Show("Введите ISBN", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ISBNBox.Focus();
                return false;
            }

            if (!isbn.Contains("-"))
            {
                MessageBox.Show("ISBN должен содержать тире (пример: 978-5-699-12014-7)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ISBNBox.Focus();
                return false;
            }

            // Проверка длины ISBN без тире
            var isbnWithoutDashes = isbn.Replace("-", "").Replace(" ", "");
            if (isbnWithoutDashes.Length != 13)
            {
                MessageBox.Show("ISBN должен содержать 13 цифр (без учёта тире)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ISBNBox.Focus();
                return false;
            }

            // Проверка количества
            if (!int.TryParse(QuantityBox.Text.Trim(), out int qty) || qty < 0)
            {
                MessageBox.Show("Неверное количество (должно быть число >= 0)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityBox.Focus();
                return false;
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}