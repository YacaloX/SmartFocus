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
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const int WM_ACTIVATE = 0x0006;
        private const int WA_ACTIVE = 1;

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

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        static string GetWindowTextRaw(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const byte VK_MENU = 0x12;
        private const byte VK_TAB = 0x09;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }
    
    public void FocusWindow(IntPtr hwnd)
    {
    // Asegurar que trabajamos con la ventana raíz
        hwnd = User32.GetAncestor((HWND)hwnd, User32.GetAncestorFlag.GA_ROOT).DangerousGetHandle();
        var target = (HWND)hwnd;

    // Validar que la ventana aún existe
        if (!User32.IsWindow(target))
            return;

    // Restaurar si está minimizada
        User32.ShowWindow(target, ShowWindowCommand.SW_RESTORE);

    // Si nuestra app ya no está en primer plano (porque nos ocultamos),
    // aplicamos la técnica de robo de foco.
        var foreground = User32.GetForegroundWindow();
        if (foreground != target)  // Si el destino no es ya el foco
        {
            uint foreThread = User32.GetWindowThreadProcessId(foreground, out _);
            uint currentThread = Kernel32.GetCurrentThreadId();

            if (foreThread != currentThread)
            {
                User32.AttachThreadInput(currentThread, foreThread, true);
                User32.SetForegroundWindow(target);
                User32.AttachThreadInput(currentThread, foreThread, false);
            }
            else
            {
            // Ya somos el primer plano, llamada directa
                User32.SetForegroundWindow(target);
            }
        }

    // Asegurar el foco del teclado en la ventana
        User32.SetFocus(target);
    }

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
