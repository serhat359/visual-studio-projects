using System;
using System.IO;
using System.Text;

namespace CasualConsoleCore;

public class PinyinConverter
{
    private static readonly string pydictionaryText = File.ReadAllText("../../../PinyinSource.txt");

    public static string Convert(string str)
    {
        var sb = new StringBuilder();

        foreach (var c in str)
        {
            var index = pydictionaryText.IndexOf(c);
            if (index < 0)
            {
                sb.Append(c);
                continue;
            }

            index++;
            var commaIndex = pydictionaryText.IndexOf(',', index);
            if (commaIndex < 0)
                throw new Exception();
            var sub = pydictionaryText[index..commaIndex];
            sb.Append(sub);
            sb.Append(" ");
        }

        return sb.ToString();
    }
}
