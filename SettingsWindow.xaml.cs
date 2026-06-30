/*
File path: SettingsWindow.xaml.cs
What it does: Loads, edits, and saves WallpaperQuick settings.
Why it exists: Keeps settings UI behavior separate from the wallpaper picker.
RELATED FILES: SettingsWindow.xaml, Services/AppSettingsService.cs, Services/StartupService.cs, Models/AppSettings.cs
*/
using System.IO;
using System.Windows;
using System.Windows.Input;
using Forms = System.Windows.Forms;
using FormsKeys = System.Windows.Forms.Keys;
using WallpaperQuick.Models;
using WallpaperQuick.Services;

namespace WallpaperQuick;

public partial class SettingsWindow : Window
{
    private int _hotKeyModifiers;
    private int _hotKeyKey;

    public SettingsWindow()
    {
        InitializeComponent();

        var settings = AppSettingsService.Settings;
        _hotKeyModifiers = settings.HotKeyModifiers;
        _hotKeyKey = settings.HotKeyKey;

        StartWithWindowsCheckBox.IsChecked = settings.StartWithWindows;
        WallpaperFolderTextBox.Text = settings.WallpaperFolder;
        HotKeyTextBox.Text = FormatHotKey();
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Select a wallpaper folder",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(WallpaperFolderTextBox.Text)
                ? WallpaperFolderTextBox.Text
                : AppSettingsService.DefaultWallpaperFolder
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            WallpaperFolderTextBox.Text = dialog.SelectedPath;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var folder = WallpaperFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(folder))
        {
            StatusText.Text = "Choose a wallpaper folder.";
            return;
        }

        Directory.CreateDirectory(folder);

        var settings = new AppSettings
        {
            WallpaperFolder = folder,
            StartWithWindows = StartWithWindowsCheckBox.IsChecked == true,
            HasCompletedFirstRun = true,
            HotKeyModifiers = _hotKeyModifiers,
            HotKeyKey = _hotKeyKey
        };

        AppSettingsService.Save(settings);
        StartupService.SetStartWithWindows(settings.StartWithWindows);
        DialogResult = true;
        Close();
    }

    private void HotKeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (IsModifierOnly(key))
        {
            return;
        }

        var keyCode = KeyInterop.VirtualKeyFromKey(key);
        if (keyCode == 0)
        {
            StatusText.Text = "Choose another shortcut.";
            return;
        }

        var modifiers = (int)Keyboard.Modifiers;
        if (modifiers == 0)
        {
            StatusText.Text = "Use at least one modifier key.";
            return;
        }

        _hotKeyModifiers = modifiers;
        _hotKeyKey = keyCode;
        HotKeyTextBox.Text = FormatHotKey();
        StatusText.Text = string.Empty;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private string FormatHotKey()
    {
        var modifiers = (WallpaperQuick.Services.ModifierKeys)_hotKeyModifiers;
        var parts = new List<string>();

        if (modifiers.HasFlag(WallpaperQuick.Services.ModifierKeys.Control))
        {
            parts.Add("Ctrl");
        }

        if (modifiers.HasFlag(WallpaperQuick.Services.ModifierKeys.Alt))
        {
            parts.Add("Alt");
        }

        if (modifiers.HasFlag(WallpaperQuick.Services.ModifierKeys.Shift))
        {
            parts.Add("Shift");
        }

        if (modifiers.HasFlag(WallpaperQuick.Services.ModifierKeys.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(((FormsKeys)_hotKeyKey).ToString());
        return string.Join(" + ", parts);
    }

    private static bool IsModifierOnly(Key key)
    {
        return key is Key.LeftAlt
            or Key.RightAlt
            or Key.LeftCtrl
            or Key.RightCtrl
            or Key.LeftShift
            or Key.RightShift
            or Key.LWin
            or Key.RWin;
    }
}
