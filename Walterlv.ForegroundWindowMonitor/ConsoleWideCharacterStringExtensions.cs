using System.Text;

namespace Walterlv.ForegroundWindowMonitor;
public static class ConsoleWideCharacterStringExtensions
{
    public static int GetConsoleLength(this string str)
    {
        int lenTotal = 0;
        int n = str.Length;
        string strWord = "";
        int asc;
        for (int i = 0; i < n; i++)
        {
            strWord = str.Substring(i, 1);
            asc = Convert.ToChar(strWord);
            if (asc < 0 || asc > 127)
            {
                lenTotal = lenTotal + 2;
            }
            else
            {
                lenTotal = lenTotal + 1;
            }
        }
        return lenTotal;
    }

    public static string ConsolePadRight(this string strOriginal, int maxTrueLength, char chrPad, bool blnCutTail)
    {
        string strNew = strOriginal;
        if (strOriginal == null || maxTrueLength <= 0)
        {
            strNew = "";
            return strNew;
        }

        int trueLen = GetConsoleLength(strOriginal);
        if (trueLen > maxTrueLength)
        {
            if (blnCutTail)
            {
                for (int i = strOriginal.Length - 1; i > 0; i--)
                {
                    strNew = strNew.Substring(0, i);
                    if (GetConsoleLength(strNew) == maxTrueLength)
                    {
                        break;
                    }
                    else if (GetConsoleLength(strNew) < maxTrueLength)
                    {
                        strNew += chrPad.ToString();
                        break;
                    }
                }
            }
        }
        else// 填充
        {
            for (int i = 0; i < maxTrueLength - trueLen; i++)
            {
                strNew += chrPad.ToString();
            }
        }
        return strNew;
    }

    public static string ConsoleSubString(this string str, int count)
    {
        byte[] bwrite = Encoding.GetEncoding("GB2312").GetBytes(str.ToCharArray());
        if (bwrite.Length >= count)
        {
            return Encoding.Default.GetString(bwrite, 0, count);
        }
        else
        {
            return Encoding.Default.GetString(bwrite);
        }
    }
}
