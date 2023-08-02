#pragma warning disable CA1416 // 验证平台兼容性
using Walterlv.ForegroundWindowMonitor;

using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

using static Windows.Win32.PInvoke;

// 输出表头。
var consoleWidth = Console.WindowWidth;
var table = new ConsoleTableBuilder<Win32Window>(consoleWidth, new ConsoleTableColumnDefinition<Win32Window>[]
{
    (8, "time", _ => $"{DateTime.Now:hh:mm:ss}"),
    (8, "hwnd", w => $"{w.Handle:X8}"),
    (0.5, "title", w => w.Title),
    (0.25, "class name", w => w.ClassName),
    (6, "pid", w => $"{w.ProcessId}"),
    (0.25, "process name", w => $"{w.ProcessName}"),
});
Console.WriteLine(table.BuildHeaderRows());

// 监听系统的前台窗口变化。
SetWinEventHook(
    EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
    HMODULE.Null, WinEventProc,
    0, 0,
    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

// 开启消息循环，以便 WinEventProc 能够被调用。
if (GetMessage(out var lpMsg, default, default, default))
{
    TranslateMessage(in lpMsg);
    DispatchMessage(in lpMsg);
}

// 当前前台窗口变化时，输出新的前台窗口信息。
void WinEventProc(HWINEVENTHOOK hWinEventHook, uint @event, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
{
    var current = GetForegroundWindow();
    var newConsoleWidth = Console.WindowWidth;
    if (newConsoleWidth != consoleWidth)
    {
        consoleWidth = newConsoleWidth;
        table = table.NewTableWidth(newConsoleWidth);
        Console.WriteLine(table.BuildHeaderRows());
    }

    var w = new Win32Window(current);
    var rowText = table.BuildRow(w, StringDisplayMode.Wrap);

    Console.WriteLine(rowText);
}
