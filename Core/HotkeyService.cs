using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace SmartFocus.Core
{
    public class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        private HwndSource? _source;
        private readonly int _hotkeyId = 9001;
        private IntPtr _handle;

        public event Action? HotkeyPressed;

        public HotkeyService(Window window)
        {
            window.SourceInitialized += (_, _) =>
            {
                _handle = new WindowInteropHelper(window).Handle;

                Debug.WriteLine($"✅ HANDLE CREADO: {_handle}");

                _source = HwndSource.FromHwnd(_handle);

                if (_source != null)
                {
                    _source.AddHook(WndProc);
                    Debug.WriteLine("✅ WndProc hook agregado");
                }
            };
        }

        public bool RegisterHotkey(User32.HotKeyModifiers modifiers, string key)
        {
            if (_handle == IntPtr.Zero)
            {
                Debug.WriteLine("❌ HANDLE INVALIDO");
                return false;
            }

            User32.UnregisterHotKey(_handle, _hotkeyId);

            uint virtualKey = GetVirtualKey(key);

            Debug.WriteLine($"KEY = {key}");
            Debug.WriteLine($"VK = {virtualKey}");

            if (virtualKey == 0)
            {
                Debug.WriteLine("❌ VirtualKey inválida");
                return false;
            }

            bool result = User32.RegisterHotKey(
                _handle,
                _hotkeyId,
                modifiers,
                virtualKey);

            int error = Marshal.GetLastWin32Error();

            Debug.WriteLine($"HOTKEY RESULT = {result}");
            Debug.WriteLine($"LAST ERROR = {error}");

            if (result)
                Debug.WriteLine("✅ HOTKEY REGISTRADO");
            else
                Debug.WriteLine("❌ NO SE PUDO REGISTRAR");

            return result;
        }

        private uint GetVirtualKey(string key)
        {
            key = key.ToUpper();

            // Letras y números
            if (key.Length == 1 && char.IsLetterOrDigit(key[0]))
                return (uint)key[0];

            // F1-F12
            if (key.StartsWith("F"))
            {
                if (int.TryParse(key.Substring(1), out int fKey))
                {
                    if (fKey >= 1 && fKey <= 12)
                        return (uint)(0x70 + (fKey - 1));
                }
            }

            return 0;
        }

        private IntPtr WndProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                Debug.WriteLine("🔥 WM_HOTKEY RECIBIDO");

                if (wParam.ToInt32() == _hotkeyId)
                {
                    Debug.WriteLine("✅ HOTKEY ACTIVADO");

                    HotkeyPressed?.Invoke();

                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_source != null)
                _source.RemoveHook(WndProc);

            if (_handle != IntPtr.Zero)
                User32.UnregisterHotKey(_handle, _hotkeyId);
        }
    }
}