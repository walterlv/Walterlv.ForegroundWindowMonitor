using System.Text;

namespace Walterlv.ForegroundWindowMonitor;
public class TableBuilder
{
    private readonly int _tableWidth;
    private readonly TableColumnDefinition[] _headers;
    private readonly int[] _columnWidths;

    public TableBuilder(int tableWidth, IReadOnlyList<TableColumnDefinition> headers)
    {
        _tableWidth = tableWidth;
        _headers = headers.ToArray();
        _columnWidths = CalculateColumnWidths(tableWidth, _headers);
    }

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

    public string BuildRow<T>(T @object, IReadOnlyList<Func<T, string>> valueFormatters)
    {
        if (valueFormatters.Count != _headers.Length)
        {
            throw new ArgumentException("格式化行时，值格式化器必须与列数相等。");
        }

        var sb = new StringBuilder();

        for (var i = 0; i < _headers.Length; i++)
        {
            var width = _columnWidths[i];
            var value = valueFormatters[i](@object);

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

    private int[] CalculateColumnWidths(int tableWidth, IReadOnlyList<TableColumnDefinition> headers)
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

public readonly record struct TableColumnDefinition
{
    public TableColumnDefinition(string text) : this()
    {
        Width = text.Length;
        WidthPercent = 0;
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public TableColumnDefinition(int width, string text)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0.");
        }

        Width = width;
        WidthPercent = 0;
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public TableColumnDefinition(double widthPercent, string text)
    {
        if (widthPercent <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(widthPercent), widthPercent, "Width percent must be greater than 0.");
        }

        Width = 0;
        WidthPercent = widthPercent;
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public int Width { get; }

    public double WidthPercent { get; }

    public string Text { get; }

    public static implicit operator TableColumnDefinition(string headerText)
    {
        return new TableColumnDefinition(headerText);
    }

    public static implicit operator TableColumnDefinition((int Width, string Text) header)
    {
        return new TableColumnDefinition(header.Width, header.Text);
    }

    public static implicit operator TableColumnDefinition((double WidthPercent, string Text) header)
    {
        return new TableColumnDefinition(header.WidthPercent, header.Text);
    }
}
