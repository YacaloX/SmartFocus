using SmartFocus.Core;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace SmartFocus
{
    public partial class App : Application
    {
        private HotkeyService? _hotkeyService;
        private MainWindow? _mainWindow;

        // Referencia permanente al icono de la bandeja
        private Hardcodet.Wpf.TaskbarNotification.TaskbarIcon? _trayIcon;

        // Win32 imports
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(uint dwProcessId);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        static string GetWindowTextRaw(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cargar configuraciones
            AliasManager.Load();
            HistoryTracker.Load();
            SettingsManager.Load();

            // Crear ventana principal (oculta al inicio)
            _mainWindow = new MainWindow();
            _mainWindow.Hide();

            // Crear servicio de hotkey (se enganchará al SourceInitialized de la ventana)
            _hotkeyService = new HotkeyService(_mainWindow);
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            // Registrar hotkey DESPUÉS de que la ventana tenga handle (evento Loaded)
            _mainWindow.WindowReady += () =>
            {
                var (modifiers, key) = SettingsManager.GetHotkey();
                bool success = _hotkeyService.RegisterHotkey(modifiers, key);

                if (!success)
                {
                    // Intento con combinación alternativa por defecto (Win + Y)
                    success = _hotkeyService.RegisterHotkey(User32.HotKeyModifiers.MOD_WIN, "Y");
                    if (success)
                    {
                        SettingsManager.SetHotkey(User32.HotKeyModifiers.MOD_WIN, "Y");
                    }
                    else
                    {
                        // Último intento: Ctrl + Alt + F
                        success = _hotkeyService.RegisterHotkey(
                            User32.HotKeyModifiers.MOD_CONTROL | User32.HotKeyModifiers.MOD_ALT,
                            "F");
                        if (success)
                        {
                            SettingsManager.SetHotkey(User32.HotKeyModifiers.MOD_CONTROL | User32.HotKeyModifiers.MOD_ALT, "F");
                        }
                        else
                        {
                            // Si nada funciona, mostrar advertencia
                            MessageBox.Show(
                                "No se pudo registrar ningún hotkey global. La aplicación solo podrá abrirse desde el icono de la bandeja.\n\n" +
                                "Verifica que ninguna otra aplicación esté usando las combinaciones: Win+Y, Ctrl+Alt+F.",
                                "SmartFocus - Advertencia",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                }
            };

            // Manejo de excepciones no controladas
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Error: {args.Exception.Message}", "SmartFocus", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            // Crear icono de bandeja (con referencia permanente)
            CreateTrayIcon();

            // MOSTRAR LA VENTANA UNA VEZ AL INICIAR (para que el usuario sepa que la app está activa)
            _mainWindow.ShowSearchBar();
        }

        private void OnHotkeyPressed()
        {
            if (_mainWindow == null) return;
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.ShowSearchBar();
            });
        }

        /// <summary>
        /// Fuerza a una ventana WPF a mostrarse y obtener el foco, incluso desde un hotkey global.
        /// </summary>
        public static void ForceWindowToFront(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;

            if (!window.IsVisible)
                window.Show();

            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;

            window.Activate();

            ShowWindow(handle, SW_SHOW);

            SetForegroundWindow(handle);

            window.Topmost = true;
            window.Topmost = false;

            window.Focus();
        }

        public async Task<bool> ReRegisterHotkey(User32.HotKeyModifiers modifiers, string key)
        {
            if (_hotkeyService == null) return false;
            return await Task.Run(() => _hotkeyService.RegisterHotkey(modifiers, key));
        }

        private void CreateTrayIcon()
        {
            try
            {
                _trayIcon = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon();

                // Cargar icono: si no existe el archivo, usar uno por defecto del sistema
                try
                {
                    _trayIcon.Icon = new System.Drawing.Icon("icon.ico");
                }
                catch
                {
                    // Fallback: icono por defecto de la aplicación (o generado)
                    _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "");
                }

                _trayIcon.ToolTipText = "SmartFocus";

                var menu = new System.Windows.Controls.ContextMenu();

                var showItem = new System.Windows.Controls.MenuItem { Header = "Mostrar" };
                showItem.Click += (s, e) => _mainWindow?.ShowSearchBar();

                var settingsItem = new System.Windows.Controls.MenuItem { Header = "Configuración" };
                settingsItem.Click += (s, e) => new SettingsWindow().ShowDialog();

                var exitItem = new System.Windows.Controls.MenuItem { Header = "Salir" };
                exitItem.Click += (s, e) => Current.Shutdown();

                menu.Items.Add(showItem);
                menu.Items.Add(settingsItem);
                menu.Items.Add(new System.Windows.Controls.Separator());
                menu.Items.Add(exitItem);

                _trayIcon.ContextMenu = menu;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando ícono de bandeja: {ex.Message}");
            }
        }
    }
}