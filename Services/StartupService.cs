/*
File path: Services/StartupService.cs
What it does: Registers or removes WallpaperQuick from Windows user startup.
Why it exists: Lets the user choose whether the app starts automatically with Windows.
RELATED FILES: SettingsWindow.xaml.cs, Services/AppSettingsService.cs, App.xaml.cs
*/
using Microsoft.Win32;

namespace WallpaperQuick.Services;

public static class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "WallpaperQuick";

    public static void SetStartWithWindows(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key is null)
        {
            return;
        }

        if (!enabled)
        {
            key.DeleteValue(AppName, throwOnMissingValue: false);
            return;
        }

        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return;
        }

        key.SetValue(AppName, $"\"{executablePath}\"");
    }
}
