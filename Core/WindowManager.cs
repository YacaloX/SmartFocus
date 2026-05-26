using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SmartFocus.Models;
using Vanara.PInvoke;

namespace SmartFocus.Core
{
    public class WindowManager
    {
        // Constantes de Windows
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const int WM_ACTIVATE = 0x0006;
        private const int WA_ACTIVE = 1;

        // Importaciones avanzadas
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(uint dwProcessId);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const byte VK_MENU = 0x12;   // Alt
        private const byte VK_TAB = 0x09;

        // Método principal mejorado
        public void FocusWindow(IntPtr hwnd)
        {
            if (!User32.IsWindow((HWND)hwnd))
                return;

            // Obtener título y PID para depuración
            var titleBuilder = new StringBuilder(256);
            User32.GetWindowText((HWND)hwnd, titleBuilder, titleBuilder.Capacity);
            string title = titleBuilder.ToString();
            Debug.WriteLine($"Enfocando: {title} (Handle: {hwnd})");

            // Forzar restauración si minimizada
            if (User32.IsIconic((HWND)hwnd))
                ShowWindow(hwnd, SW_RESTORE);
            else
                ShowWindow(hwnd, SW_SHOW);

            // 1. Técnica de AttachThreadInput (la más fiable)
            uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), out _);
            uint targetThread = GetWindowThreadProcessId(hwnd, out _);
            if (foreThread != targetThread)
            {
                AttachThreadInput(foreThread, targetThread, true);
                SetForegroundWindow(hwnd);
                BringWindowToTop(hwnd);
                AttachThreadInput(foreThread, targetThread, false);
            }
            else
            {
                SetForegroundWindow(hwnd);
                BringWindowToTop(hwnd);
            }

            // 2. Forzar con SwitchToThisWindow (como respaldo)
            SwitchToThisWindow(hwnd, true);

            // 3. Enviar mensaje de activación directa
            SendMessage(hwnd, WM_ACTIVATE, WA_ACTIVE, 0);

            // 4. Simular Alt+Tab si la ventana sigue sin ganar foco
            Thread.Sleep(50);
            IntPtr currentFore = GetForegroundWindow();
            if (currentFore != hwnd)
            {
                Debug.WriteLine("Método normal falló, simulando Alt+Tab...");
                // Simular presionar Alt (sin soltar)
                keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                // Simular presionar Tab
                keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                // Soltar Tab
                keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
                // Soltar Alt
                keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);

                // Después de Alt+Tab, la ventana debería estar al frente, pero puede ser otra.
                // Volvemos a intentar enfocar directamente
                Thread.Sleep(50);
                SetForegroundWindow(hwnd);
                BringWindowToTop(hwnd);
            }
        }

        // Método para obtener todas las ventanas visibles (sin cambios)
        public List<WindowInfo> GetAllWindows()
        {
            var windows = new List<WindowInfo>();
            User32.EnumWindows((hwnd, lParam) =>
            {
                if (User32.IsWindowVisible(hwnd) && User32.GetWindowTextLength(hwnd) > 0)
                {
                    User32.GetWindowThreadProcessId(hwnd, out uint pid);
                    var titleBuilder = new StringBuilder(256);
                    User32.GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);
                    string title = titleBuilder.ToString();

                    string? processName = null;
                    try
                    {
                        using (var proc = Process.GetProcessById((int)pid))
                            processName = proc.ProcessName;
                    }
                    catch { }

                    IntPtr handlePtr = hwnd.DangerousGetHandle();
                    windows.Add(new WindowInfo(handlePtr, title, pid, processName));
                }
                return true;
            }, IntPtr.Zero);
            return windows;
        }
    }
}