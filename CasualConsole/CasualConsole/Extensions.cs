using System;
using System.Collections.Generic;

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

        public static V GetValueAssuring<K, V>(this Dictionary<K, V> dic, K key) where V : new()
        {
            V value;

            if (dic.ContainsKey(key))
            {
                value = dic[key];
            }
            else
            {
                value = new V();
                dic[key] = value;
            }

            return value;
        }

        public static V GetExistingOrDefault<K, V>(this Dictionary<K, V> dic, K key)
        {
            if (dic.ContainsKey(key))
                return dic[key];
            else
                return default(V);
        }
    }
}
