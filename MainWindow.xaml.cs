/*
File path: MainWindow.xaml.cs
What it does: Loads wallpapers, filters the picker list, and applies the selected image.
Why it exists: Keeps the UI responsive and makes the hotkey picker behave like a launcher.
RELATED FILES: MainWindow.xaml, SettingsWindow.xaml.cs, Models/WallpaperItem.cs, Services/WallpaperService.cs
*/
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Input;
using WallpaperQuick.Models;
using WallpaperQuick.Services;

namespace WallpaperQuick;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<WallpaperItem> _visibleWallpapers = new();
    private List<WallpaperItem> _allWallpapers = new();
    private bool _isApplyingWallpaper;
    private bool _isAnimatingHide;

    public MainWindow()
    {
        InitializeComponent();
        WallpaperList.ItemsSource = _visibleWallpapers;
    }

    public void RefreshWallpapers()
    {
        _allWallpapers = WallpaperService.GetWallpapers();
        ApplyFilter();

        SearchBox.ToolTip = _allWallpapers.Count == 0
            ? $"Put images in: {WallpaperService.WallpaperFolder}"
            : null;
    }

    public void FocusSearchBox()
    {
        SearchBox.Focus();
        SearchBox.SelectAll();
    }

    public void PrepareToShow()
    {
        _isApplyingWallpaper = false;
        _isAnimatingHide = false;
        PickerRoot.BeginAnimation(UIElement.OpacityProperty, null);
        PickerRoot.Opacity = 1;
    }

    public void ReleaseWallpapers()
    {
        _allWallpapers.Clear();
        _visibleWallpapers.Clear();
    }

    private void ApplyFilter()
    {
        var query = SearchBox.Text.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _allWallpapers
            : _allWallpapers
                .Where(item => item.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _visibleWallpapers.Clear();
        foreach (var item in matches)
        {
            _visibleWallpapers.Add(item);
        }

        WallpaperList.SelectedIndex = _visibleWallpapers.Count > 0 ? 0 : -1;
        Placeholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplySelectedWallpaper()
    {
        if (WallpaperList.SelectedItem is not WallpaperItem item)
        {
            return;
        }

        try
        {
            _isApplyingWallpaper = true;
            if (WallpaperService.SetWallpaper(item.Path))
            {
                AnimateAndHidePicker();
                return;
            }

            _isApplyingWallpaper = false;
            SearchBox.ToolTip = "Could not change wallpaper.";
        }
        catch (Exception ex)
        {
            _isApplyingWallpaper = false;
            SearchBox.ToolTip = $"Could not load image: {ex.Message}";
        }
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void WallpaperList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
    }

    private void WallpaperList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ApplySelectedWallpaper();
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            HidePicker();
            return;
        }

        if (e.Key == Key.Enter)
        {
            ApplySelectedWallpaper();
            return;
        }

        if (e.Key == Key.Down && WallpaperList.SelectedIndex < _visibleWallpapers.Count - 1)
        {
            WallpaperList.SelectedIndex++;
            WallpaperList.ScrollIntoView(WallpaperList.SelectedItem);
            e.Handled = true;
        }

        if (e.Key == Key.Up && WallpaperList.SelectedIndex > 0)
        {
            WallpaperList.SelectedIndex--;
            WallpaperList.ScrollIntoView(WallpaperList.SelectedItem);
            e.Handled = true;
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (_isApplyingWallpaper || _isAnimatingHide)
        {
            return;
        }

        HidePicker();
    }

    private void HidePicker()
    {
        PickerRoot.BeginAnimation(UIElement.OpacityProperty, null);
        _isApplyingWallpaper = false;
        _isAnimatingHide = false;
        Hide();
        ReleaseWallpapers();
        PickerRoot.Opacity = 1;
    }

    private void AnimateAndHidePicker()
    {
        _isAnimatingHide = true;
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(260),
            FillBehavior = FillBehavior.Stop
        };

        fade.Completed += (_, _) => HidePicker();
        PickerRoot.BeginAnimation(UIElement.OpacityProperty, fade);
    }
}
