using System.IO;
using System.Text.Json;
using SmartFocus.Core.Interfaces;
using Vanara.PInvoke;

namespace SmartFocus.Core
{
    public class SettingsManager : ISettingsManager
    {
        private readonly string _filePath = Path.Combine(AppPaths.AppFolder, "settings.json");
        private Settings? _settings;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        private readonly ReaderWriterLockSlim _lock = new();

        private readonly System.Timers.Timer _saveTimer = new(2000) { AutoReset = false };
        private bool _dirty;

        public SettingsManager()
        {
            _saveTimer.Elapsed += (_, _) => FlushSave();
            try { Load(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsManager.Load failed: {ex.Message}");
                _settings = new Settings();
            }
        }

        private Settings Current => _settings ?? throw new InvalidOperationException("Settings failed to load");

        public void Load()
        {
            _lock.EnterWriteLock();
            try
            {
                if (!File.Exists(_filePath))
                {
                    _settings = new Settings();
                    Save();
                    return;
                }
                var json = File.ReadAllText(_filePath);
                _settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void FlushSave()
        {
            _lock.EnterReadLock();
            try
            {
                if (!_dirty || _settings == null) return;
                string dir = Path.GetDirectoryName(_filePath)!;
                Directory.CreateDirectory(dir);
                string temp = Path.GetTempFileName();
                var json = JsonSerializer.Serialize(_settings, _jsonOptions);
                File.WriteAllText(temp, json);
                File.Move(temp, _filePath, overwrite: true);
                _dirty = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsManager.FlushSave failed: {ex.Message}");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Save()
        {
            _lock.EnterReadLock();
            _dirty = true;
            _lock.ExitReadLock();
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        public string GetAccentColor()
        {
            _lock.EnterReadLock();
            try
            {
                return Current.AccentColor;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void SetAccentColor(string color)
        {
            _lock.EnterWriteLock();
            try
            {
                Current.AccentColor = color;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            Save();
        }

        public (User32.HotKeyModifiers modifiers, string key) GetHotkey()
        {
            _lock.EnterReadLock();
            try
            {
                return (Current.HotkeyModifiers, Current.HotkeyKey);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void SetHotkey(User32.HotKeyModifiers modifiers, string key)
        {
            _lock.EnterWriteLock();
            try
            {
                Current.HotkeyModifiers = modifiers;
                Current.HotkeyKey = key;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            Save();
        }
    }

    public class Settings
    {
        public string AccentColor { get; set; } = "#00FFFF";
        public User32.HotKeyModifiers HotkeyModifiers { get; set; } =
            User32.HotKeyModifiers.MOD_CONTROL | User32.HotKeyModifiers.MOD_ALT;
        public string HotkeyKey { get; set; } = "F";
    }
}
