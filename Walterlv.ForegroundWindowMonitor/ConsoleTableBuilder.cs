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
    /// <returns>数据行字符串。</returns>
    public string BuildRow(T @object)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < _headers.Length; i++)
        {
            var width = _columnWidths[i];
            var value = _headers[i].ColumnValueFormatter(@object);

            sb.Append('│')
                .Append(' ')
                .Append(value.ConsolePadRight(width, ' ', true))
                .Append(' ');
            if (i == _headers.Length - 1)
            {
                sb.Append('│');
            }
        }

        return sb.ToString();
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

/// <summary>
/// 表示一个控制台表格的列定义。
/// </summary>
/// <typeparam name="T">表格中每一行的数据类型。</typeparam>
public readonly record struct ConsoleTableColumnDefinition<T> where T : notnull
{
    public ConsoleTableColumnDefinition(string text, Func<T, string> columnValueFormatter)
    {
        Width = text.Length;
        WidthPercent = 0;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ColumnValueFormatter = columnValueFormatter;
    }

    public ConsoleTableColumnDefinition(int width, string text, Func<T, string> columnValueFormatter)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0.");
        }

        Width = width;
        WidthPercent = 0;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ColumnValueFormatter = columnValueFormatter;
    }

    public ConsoleTableColumnDefinition(double widthPercent, string text, Func<T, string> columnValueFormatter)
    {
        if (widthPercent <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(widthPercent), widthPercent, "Width percent must be greater than 0.");
        }

        Width = 0;
        WidthPercent = widthPercent;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ColumnValueFormatter = columnValueFormatter;
    }

    /// <summary>
    /// 获取列的字符显示宽度。
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 获取列的字符显示宽度百分比。
    /// </summary>
    /// <remarks>
    /// 指定了 <see cref="Width"/> 的列不参与计算百分比，其他列按百分比分剩余宽度。
    /// <para/>
    /// 所有列宽度百分比的总和允许大于 100%。当大于时，会压缩每一列按百分比计算的宽度。
    /// </remarks>
    public double WidthPercent { get; }

    /// <summary>
    /// 获取列的标题。
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// 获取列的值格式化器。
    /// </summary>
    public Func<T, string> ColumnValueFormatter { get; }

    public static implicit operator ConsoleTableColumnDefinition<T>(string headerText)
    {
        return new ConsoleTableColumnDefinition<T>(headerText, v => v.ToString()!);
    }

    public static implicit operator ConsoleTableColumnDefinition<T>((int Width, string Text) header)
    {
        return new ConsoleTableColumnDefinition<T>(header.Width, header.Text, v => v.ToString()!);
    }

    public static implicit operator ConsoleTableColumnDefinition<T>((int Width, string Text, Func<T, string> ColumnValueFormatter) header)
    {
        return new ConsoleTableColumnDefinition<T>(header.Width, header.Text, header.ColumnValueFormatter);
    }

    public static implicit operator ConsoleTableColumnDefinition<T>((double WidthPercent, string Text) header)
    {
        return new ConsoleTableColumnDefinition<T>(header.WidthPercent, header.Text, v => v.ToString()!);
    }

    public static implicit operator ConsoleTableColumnDefinition<T>((double WidthPercent, string Text, Func<T, string> ColumnValueFormatter) header)
    {
        return new ConsoleTableColumnDefinition<T>(header.WidthPercent, header.Text, header.ColumnValueFormatter);
    }
}
