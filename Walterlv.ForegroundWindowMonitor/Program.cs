using Walterlv.ForegroundWindowMonitor;

using Windows.Win32.Foundation;

using static Windows.Win32.PInvoke;

HWND last = default;
while (true)
{
    var current = GetForegroundWindow();
    if (current != last)
    {
        last = current;
        var w = new Win32Window(current);
        //var (hWnd, className, title) = GetWindowInfo(current);
        //GetWindowThreadProcessId(hWnd, out var pid);
        Console.WriteLine($"[{DateTime.Now:mm:ss}] [pid={w.ProcessId}] [{w.Handle:X8}] {w.Title} - {w.ClassName}");
    }
    await Task.Delay(200).ConfigureAwait(false);
}
