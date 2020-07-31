using System.Collections.Generic;
using System.Text;

namespace CasualConsole.MultiReplacer
{
    public class StringBuilderReplacer
    {
        public StringBuilderReplacer()
        {
        }

        public string Replace(string s, Dictionary<string, string> dic)
        {
            var sb = new StringBuilder(s);

            foreach (var item in dic)
            {
                sb.Replace(item.Key, item.Value);
            }

            return sb.ToString();
        }
    }
}
