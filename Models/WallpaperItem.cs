/*
File path: Models/WallpaperItem.cs
What it does: Represents one wallpaper file shown in the picker.
Why it exists: Keeps file path, display name, and thumbnail data together.
RELATED FILES: MainWindow.xaml, MainWindow.xaml.cs, Services/WallpaperService.cs
*/
using System.Windows.Media.Imaging;

namespace WallpaperQuick.Models;

public sealed class WallpaperItem
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required BitmapImage Thumbnail { get; init; }
}
