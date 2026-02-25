using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace LibraryApp
{
    public partial class MainWindow : Window
    {
        private readonly LibraryDbContext _context;
        private ObservableCollection<Book> _books;

        public MainWindow()
        {
            InitializeComponent();
            _context = new LibraryDbContext();
            LoadBooks();
            LoadFilters();
        }

        private void LoadBooks()
        {
            _books = new ObservableCollection<Book>(
                _context.Books.Include(b => b.Author).Include(b => b.Genre).ToList());
            BooksGrid.ItemsSource = _books;
        }

        private void LoadFilters()
        {
            AuthorFilter.ItemsSource = _context.Authors.ToList();
            GenreFilter.ItemsSource = _context.Genres.ToList();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var query = _context.Books.Include(b => b.Author).Include(b => b.Genre).AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                query = query.Where(b => b.Title.Contains(SearchBox.Text));

            _books = new ObservableCollection<Book>(query.ToList());
            BooksGrid.ItemsSource = _books;
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var query = _context.Books.Include(b => b.Author).Include(b => b.Genre).AsQueryable();

            if (AuthorFilter.SelectedItem is Author selectedAuthor)
                query = query.Where(b => b.AuthorId == selectedAuthor.Id);
            if (GenreFilter.SelectedItem is Genre selectedGenre)
                query = query.Where(b => b.GenreId == selectedGenre.Id);

            _books = new ObservableCollection<Book>(query.ToList());
            BooksGrid.ItemsSource = _books;
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            AuthorFilter.SelectedItem = null;
            GenreFilter.SelectedItem = null;
            LoadBooks();
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            var window = new BookEditWindow(_context);
            window.Owner = this;
            if (window.ShowDialog() == true)
                LoadBooks();
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem is Book selected)
            {
                var window = new BookEditWindow(_context, selected);
                window.Owner = this;
                if (window.ShowDialog() == true)
                    LoadBooks();
            }
            else
                MessageBox.Show("Выберите книгу для редактирования");
        }

        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem is Book selected)
            {
                var result = MessageBox.Show($"Удалить книгу \"{selected.Title}\"?",
                    "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Books.Remove(selected);
                    _context.SaveChanges();
                    LoadBooks();
                }
            }
            else
                MessageBox.Show("Выберите книгу для удаления");
        }

        private void ManageAuthors_Click(object sender, RoutedEventArgs e)
        {
            var window = new AuthorWindow(_context);
            window.Owner = this;
            window.ShowDialog();
            LoadFilters();
        }

        private void ManageGenres_Click(object sender, RoutedEventArgs e)
        {
            var window = new GenreWindow(_context);
            window.Owner = this;
            window.ShowDialog();
            LoadFilters();
        }
    }
}