/*
File path: Services/AppSettingsService.cs
What it does: Loads and saves WallpaperQuick settings as a local JSON file.
Why it exists: Lets the app remember folder, startup, and hotkey preferences.
RELATED FILES: Models/AppSettings.cs, SettingsWindow.xaml.cs, Services/WallpaperService.cs, App.xaml.cs
*/
using System.IO;
using System.Text.Json;
using FormsKeys = System.Windows.Forms.Keys;
using WallpaperQuick.Models;

namespace WallpaperQuick.Services;

public static class AppSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string SettingsFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WallpaperQuick");

    public static string SettingsPath { get; } = Path.Combine(SettingsFolder, "settings.json");

    public static AppSettings Settings { get; private set; } = Load();

    public static string DefaultWallpaperFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "WallpaperQuick");

    public static int DefaultHotKeyModifiers => (int)(ModifierKeys.Control | ModifierKeys.Alt);
    public static int DefaultHotKeyKey => (int)FormsKeys.W;

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsFolder);
        Settings = settings;

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    private static AppSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return CreateDefaultSettings();
        }

        try
        {
            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();

            if (string.IsNullOrWhiteSpace(settings.WallpaperFolder))
            {
                settings.WallpaperFolder = DefaultWallpaperFolder;
            }

            if (settings.HotKeyModifiers == 0 || settings.HotKeyKey == 0)
            {
                settings.HotKeyModifiers = DefaultHotKeyModifiers;
                settings.HotKeyKey = DefaultHotKeyKey;
            }

            return settings;
        }
        catch
        {
            return CreateDefaultSettings();
        }
    }

    private static AppSettings CreateDefaultSettings()
    {
        return new AppSettings
        {
            WallpaperFolder = DefaultWallpaperFolder,
            HotKeyModifiers = DefaultHotKeyModifiers,
            HotKeyKey = DefaultHotKeyKey
        };
    }
}
