using System.Collections.Generic;
using System.Text;

namespace CasualConsole
{
    public class StringJoin
    {
        public static string Join(string[] source, char c)
        {
            var ss = new StringBuilder();

            var sourceLength = source.Length;
            var lastIndex = sourceLength - 1;
            for (int i = 0; i < sourceLength; i++)
            {
                var str = source[i];

                for (int j = 0; j < str.Length; j++)
                {
                    var cstr = str[j];

                    ss.Append(cstr);

                    if (cstr == c)
                        ss.Append(cstr);
                }

                if (i != lastIndex)
                    ss.Append(c);
            }

            return ss.ToString();
        }

        public static string[] Split(string str, char c)
        {
            var list = new List<string>();

            int lastIndex = 0;
            int i = 0;
            while (true)
            {
                if (i >= str.Length)
                    break;

                var charat = CharAt(str, i);

                if (charat != c)
                {
                    i++;
                    continue;
                }

                var charNext = CharAt(str, i + 1);
                if (charNext == c)
                {
                    i += 2;
                    continue;
                }

                // Actual Code Begins Now
                var subString = GetReplacedSubstring(str, lastIndex, i, c);
                list.Add(subString);
                lastIndex = i + 1;

                i++;
            }

            var lastSubString = GetReplacedSubstring(str, lastIndex, i, c);
            list.Add(lastSubString);

            return list.ToArray();
        }

        private static char? CharAt(string str, int i)
        {
            if (i >= str.Length)
                return null;
            else
                return str[i];
        }

        private static string GetReplacedSubstring(string str, int startIndex, int endIndex, char c)
        {
            var ss = new StringBuilder();
            for (int i = startIndex; i < endIndex; i++)
            {
                char charAt = str[i];
                ss.Append(charAt);
                if (charAt == c)
                {
                    i++;
                }
            }
            return ss.ToString();
        }
    }
}
