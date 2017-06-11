using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CasualConsole
{
    public static class Ext
    {
        public static void Each<T>(this IEnumerable<T> list, Action<T, int> action)
        {
            int i = 0;

            foreach (T t in list)
            {
                action(t, i++);
            }
        }

        public static void Each<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (T t in list)
            {
                action(t);
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

        public static string NameOf<E>(Expression<Func<E>> expr)
        {
            return ((expr.Body as MemberExpression).Member as PropertyInfo).Name;
        }
        
        public static int DoOrDie(Func<int> action, string errorMessage)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                throw new Exception(errorMessage, e);
            }
        }

    }
}
