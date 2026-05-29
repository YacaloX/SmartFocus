using Vanara.PInvoke;

namespace SmartFocus.Core.Interfaces;

public interface ISettingsManager
{
    string GetAccentColor();
    void SetAccentColor(string color);
    (User32.HotKeyModifiers modifiers, string key) GetHotkey();
    void SetHotkey(User32.HotKeyModifiers modifiers, string key);
    void Save();
}
