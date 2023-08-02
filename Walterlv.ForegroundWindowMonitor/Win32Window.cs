using System.Buffers;

using Windows.Win32.Foundation;

using static Windows.Win32.PInvoke;

namespace Walterlv.ForegroundWindowMonitor;
public class Win32Window
{
    private readonly HWND _hWnd;
    private string? _className;
    private string? _title;
    private uint _pid;

    internal Win32Window(nint handle)
    {
        _hWnd = (HWND)handle;
    }

    public nint Handle => _hWnd;

    public string ClassName => _className ??= CallWin32ToGetPWSTR(512, (p, l) => GetClassName(_hWnd, p, l));

    public string Title => _title ??= CallWin32ToGetPWSTR(512, (p, l) => GetWindowText(_hWnd, p, l));

    public uint ProcessId => _pid is 0 ? (_pid = GetProcessIdCore()) : _pid;

    private unsafe uint GetProcessIdCore()
    {
        uint pid = 0;
        GetWindowThreadProcessId(_hWnd, &pid);
        return pid;
    }

    private unsafe string CallWin32ToGetPWSTR(int bufferLength, Func<PWSTR, int, int> getter)
    {
        var buffer = ArrayPool<char>.Shared.Rent(bufferLength);
        try
        {
            fixed (char* ptr = buffer)
            {
                getter(ptr, bufferLength);
                return new string(buffer, 0, Array.IndexOf(buffer, '\0'));
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
