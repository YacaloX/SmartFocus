using SmartFocus.Core;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace SmartFocus
{
    public partial class App : Application
    {
        private HotkeyService? _hotkeyService;
        private MainWindow? _mainWindow;

        // Referencia permanente al icono de la bandeja
        private Hardcodet.Wpf.TaskbarNotification.TaskbarIcon? _trayIcon;

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(uint dwProcessId);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Crear ventana principal (oculta al inicio)
            _mainWindow = new MainWindow();
            _mainWindow.Hide();

            // Crear servicio de hotkey (se enganchará al SourceInitialized de la ventana)
            _hotkeyService = new HotkeyService(_mainWindow);
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            // Registrar hotkey DESPUÉS de que la ventana tenga handle (evento Loaded)
            _mainWindow.WindowReady += () =>
            {
                var (modifiers, key) = AppServices.Settings.GetHotkey();
                bool success = _hotkeyService.RegisterHotkey(modifiers, key);

                if (!success)
                {
                    // Intento con combinación alternativa por defecto (Win + Y)
                    success = _hotkeyService.RegisterHotkey(User32.HotKeyModifiers.MOD_WIN, "Y");
                    if (success)
                    {
                        AppServices.Settings.SetHotkey(User32.HotKeyModifiers.MOD_WIN, "Y");
                    }
                    else
                    {
                        // Último intento: Ctrl + Alt + F
                        success = _hotkeyService.RegisterHotkey(
                            User32.HotKeyModifiers.MOD_CONTROL | User32.HotKeyModifiers.MOD_ALT,
                            "F");
                        if (success)
                        {
                            AppServices.Settings.SetHotkey(User32.HotKeyModifiers.MOD_CONTROL | User32.HotKeyModifiers.MOD_ALT, "F");
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
            _mainWindow?.ShowSearchBar();
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

        protected override void OnExit(ExitEventArgs e)
        {
            if (_hotkeyService != null)
            {
                _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
                _hotkeyService.Dispose();
            }
            _trayIcon?.Dispose();
            base.OnExit(e);
        }

        private void CreateTrayIcon()
        {
            try
            {
                _trayIcon = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon();

                // Cargar icono desde recurso incrustado
                try
                {
                    using var stream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("SmartFocus.UI.appIcon.ico");
                    if (stream != null)
                        _trayIcon.Icon = new System.Drawing.Icon(stream);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading tray icon: {ex.Message}");
                    var fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(fileName))
                        _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
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