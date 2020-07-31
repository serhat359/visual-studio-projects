using System.Collections.Generic;

namespace CasualConsole.MultiReplacer
{
    public class SimpleReplacer
    {
        public SimpleReplacer()
        {
        }

        public string Replace(string s, Dictionary<string, string> dic)
        {
            foreach (var item in dic)
            {
                s = s.Replace(item.Key, item.Value);
            }

            return s;
        }
    }
}
