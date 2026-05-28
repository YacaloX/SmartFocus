using SmartFocus.Core;
using SmartFocus.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmartFocus
{
    public partial class MainWindow : Window
    {
        private WindowManager _windowManager = new();
        private MainViewModel _viewModel = new();
        public event Action? WindowReady;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowReady?.Invoke();
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            Visibility = Visibility.Hidden;  // ← en lugar de Hide()
            Loaded += (_, _) => RefreshWindowList();
        }

        public void RefreshWindowList()
        {
            var windows = _windowManager.GetAllWindows();
            Dispatcher.Invoke(() => _viewModel.UpdateWindows(windows));
        }

        public void FocusSearchBox()
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
        }

        // Método público que puede llamar el icono de bandeja
        public void ShowSearchBar()
        {
            RefreshWindowList();
            Show();
            Activate();
            FocusSearchBox();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
                return;
            }

            if (e.Key == Key.Enter)
            {
            var selected = _viewModel.SelectedResult;
            if (selected == null) return;

            var handle = selected.Handle;
            var processName = selected.ProcessName;

        // 1. Ocultar inmediatamente
            Hide();

        // 2. Registrar uso (en background)
            HistoryTracker.RegisterUse(processName);

        // 3. Forzar foco después de que la UI haya procesado el Hide
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _windowManager.FocusWindow(handle);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }

        // protected override void OnDeactivated(EventArgs e)
        // {
        //     base.OnDeactivated(e);
        //     Hide();
        // }
    }
}
