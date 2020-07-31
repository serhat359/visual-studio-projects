using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CasualConsole.MultiReplacer
{
    public class CustomReplacer
    {
        public CustomReplacer()
        {
        }

        public string Replace(string s, Dictionary<string, string> dic)
        {
            int currentIndex = 0;
            int lastIndex = 0;
            var sb = new StringBuilder();
            var keys = dic.Keys.ToArray();
            var values = keys.Select(x => dic[x]).ToArray();
            var indexes = new int[keys.Length];

            for (int i = 0; i < keys.Length; i++)
                indexes[i] = s.IndexOf(keys[i], currentIndex);

            // code starts here

            while (true)
            {
                int minI = int.MaxValue;
                int minIndex = int.MaxValue;
                for (int i = 0; i < keys.Length; i++)
                {
                    var index = indexes[i];
                    if (index >= 0 && index < minIndex)
                    {
                        minI = i;
                        minIndex = index;
                    }
                }

                if (minIndex >= 0 && minIndex < int.MaxValue)
                {
                    sb.Append(s.Substring(currentIndex, minIndex - lastIndex));
                    sb.Append(values[minI]);
                    lastIndex = currentIndex = minIndex + keys[minI].Length;
                    indexes[minI] = s.IndexOf(keys[minI], currentIndex);
                }
                else
                {
                    sb.Append(s.Substring(currentIndex));
                    return sb.ToString();
                }
            }
        }

    }
}
