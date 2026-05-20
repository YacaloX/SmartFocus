using System.Windows;
using System.Windows.Input;

namespace SmartFocus
{
    public partial class MainWindow : Window
    {
        private HotkeyService? _hotkeyService;

        public MainWindow()
        {
            InitializeComponent();
            // No crees el HotkeyService aquí
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Ahora la ventana tiene handle, podemos registrar la hotkey
            _hotkeyService = new HotkeyService(this);
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        }

        private void OnHotkeyPressed()
        {
            ShowSearchBar();
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                // Al cargar, ocultar inmediatamente (solo se muestra con la hotkey)
                Hide();
            };
        }

        // Método público para mostrar la barra desde el ViewModel
        public void ShowSearchBar()
        {
            Show();
            Activate();
            SearchBox.Focus();
            SearchBox.SelectAll();
        }

        // Cerrar con Escape
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide();
        }

        // Para que al perder el foco se oculte automáticamente (opcional)
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            // Pequeño retardo para evitar que se oculte al hacer clic en la lista
            // Puedes usar un DispatcherTimer o simplemente Hide() aquí si prefieres.
            // Hide();
        }
    }
}