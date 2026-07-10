using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

internal static class WindowsProtectedData
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob { public int Length; public IntPtr Data; }

    [DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptProtectData(ref DataBlob input, string description, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

    [DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptUnprotectData(ref DataBlob input, IntPtr description, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

    [DllImport("Kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);

    public static byte[] Protect(byte[] value)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Google token protection requires Windows.");
        return Transform(value, protect: true);
    }

    public static byte[] Unprotect(byte[] value)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Google token protection requires Windows.");
        return Transform(value, protect: false);
    }

    private static byte[] Transform(byte[] value, bool protect)
    {
        var inputPointer = Marshal.AllocHGlobal(value.Length);
        try
        {
            Marshal.Copy(value, 0, inputPointer, value.Length);
            var input = new DataBlob { Length = value.Length, Data = inputPointer };
            DataBlob output;
            var success = protect
                ? CryptProtectData(ref input, "LifeOS Google Calendar token", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, out output)
                : CryptUnprotectData(ref input, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, out output);
            if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());
            try
            {
                var bytes = new byte[output.Length];
                Marshal.Copy(output.Data, bytes, 0, output.Length);
                return bytes;
            }
            finally { LocalFree(output.Data); }
        }
        finally { Marshal.FreeHGlobal(inputPointer); }
    }
}
