using System.Windows;

namespace LibraryApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Глобальный перехват исключений
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show(
                    $"Произошла ошибка:\n\n{args.Exception.Message}\n\n" +
                    $"Подробности:\n{args.Exception.InnerException?.Message}",
                    "Критическая ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                args.Handled = true;
            };

            base.OnStartup(e);
        }
    }
}