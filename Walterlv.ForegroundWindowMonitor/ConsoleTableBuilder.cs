using System.Text;

namespace Walterlv.ForegroundWindowMonitor;
/// <summary>
/// 为在控制台中输出表格提供支持。
/// </summary>
/// <typeparam name="T">表格中的每一行的数据类型。</typeparam>
public class ConsoleTableBuilder<T> where T : notnull
{
    private readonly int _tableWidth;
    private readonly ConsoleTableColumnDefinition<T>[] _headers;
    private readonly int[] _columnWidths;

    /// <summary>
    /// 创建 <see cref="ConsoleTableBuilder{T}"/> 的新实例。
    /// </summary>
    /// <param name="tableWidth">表格的总字符宽度。</param>
    /// <param name="headers">表格的列定义。</param>
    public ConsoleTableBuilder(int tableWidth, IReadOnlyList<ConsoleTableColumnDefinition<T>> headers)
    {
        _tableWidth = tableWidth;
        _headers = headers.ToArray();
        _columnWidths = CalculateColumnWidths(tableWidth, _headers);
    }

    /// <summary>
    /// 获取一个用来在控制台输出的表头行字符串。
    /// </summary>
    /// <returns>表头行字符串。</returns>
    public string BuildHeaderRows()
    {
        var sb = new StringBuilder();

        // 标题首行。
        for (var i = 0; i < _headers.Length; i++)
        {
            sb.Append(i switch
            {
                0 => '┌',
                _ => '┬',
            });
            sb.Append('─', _columnWidths[i] + 2);
            if (i == _headers.Length - 1)
            {
                sb.Append('┐');
            }
        }
        sb.AppendLine();

        // 标题文字行。
        for (var i = 0; i < _headers.Length; i++)
        {
            var header = _headers[i];
            sb.Append('│');
            var width = _columnWidths[i];
            sb.Append(' ').Append(header.Text);
            sb.Append(' ', width + 1 - header.Text.Length);
            if (i == _headers.Length - 1)
            {
                sb.Append('│');
            }
        }
        sb.AppendLine();

        // 标题尾行。
        for (var i = 0; i < _headers.Length; i++)
        {
            sb.Append(i switch
            {
                0 => '├',
                _ => '┼',
            });
            sb.Append('─', _columnWidths[i] + 2);
            if (i == _headers.Length - 1)
            {
                sb.Append('┤');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 获取一个用来在控制台输出 <typeparamref name="T"/> 类型数据的行字符串。
    /// </summary>
    /// <param name="object">要输出的数据。</param>
    /// <param name="displayMode">指定当字符串长度超过可显示长度时，应如何显示。</param>
    /// <returns>数据行字符串。</returns>
    public string BuildRow(T @object, StringDisplayMode displayMode = StringDisplayMode.Truncate)
    {
        var lines = new List<string[]>();

        for (var i = 0; i < _headers.Length; i++)
        {
            var width = _columnWidths[i];
            var value = _headers[i].ColumnValueFormatter(@object);
            string[] segmentedValues;

            switch (displayMode)
            {
                case StringDisplayMode.Truncate:
                    segmentedValues = new[] { value.ConsolePadRight(width, ' ', true) };
                    break;
                case StringDisplayMode.TruncateWithEllipsis:
                    var truncatedValue = value.Length > width ? value[..(width - 3)] + "..." : value;
                    segmentedValues = new[] { truncatedValue };
                    break;
                case StringDisplayMode.Wrap:
                    segmentedValues = WrapString(value, width);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            lines.Add(segmentedValues);
        }

        var maxLines = lines.Max(x => x.Length);
        var sb = new StringBuilder();
        for (var l = 0; l < maxLines; l++)
        {
            for (var i = 0; i < _headers.Length; i++)
            {
                var width = _columnWidths[i];
                var lineValue = l < lines[i].Length ? lines[i][l] : new string(' ', _columnWidths[i]);
                sb.Append('│')
                    .Append(' ')
                    .Append(lineValue.ConsolePadRight(width, ' ', true))
                    .Append(' ');
                if (i == _headers.Length - 1)
                {
                    sb.Append('│');
                }
            }
            if (l < maxLines - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string[] WrapString(string str, int width)
    {
        var lines = new List<string>();
        int currentIndex = 0;

        while (currentIndex < str.Length)
        {
            if (currentIndex + width >= str.Length)
            {
                lines.Add(str.Substring(currentIndex));
            }
            else
            {
                lines.Add(str.Substring(currentIndex, width));
            }
            currentIndex += width;
        }

        return lines.ToArray();
    }


    /// <summary>
    /// 获取一个新的 <see cref="ConsoleTableBuilder{T}"/> 实例，该实例的表格宽度为新的指定值。
    /// </summary>
    /// <param name="newTableWidth">新的表格宽度。</param>
    /// <returns>新的 <see cref="ConsoleTableBuilder{T}"/> 实例。</returns>
    public ConsoleTableBuilder<T> NewTableWidth(int newTableWidth)
    {
        return new ConsoleTableBuilder<T>(newTableWidth, _headers);
    }

    /// <summary>
    /// 根据表格的字符总宽度和列定义，计算每一列不含表格框架字符的字符宽度。
    /// </summary>
    /// <param name="tableWidth">表格的字符总宽度。</param>
    /// <param name="headers">表格的列定义。</param>
    /// <returns>每一列不含表格框架字符的字符宽度。</returns>
    private static int[] CalculateColumnWidths(int tableWidth, IReadOnlyList<ConsoleTableColumnDefinition<T>> headers)
    {
        var calculatedCount = 0;
        var remainingWidth = tableWidth;
        var widths = new int[headers.Count];
        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i];
            if (header.Width is not 0)
            {
                calculatedCount++;
                remainingWidth -= header.Width;
                widths[i] = header.Width;
            }
            remainingWidth -= 3;
        }
        remainingWidth -= 1;
        var percentWidths = remainingWidth;
        var totalPercent = 0d;
        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i];
            if (header.WidthPercent is not 0)
            {
                totalPercent += header.WidthPercent;
            }
        }
        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i];
            if (header.WidthPercent is not 0)
            {
                var width = (int)Math.Round((header.WidthPercent / totalPercent) * percentWidths);
                calculatedCount++;
                if (calculatedCount == headers.Count)
                {
                    widths[i] = remainingWidth;
                }
                else
                {
                    widths[i] = width;
                }
                remainingWidth -= width;
            }
        }
        return widths;
    }
}
