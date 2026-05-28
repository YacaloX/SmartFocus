using System.IO;
using System.Text.Json;
using Vanara.PInvoke;

namespace SmartFocus.Core
{
    public static class SettingsManager
    {
        private static readonly string FilePath = Path.Combine(AppPaths.AppFolder, "settings.json");
        private static Settings? _settings;  // ✅ Ahora es nullable

        static SettingsManager()
        {
            Load();
        }

        public static void Load()
        {
            if (!File.Exists(FilePath))
            {
                _settings = new Settings();
                Save();
                return;
            }
            var json = File.ReadAllText(FilePath);
            // ✅ Si la deserialización devuelve null, se asigna un nuevo Settings()
            _settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
        }

        public static void Save()
        {
            if (_settings == null)
                Load(); // Seguridad: si por algún motivo es null, recargar

            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public static string GetAccentColor()
        {
            if (_settings == null) Load();
            return _settings!.AccentColor;
        }

        public static void SetAccentColor(string color)
        {
            if (_settings == null) Load();
            _settings!.AccentColor = color;
            Save();
        }

        public static (User32.HotKeyModifiers modifiers, string key) GetHotkey()
        {
            if (_settings == null) Load();
            return (_settings!.HotkeyModifiers, _settings!.HotkeyKey);
        }

        public static void SetHotkey(User32.HotKeyModifiers modifiers, string key)
        {
            if (_settings == null) Load();
            _settings!.HotkeyModifiers = modifiers;
            _settings!.HotkeyKey = key;
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