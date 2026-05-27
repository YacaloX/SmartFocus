using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace SmartFocus.Core;

public class HotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private readonly HwndSource _source;
    private readonly int _hotkeyId = 9001;
    public event Action? HotkeyPressed;

    public HotkeyService(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        _source = HwndSource.FromHwnd(handle)!;
        _source.AddHook(WndProc);

        bool registered = User32.RegisterHotKey(handle, _hotkeyId,
            User32.HotKeyModifiers.MOD_CONTROL | User32.HotKeyModifiers.MOD_SHIFT, 0x7B);

        if (!registered)
        {
            Debug.WriteLine("ERROR: No se pudo registrar la hotkey Ctrl+Shift+F12. La aplicación funcionará sin hotkey.");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
        {
            HotkeyPressed?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        _source.RemoveHook(WndProc);
        User32.UnregisterHotKey(_source.Handle, _hotkeyId);
    }
}