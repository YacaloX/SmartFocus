using SmartFocus.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vanara.PInvoke;

namespace SmartFocus
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var (modifiers, key) = SettingsManager.GetHotkey();

            CtrlCheck.IsChecked = modifiers.HasFlag(User32.HotKeyModifiers.MOD_CONTROL);
            AltCheck.IsChecked = modifiers.HasFlag(User32.HotKeyModifiers.MOD_ALT);
            ShiftCheck.IsChecked = modifiers.HasFlag(User32.HotKeyModifiers.MOD_SHIFT);
            WinCheck.IsChecked = modifiers.HasFlag(User32.HotKeyModifiers.MOD_WIN);

            foreach (ComboBoxItem item in KeyCombo.Items)
                if (item.Tag?.ToString() == key) { KeyCombo.SelectedItem = item; break; }

            string savedColor = SettingsManager.GetAccentColor();
            bool found = false;
            foreach (ComboBoxItem item in ColorCombo.Items)
            {
                if (item.Tag?.ToString() == savedColor)
                {
                    ColorCombo.SelectedItem = item;
                    found = true;
                    break;
                }
            }
            if (!found && !string.IsNullOrEmpty(savedColor))
            {
                var customItem = new ComboBoxItem { Content = $"🎨 {savedColor}", Tag = "CUSTOM" };
                ColorCombo.Items.Add(customItem);
                ColorCombo.SelectedItem = customItem;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Línea 88 corregida: uso de ?. en Tag y ToString()
            string newColor = ((ComboBoxItem)ColorCombo.SelectedItem)?.Tag?.ToString() ?? "#00FFFF";
            SettingsManager.SetAccentColor(newColor);
            ApplyColorChange(newColor);

            User32.HotKeyModifiers modifiers = 0;
            if (CtrlCheck.IsChecked == true) modifiers |= User32.HotKeyModifiers.MOD_CONTROL;
            if (AltCheck.IsChecked == true) modifiers |= User32.HotKeyModifiers.MOD_ALT;
            if (ShiftCheck.IsChecked == true) modifiers |= User32.HotKeyModifiers.MOD_SHIFT;
            if (WinCheck.IsChecked == true) modifiers |= User32.HotKeyModifiers.MOD_WIN;

            string key = ((ComboBoxItem)KeyCombo.SelectedItem)?.Tag?.ToString() ?? "Y";

            bool success = await ((App)Application.Current).ReRegisterHotkey(modifiers, key);
            if (success)
            {
                SettingsManager.SetHotkey(modifiers, key);
                StatusText.Text = "Configuración guardada correctamente.";
                StatusText.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                StatusText.Text = "Error: la hotkey ya está en uso o no es válida.";
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void ApplyColorChange(string hexColor)
        {
            var color = (Color)ColorConverter.ConvertFromString(hexColor);
            var brush = new SolidColorBrush(color);
            Application.Current.Resources["NeonCyan"] = brush;
        }

        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorCombo.SelectedItem is ComboBoxItem item)
            {
                string? tag = item.Tag?.ToString();
                if (tag == "CUSTOM")
                {
                    using (var dialog = new System.Windows.Forms.ColorDialog())
                    {
                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string hex = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                            ApplyAccentColor(hex);
                            item.Content = $"🎨 {hex}";
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(tag))
                {
                    ApplyAccentColor(tag);
                }
            }
        }

        private void ApplyAccentColor(string hex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                var brush = new SolidColorBrush(color);
                Application.Current.Resources["NeonCyan"] = brush;
                SettingsManager.SetAccentColor(hex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al aplicar color: {ex.Message}");
            }
        }
    }
}