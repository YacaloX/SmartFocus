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
            try
            {
                if (!User32.IsWindow((HWND)hwnd))
                    return;

                if (User32.IsIconic((HWND)hwnd))
                    ShowWindow(hwnd, SW_RESTORE);
                else
                    ShowWindow(hwnd, SW_SHOW);

                Thread.Sleep(50);

                GetWindowThreadProcessId(hwnd, out uint destPid);
                AllowSetForegroundWindow(destPid);

                IntPtr foreground = GetForegroundWindow();
                uint foreThread = GetWindowThreadProcessId(foreground, out _);
                uint targetThread = GetWindowThreadProcessId(hwnd, out _);
                bool attached = false;
                if (foreThread != targetThread)
                {
                    AttachThreadInput(foreThread, targetThread, true);
                    attached = true;
                }

                SetForegroundWindow(hwnd);
                BringWindowToTop(hwnd);
                SwitchToThisWindow(hwnd, true);

                if (attached)
                    AttachThreadInput(foreThread, targetThread, false);

                Thread.Sleep(100);
                if (GetForegroundWindow() != hwnd)
                {
                    keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                    keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                    Thread.Sleep(30);
                    keybd_event(VK_TAB, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
                    keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
                    Thread.Sleep(100);
                    SetForegroundWindow(hwnd);
                }

                if (GetForegroundWindow() != hwnd)
                {
                    if (GetWindowRect(hwnd, out RECT rect))
                    {
                        int x = (rect.Left + rect.Right) / 2;
                        int y = rect.Top + 15;
                        SetCursorPos(x, y);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                        Thread.Sleep(10);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                    }
                }
            }
            catch (Exception ex)
            {
                // El error se ignora silenciosamente en producción
                Debug.WriteLine($"Error en FocusWindow: {ex.Message}");
            }
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