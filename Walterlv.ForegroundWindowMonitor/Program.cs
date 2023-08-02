using System.Text;

using Walterlv.ForegroundWindowMonitor;

using Windows.Win32.Foundation;

using static Windows.Win32.PInvoke;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var table = new TableBuilder(Console.WindowWidth, new TableColumnDefinition[]
{
    (8, "time"),
    (8, "hwnd"),
    (6, "pid"),
    (0.5, "title"),
    (0.5, "description"),
});
Console.WriteLine(table.BuildHeaderRows());

HWND last = default;
while (true)
{
    var current = GetForegroundWindow();
    if (current != last)
    {
        last = current;
        var w = new Win32Window(current);
        var rowText = table.BuildRow(w, new List<Func<Win32Window, string>>()
        {
            w => $"{DateTime.Now:hh:mm:ss}",
            w => $"{w.Handle:X8}",
            w => $"{w.ProcessId}",
            w => w.Title,
            w => w.ClassName,
        });
        Console.WriteLine(rowText);
    }
    await Task.Delay(200).ConfigureAwait(false);
}
