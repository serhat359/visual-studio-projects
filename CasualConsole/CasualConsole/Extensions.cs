using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;

namespace CasualConsole
{
    public static class Extensions
    {
        public static void Each<T>(this IEnumerable<T> list, Action<T, int> action)
        {
            int i = 0;

            foreach (T t in list)
            {
                action(t, i++);
            }
        }

        public static V GetValueOrNew<K, V>(this Dictionary<K, V> dic, K key) where V : new()
        {
            V value;

            if (dic.TryGetValue(key, out value))
            {
                
            }
            else
            {
                value = new V();
                dic[key] = value;
            }

            return value;
        }

        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dic, K key)
        {
            V value;

            if (dic.TryGetValue(key, out value))
                return value;
            else
                return default(V);
        }

        public static IEnumerable<object> AsEnumerable(IEnumerable collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }

        public static IEnumerable<Group> GroupList(this Match match)
        {
            for (int i = 0; i < match.Groups.Count; i++)
            {
                yield return match.Groups[i];
            }
        }
    }
}
