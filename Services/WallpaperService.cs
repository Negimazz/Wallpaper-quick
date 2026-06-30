/*
File path: Services/WallpaperService.cs
What it does: Finds wallpaper images, creates thumbnails, opens the wallpaper folder, and applies wallpapers.
Why it exists: Keeps Windows file and wallpaper API details out of the UI code.
RELATED FILES: MainWindow.xaml.cs, Models/WallpaperItem.cs, Services/AppSettingsService.cs, App.xaml.cs
*/
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using WallpaperQuick.Models;

namespace WallpaperQuick.Services;

public static class WallpaperService
{
    private const int SpiSetDeskWallpaper = 20;
    private const int SpifUpdateIniFile = 0x01;
    private const int SpifSendWinIniChange = 0x02;

    private static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".webp" };

    public static string WallpaperFolder => AppSettingsService.Settings.WallpaperFolder;

    private static string CacheFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WallpaperQuick");

    public static List<WallpaperItem> GetWallpapers()
    {
        Directory.CreateDirectory(WallpaperFolder);

        return Directory
            .EnumerateFiles(WallpaperFolder)
            .Where(IsSupportedImage)
            .OrderBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase)
            .Select(CreateWallpaperItem)
            .ToList();
    }

    public static bool SetWallpaper(string imagePath)
    {
        var wallpaperPath = PrepareWallpaperFile(imagePath);
        return SystemParametersInfo(
            SpiSetDeskWallpaper,
            0,
            wallpaperPath,
            SpifUpdateIniFile | SpifSendWinIniChange);
    }

    public static void OpenWallpaperFolder()
    {
        Directory.CreateDirectory(WallpaperFolder);
        Process.Start(new ProcessStartInfo
        {
            FileName = WallpaperFolder,
            UseShellExecute = true
        });
    }

    private static bool IsSupportedImage(string path)
    {
        var extension = Path.GetExtension(path);
        return SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private static WallpaperItem CreateWallpaperItem(string path)
    {
        return new WallpaperItem
        {
            Name = Path.GetFileNameWithoutExtension(path),
            Path = path,
            Thumbnail = LoadThumbnail(path)
        };
    }

    private static BitmapImage LoadThumbnail(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.DecodePixelWidth = 120;
        image.UriSource = new Uri(path);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private static string PrepareWallpaperFile(string imagePath)
    {
        Directory.CreateDirectory(CacheFolder);

        var outputPath = Path.Combine(CacheFolder, "current-wallpaper.jpg");
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(imagePath);
        image.EndInit();

        using var output = File.Create(outputPath);
        var encoder = new JpegBitmapEncoder { QualityLevel = 95 };
        encoder.Frames.Add(BitmapFrame.Create(image));
        encoder.Save(output);

        return outputPath;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(int action, int param, string value, int update);
}
