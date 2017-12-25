using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable collection)
        {
            foreach (var item in collection)
            {
                yield return (T)item;
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

        public static string ReplaceOnce(this string text, string oldValue, string newValue)
        {
            var regex = new Regex(Regex.Escape(oldValue));
            var newText = regex.Replace(text, newValue, 1);
            return newText;
        }

        // An extension to access UI elements from another thread safely
        public static void ThreadSafe<T>(this T control, Action<T> action) where T : Control
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke((MethodInvoker)delegate ()
                {
                    action(control);
                });
            }
            else
            {
                action(control);
            }
        }

        public static CachedFunc<T> CacheFunction<T>(Func<T> func)
        {
            return new CachedFunc<T>(func);
        }

        public class CachedFunc<T>
        {
            bool isValueSet = false;
            T value;
            Func<T> func;

            public CachedFunc(Func<T> func)
            {
                this.func = func;
            }

            public T Call()
            {
                if (!isValueSet)
                {
                    value = func();
                    isValueSet = true;
                }

                return value;
            }
        }

        public static partial class Enumerable
        {
            public static IEnumerable<int> RangeByEnd(int start, int end)
            {
                while (start < end)
                {
                    yield return start++;
                }
            }
        }

        public static bool SafeQueueDoJob<T>(this Queue<T> queue, Action<T> action)
        {
            T queueItem = default(T);
            bool hasJob = false;

            lock (queue)
            {
                if (queue.Count > 0)
                {
                    queueItem = queue.Dequeue();
                    hasJob = true;
                }
            }

            if (hasJob)
                action(queueItem);

            return hasJob;
        }

        public static T LastItem<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }
    }
}

