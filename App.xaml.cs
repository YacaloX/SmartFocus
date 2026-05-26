using System.Windows;
using SmartFocus.Core;

namespace SmartFocus
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"Error fatal:\n{ex.Message}\n\nStack:\n{ex.StackTrace}", "Crash", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            // También captura excepciones en el hilo de la UI
            Application.Current.DispatcherUnhandledException += (sender, args) =>
            {
                args.Handled = true;
                MessageBox.Show($"Error UI:\n{args.Exception.Message}\n\nStack:\n{args.Exception.StackTrace}", "Crash", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            AliasManager.Load();
            HistoryTracker.Load();

        }
    }
}