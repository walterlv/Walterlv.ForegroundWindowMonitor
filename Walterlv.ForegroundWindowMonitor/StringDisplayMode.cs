namespace Walterlv.ForegroundWindowMonitor;

/// <summary>
/// 当字符串长度超过指定长度时，如何处理字符串。
/// </summary>
public enum StringDisplayMode
{
    /// <summary>
    /// 截断字符串。
    /// </summary>
    Truncate,

    /// <summary>
    /// 截断字符串，并在末尾添加省略号。
    /// </summary>
    TruncateWithEllipsis,

    /// <summary>
    /// 将字符串换行。
    /// </summary>
    Wrap,
}
