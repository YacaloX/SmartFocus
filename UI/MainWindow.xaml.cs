using System.Windows;
using System.Windows.Input;
using SmartFocus.Core;
using SmartFocus.Models;   // ← Asegúrate de tener este using
using System.Windows.Threading;

namespace SmartFocus
{
    public partial class MainWindow : Window
    {
        private HotkeyService? _hotkeyService;
        private WindowManager _windowManager = new();
        private MainViewModel _viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AliasManager.Load();
            HistoryTracker.Load();

            _hotkeyService = new HotkeyService(this);
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            Hide();

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s, _) => RefreshWindowList();
            timer.Start();
            RefreshWindowList();
        }

        private void RefreshWindowList()
        {
            var windows = _windowManager.GetAllWindows();
            Dispatcher.Invoke(() => _viewModel.UpdateWindows(windows));
        }

        private void OnHotkeyPressed() => ShowSearchBar();

        public void ShowSearchBar()
        {
            RefreshWindowList();  // Datos frescos
            Show();
            Activate();
            SearchBox.Focus();
            SearchBox.SelectAll();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide();
            else if (e.Key == Key.Enter && _viewModel.SelectedResult != null)
            {
                var handle = _viewModel.SelectedResult.Handle;
                var processName = _viewModel.SelectedResult.ProcessName;

                // Primero dar foco, luego registrar y ocultar con retraso
                _windowManager.FocusWindow(handle);
                HistoryTracker.RegisterUse(processName);

                // Pequeño retraso para que el foco se establezca antes de ocultar SmartFocus
                Dispatcher.BeginInvoke(new Action(() => Hide()), DispatcherPriority.Background);
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            // Hide();  // si quieres que se oculte al perder foco, descomenta
        }
    }
}