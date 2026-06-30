/*
File path: App.xaml.cs
What it does: Starts WallpaperQuick, owns the tray icon, wires the custom hotkey, and opens settings.
Why it exists: Keeps the app running quietly while still giving access to setup options.
RELATED FILES: App.xaml, MainWindow.xaml.cs, SettingsWindow.xaml.cs, Services/AppSettingsService.cs
*/
using System.Windows;
using System.Windows.Forms;
using WallpaperQuick.Services;
using FormsKeys = System.Windows.Forms.Keys;

namespace WallpaperQuick;

public partial class App : System.Windows.Application
{
    private MainWindow? _window;
    private NotifyIcon? _trayIcon;
    private HotKeyService? _hotKeyService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _window = new MainWindow();
        var settings = AppSettingsService.Settings;
        _hotKeyService = new HotKeyService(
            _window,
            (ModifierKeys)settings.HotKeyModifiers,
            (FormsKeys)settings.HotKeyKey);
        _hotKeyService.Pressed += ShowPicker;

        CreateTrayIcon();

        if (!AppSettingsService.Settings.HasCompletedFirstRun)
        {
            ShowSettings();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotKeyService?.Dispose();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }

    private void CreateTrayIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open picker", null, (_, _) => ShowPicker());
        menu.Items.Add("Open wallpaper folder", null, (_, _) => WallpaperService.OpenWallpaperFolder());
        menu.Items.Add("Settings", null, (_, _) => ShowSettings());
        menu.Items.Add("Exit", null, (_, _) => Shutdown());

        _trayIcon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "WallpaperQuick",
            Visible = true,
            ContextMenuStrip = menu
        };
        _trayIcon.DoubleClick += (_, _) => ShowPicker();
    }

    private void ShowPicker()
    {
        if (_window is null)
        {
            return;
        }

        _window.PrepareToShow();
        _window.RefreshWallpapers();
        _window.Show();
        _window.Activate();
        _window.FocusSearchBox();
    }

    private void ShowSettings()
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = _window
        };

        settingsWindow.ShowDialog();
        ApplyHotKeySettings();

        if (_window?.IsVisible == true)
        {
            _window.RefreshWallpapers();
        }
        else
        {
            _window?.ReleaseWallpapers();
        }
    }

    private void ApplyHotKeySettings()
    {
        var settings = AppSettingsService.Settings;
        _hotKeyService?.Update(
            (ModifierKeys)settings.HotKeyModifiers,
            (FormsKeys)settings.HotKeyKey);
    }
}
