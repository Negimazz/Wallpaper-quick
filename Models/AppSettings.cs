/*
File path: Models/AppSettings.cs
What it does: Stores user-configurable WallpaperQuick settings.
Why it exists: Keeps startup and wallpaper folder preferences in one simple model.
RELATED FILES: Services/AppSettingsService.cs, SettingsWindow.xaml.cs, Services/StartupService.cs
*/
namespace WallpaperQuick.Models;

public sealed class AppSettings
{
    public string WallpaperFolder { get; set; } = string.Empty;
    public bool StartWithWindows { get; set; }
    public bool HasCompletedFirstRun { get; set; }
    public int HotKeyModifiers { get; set; }
    public int HotKeyKey { get; set; }
}
