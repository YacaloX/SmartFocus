using SmartFocus.Core;
using SmartFocus.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
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
            this.PreviewKeyDown += Window_KeyDown;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AliasManager.Load();
            HistoryTracker.Load();

            _hotkeyService = new HotkeyService(this);
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            Hide();
            RefreshWindowList();
        }

        private void RefreshWindowList()
        {
            var windows = _windowManager.GetAllWindows();
            Dispatcher.Invoke(() => _viewModel.UpdateWindows(windows));
        }

        private void OnHotkeyPressed()
        {
            ShowSearchBar();
        }

        public void ShowSearchBar()
        {
            RefreshWindowList();
            Show();
            Activate();
            SearchBox.Focus();
            SearchBox.SelectAll();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide();
            else if (e.Key == Key.Enter)
            {
                var selected = _viewModel.SelectedResult;
                if (selected == null) return;

                var handle = selected.Handle;
                var processName = selected.ProcessName;

                _windowManager.FocusWindow(handle);
                HistoryTracker.RegisterUse(processName);

                await Task.Delay(200);
                Hide();
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            // Hide();  // si quieres que se oculte al perder foco, descomenta
        }
    }
}