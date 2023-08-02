using System.Collections.Immutable;

namespace Walterlv.ForegroundWindowMonitor;
public class ConsoleTableColumnEditor
{
    private static readonly ImmutableArray<ConsoleTableColumnDefinition<Win32Window>> AvailableColumns = new ConsoleTableColumnDefinition<Win32Window>[]
    {
        (8, "time", _ => $"{DateTime.Now:HH:mm:ss}"),
        (8, "hwnd", w => $"{w.Handle:X8}"),
        (0.5, "title", w => w.Title),
        (0.25, "class name", w => w.ClassName),
        (6, "pid", w => $"{w.ProcessId}"),
        (0.25, "process name", w => $"{w.ProcessName}"),
    }.ToImmutableArray();

    private static readonly ImmutableArray<int> AllIndexes = Enumerable.Range(0, AvailableColumns.Length).ToImmutableArray();

    public ConsoleTableBuilder<Win32Window> CreateTableBuilder() => CreateTableBuilder(AllIndexes);

    public ConsoleTableBuilder<Win32Window> CreateTableBuilder(IReadOnlyList<int> indexes)
    {
        var tableWidth = Console.WindowWidth;
        var columns = IndexToColumnDefinition(indexes);
        return new ConsoleTableBuilder<Win32Window>(tableWidth, columns.ToArray());

        static IEnumerable<ConsoleTableColumnDefinition<Win32Window>> IndexToColumnDefinition(IEnumerable<int> indexes)
        {
            foreach (var index in indexes)
            {
                if (index >= 0 && index < AvailableColumns.Length)
                {
                    yield return AvailableColumns[index];
                }
            }
        }
    }
}
