using SmartFocus.Core;
using SmartFocus.Core.Interfaces;
using SmartFocus.Models;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SmartFocus
{
    public partial class MainWindow : Window
    {
        private readonly IWindowManager _windowManager = AppServices.WindowManager;
        private readonly MainViewModel _viewModel = new();
        public event Action? WindowReady;

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(uint dwProcessId);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOW = 5;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowReady?.Invoke();
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            Visibility = Visibility.Hidden;
            Loaded += (_, _) => RefreshWindowList();
        }

        public void RefreshWindowList()
        {
            var windows = _windowManager.GetAllWindows();
            _viewModel.UpdateWindows(windows);
        }

        public void FocusSearchBox()
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
        }

        public void ShowSearchBar()
        {
            RefreshWindowList();
            Show();

            var handle = new WindowInteropHelper(this).Handle;
            AllowSetForegroundWindow(uint.MaxValue);
            SetForegroundWindow(handle);
            ShowWindow(handle, SW_SHOW);
            Activate();
            Topmost = true;
            Topmost = false;

            FocusSearchBox();
        }

        private async void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    Hide();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    var selected = _viewModel.SelectedResult ?? _viewModel.Results.FirstOrDefault();
                    if (selected == null) return;

                    var handle = selected.Handle;
                    var processName = selected.ProcessName;

                    await _windowManager.FocusWindowAsync(handle);

                    AppServices.History.RegisterUse(processName);

                    await Task.Delay(100);

                    Hide();

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PreviewKeyDown error: {ex.Message}");
            }
        }
    }
}