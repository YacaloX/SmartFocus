using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SmartFocus.Core.Interfaces;
using SmartFocus.Models;
using Vanara.PInvoke;

namespace SmartFocus.Core
{
    public class WindowManager : IWindowManager
    {
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const int SW_SHOWMINIMIZED = 2;
        private const uint ASFW_ANY = uint.MaxValue;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(uint dwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

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
                    catch (ArgumentException) { }

                    windows.Add(new WindowInfo(hwnd.DangerousGetHandle(), title, pid, processName));
                }
                return true;
            }, IntPtr.Zero);
            return windows;
        }

        public async Task FocusWindowAsync(IntPtr hwndPtr)
        {
            try
            {
                if (!IsWindow(hwndPtr))
                    return;

                if (IsIconic(hwndPtr))
                    ShowWindow(hwndPtr, SW_RESTORE);
                else
                    ShowWindow(hwndPtr, SW_SHOW);

                await Task.Delay(50);

                uint currentThreadId = GetCurrentThreadId();

                AllowSetForegroundWindow(ASFW_ANY);
                bool focused = SetForegroundWindow(hwndPtr);

                if (!focused || GetForegroundWindow() != hwndPtr)
                {
                    IntPtr foreground = GetForegroundWindow();
                    if (foreground != IntPtr.Zero)
                    {
                        uint foreThread = GetWindowThreadProcessId(foreground, IntPtr.Zero);
                        if (foreThread != 0 && foreThread != currentThreadId)
                        {
                            AttachThreadInput(foreThread, currentThreadId, true);
                            SetForegroundWindow(hwndPtr);
                            BringWindowToTop(hwndPtr);
                            await Task.Delay(20);
                            AttachThreadInput(foreThread, currentThreadId, false);
                        }
                    }
                }

                if (GetForegroundWindow() != hwndPtr)
                {
                    ShowWindow(hwndPtr, SW_SHOWMINIMIZED);
                    await Task.Delay(30);
                    ShowWindow(hwndPtr, SW_RESTORE);
                    AllowSetForegroundWindow(ASFW_ANY);
                    SetForegroundWindow(hwndPtr);
                }

                if (GetForegroundWindow() != hwndPtr)
                {
                    Debug.WriteLine($"FocusWindow: All 3 levels failed for handle {hwndPtr}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FocusWindow error: {ex.Message}");
            }
        }
    }
}
