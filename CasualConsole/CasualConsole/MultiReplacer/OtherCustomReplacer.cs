using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualConsole.MultiReplacer
{
    public class OtherCustomReplacer
    {
        public string Replace(string s, Dictionary<string, string> dic)
        {
            IEnumerable<string> arr = new string[] { s };

            foreach (var item in dic)
            {
                var split = arr.Select(x => x.Split(new string[] { item.Key }, StringSplitOptions.None));
                arr = split.Select(x => Join(x, item.Value)).SelectMany(x => x);
            }

            return String.Concat(arr);
        }

        public IEnumerable<string> Join(string[] arr, string value)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                yield return arr[i];
                yield return value;
            }

            yield return arr[arr.Length - 1];
        }
    }
}

