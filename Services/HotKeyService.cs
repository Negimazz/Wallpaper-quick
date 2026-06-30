/*
File path: Services/HotKeyService.cs
What it does: Registers and updates the Windows global hotkey.
Why it exists: Lets WallpaperQuick appear instantly with the user's chosen shortcut.
RELATED FILES: App.xaml.cs, SettingsWindow.xaml.cs, MainWindow.xaml.cs, WallpaperQuick.csproj
*/
using System.Runtime.InteropServices;
using System.Windows.Interop;
using FormsKeys = System.Windows.Forms.Keys;

namespace WallpaperQuick.Services;

[Flags]
public enum ModifierKeys
{
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008
}

public sealed class HotKeyService : IDisposable
{
    private const int HotKeyId = 9173;
    private const int WmHotKey = 0x0312;

    private readonly HwndSource _source;

    public event Action? Pressed;

    public HotKeyService(System.Windows.Window window, ModifierKeys modifiers, FormsKeys key)
    {
        var helper = new WindowInteropHelper(window);
        window.SourceInitialized += (_, _) => Register(helper.Handle, modifiers, key);

        window.Show();
        window.Hide();

        _source = HwndSource.FromHwnd(helper.Handle);
        _source.AddHook(WndProc);
    }

    public void Update(ModifierKeys modifiers, FormsKeys key)
    {
        UnregisterHotKey(_source.Handle, HotKeyId);
        Register(_source.Handle, modifiers, key);
    }

    public void Dispose()
    {
        UnregisterHotKey(_source.Handle, HotKeyId);
        _source.RemoveHook(WndProc);
    }

    private void Register(IntPtr handle, ModifierKeys modifiers, FormsKeys key)
    {
        RegisterHotKey(handle, HotKeyId, (uint)modifiers, (uint)key);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotKey && wParam.ToInt32() == HotKeyId)
        {
            Pressed?.Invoke();
            handled = true;
        }

        return IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
