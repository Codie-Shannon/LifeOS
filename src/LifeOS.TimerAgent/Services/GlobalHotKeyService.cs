using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace LifeOS.TimerAgent.Services;

[Flags]
public enum HotKeyModifiers : uint
{
    None = 0x0000,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008,
    NoRepeat = 0x4000
}

public sealed class GlobalHotKeyService : IDisposable
{
    private const int WmHotKey = 0x0312;

    private IntPtr _windowHandle;
    private HwndSource? _source;
    private int _hotKeyId;
    private bool _isRegistered;

    public event EventHandler? HotKeyPressed;

    public void Register(Window window, int hotKeyId, Key key, HotKeyModifiers modifiers)
    {
        if (_isRegistered)
        {
            throw new InvalidOperationException("A hotkey is already registered.");
        }

        var helper = new WindowInteropHelper(window);
        _windowHandle = helper.EnsureHandle();

        _source = HwndSource.FromHwnd(_windowHandle);

        if (_source is null)
        {
            throw new InvalidOperationException("Could not create window message source for hotkey.");
        }

        _hotKeyId = hotKeyId;

        var virtualKey = KeyInterop.VirtualKeyFromKey(key);

        var registered = RegisterHotKey(
            _windowHandle,
            _hotKeyId,
            (uint)modifiers,
            (uint)virtualKey);

        if (!registered)
        {
            var errorCode = Marshal.GetLastWin32Error();

            throw new InvalidOperationException(
                $"Could not register global hotkey. Windows error code: {errorCode}");
        }

        _source.AddHook(HandleWindowMessage);
        _isRegistered = true;
    }

    public void Dispose()
    {
        if (!_isRegistered)
        {
            return;
        }

        if (_source is not null)
        {
            _source.RemoveHook(HandleWindowMessage);
        }

        UnregisterHotKey(_windowHandle, _hotKeyId);

        _source = null;
        _windowHandle = IntPtr.Zero;
        _hotKeyId = 0;
        _isRegistered = false;
    }

    private IntPtr HandleWindowMessage(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (message == WmHotKey && wParam.ToInt32() == _hotKeyId)
        {
            HotKeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(
        IntPtr hWnd,
        int id);
}