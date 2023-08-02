using System.Text;

using Walterlv.ForegroundWindowMonitor;

using Windows.Win32.Foundation;

using static Windows.Win32.PInvoke;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var consoleWidth = Console.WindowWidth;
var table = new ConsoleTableBuilder<Win32Window>(consoleWidth, new ConsoleTableColumnDefinition<Win32Window>[]
{
    (8, "time", _ => $"{DateTime.Now:hh:mm:ss}"),
    (8, "hwnd", w => $"{w.Handle:X8}"),
    (6, "pid", w => $"{w.ProcessId}"),
    (0.5, "title", w => w.Title),
    (0.25, "class name", w => w.ClassName),
    (0.25, "process name", w => $"{w.ProcessName}"),
});
Console.WriteLine(table.BuildHeaderRows());

HWND last = default;
while (true)
{
    var current = GetForegroundWindow();
    if (current != last)
    {
        var newConsoleWidth = Console.WindowWidth;
        if (newConsoleWidth != consoleWidth)
        {
            consoleWidth = newConsoleWidth;
            table = table.NewTableWidth(newConsoleWidth);
            Console.WriteLine(table.BuildHeaderRows());
        }

        last = current;
        var w = new Win32Window(current);
        var rowText = table.BuildRow(w);
        Console.WriteLine(rowText);
    }
    await Task.Delay(200).ConfigureAwait(false);
}
