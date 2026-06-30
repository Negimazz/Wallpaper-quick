<!--
File path: README.md
What it does: Explains how to publish and use WallpaperQuick.
Why it exists: Gives the project a simple handoff note for local Windows use.
RELATED FILES: WallpaperQuick.csproj, App.xaml.cs, Services/AppSettingsService.cs
-->
# WallpaperQuick

WallpaperQuick is a lightweight Windows 11 wallpaper picker.

## Usage

1. Start `WallpaperQuick.exe`.
2. On first launch, choose whether to start with Windows.
3. Choose the folder that contains your wallpapers.
4. Choose the shortcut that opens the picker.
5. Type to filter, then press `Enter` or double-click an image.

Settings can also be opened from the tray icon menu.

## Publish

```powershell
dotnet publish -c Release
```

The exe is created under:

```text
bin\Release\net7.0-windows\win-x64\publish\WallpaperQuick.exe
```

## Supported images

- `.jpg`
- `.jpeg`
- `.png`
- `.bmp`
- `.webp`
