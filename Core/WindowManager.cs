using Vanara.PInvoke;
using System.Diagnostics;

namespace SmartFocus.Core;

public record WindowInfo(IntPtr Handle, string Title, uint ProcessId, string? ProcessName);

public class WindowManager
{
    public void FocusWindow(IntPtr hwnd)
    {
        // 1. Restaurar si está minimizada
        if (User32.IsIconic(hwnd))
            User32.ShowWindow(hwnd, ShowWindowCommand.SW_RESTORE);

        // 2. Traer al frente
        User32.SetForegroundWindow(hwnd);
        User32.BringWindowToTop(hwnd);
        User32.ShowWindow(hwnd, ShowWindowCommand.SW_SHOW);
        User32.SetFocus(hwnd);

        // 3. Robo de foco forzado (truco de AttachThreadInput)
        var foreThread = User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out _);
        var targetThread = User32.GetWindowThreadProcessId(hwnd, out _);
        if (foreThread != targetThread)
        {
            User32.AttachThreadInput(foreThread, targetThread, true);
            User32.SetForegroundWindow(hwnd);
            User32.BringWindowToTop(hwnd);
            User32.AttachThreadInput(foreThread, targetThread, false);
        }

        // 4. Asegurar que no quede minimizada por algún bug
        if (User32.IsIconic(hwnd))
            User32.ShowWindow(hwnd, ShowWindowCommand.SW_RESTORE);

    }
}