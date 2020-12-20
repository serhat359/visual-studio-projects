using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace CasualConsole
{
    public static class Extensions
    {
        static JsonSerializer serializer = new JsonSerializer();

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

        public static T MinBy<T, E>(this IEnumerable<T> source, Func<T, E> selector) where E : IComparable<E>
        {
            return MinBy(source, selector, out var v);
        }

        public static T MinBy<T, E>(this IEnumerable<T> source, Func<T, E> selector, out E minimumValue) where E : IComparable<E>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var sourceEnumerator = source.GetEnumerator();

            var hasNoValue = !sourceEnumerator.MoveNext();
            if (hasNoValue)
                throw new InvalidOperationException("Sequence contains no elements");

            T minElem = sourceEnumerator.Current;
            E minValue = selector(minElem);

            while (sourceEnumerator.MoveNext())
            {
                T newElem = sourceEnumerator.Current;
                E newValue = selector(newElem);

                if (newValue.CompareTo(minValue) < 0)
                {
                    minElem = newElem;
                    minValue = newValue;
                }
            }

            minimumValue = minValue;
            return minElem;
        }

        public static IEnumerable<(T elem, int index)> SelectI<T>(this IEnumerable<T> elements)
        {
            int i = 0;

            foreach (var item in elements)
                yield return (item, i++);
        }

        public static T FirstOrCustom<T>(this IEnumerable<T> list, T customValue)
        {
            if (list.Any())
                return list.First();
            else
                return customValue;
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> list)
        {
            if (list != null)
                return list;

            return new T[] { };
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

            if (dic.TryGetValue(key, out V value))
                return value;
            else
                return default(V);
        }

        public static E? GetValueOrNull<T, E>(this Dictionary<T, E> dictionary, T key) where E : struct
        {
            if (dictionary.TryGetValue(key, out E value))
                return value;
            else
                return null;
        }

        public static bool SafeEquals<T>(this IEnumerable<T> collection, IEnumerable<T> other) where T : IEquatable<T>
        {
            return SafeEquals(collection, other, (x, y) => x.Equals(y));
        }

        public static bool SafeEquals<T>(this IEnumerable<T> collection, IEnumerable<T> other, IEqualityComparer<T> comparer)
        {
            return SafeEquals(collection, other, (x, y) => comparer.Equals(x, y));
        }

        public static bool SafeEquals<T>(this IEnumerable<T> collection, IEnumerable<T> other, Func<T, T, bool> comparer)
        {
            if (collection == null && other == null)
                return true;

            if (collection == null || other == null)
                return false;

            var firstEnumerator = collection.GetEnumerator();
            var secondEnumerator = other.GetEnumerator();

            while (true)
            {
                var firstMoveNext = firstEnumerator.MoveNext();
                var secondMoveNext = secondEnumerator.MoveNext();

                if (firstMoveNext == true && secondMoveNext == false)
                    return false;

                if (firstMoveNext == false && secondMoveNext == true)
                    return false;

                if (firstMoveNext == false && secondMoveNext == false)
                    break;

                if (!comparer(firstEnumerator.Current, secondEnumerator.Current))
                    return false;
            }

            return true;
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

        public static T LastItem<T>(this T[] array)
        {
            return array[array.Length - 1];
        }

        public static T[] AddSize<T>(this T[] array, int increment)
        {
            T[] newArray = new T[array.Length + increment];

            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            return newArray;
        }

        public static T[] AddSizeWithNew<T>(this T[] array, int increment) where T : new()
        {
            T[] newArray = AddSize(array, increment);

            for (int i = array.Length; i < array.Length + increment; i++)
            {
                newArray[i] = new T();
            }

            return newArray;
        }

        public static XmlNode GetChildNamed(this XmlNode node, string name)
        {
            return node.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == name);
        }

        public static bool ContainsCaseInsensitive(this string whole, string substring)
        {
            bool contains = whole.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;

            return contains;
        }

        public static void SortBy<T, E>(this List<T> list, Func<T, E> selector)
            where E : IComparable<E>
        {
            list.Sort((x, y) => selector(x).CompareTo(selector(y)));
        }

        public static void SortBy<T, E1, E2>(this List<T> list, Func<T, E1> selector1, Func<T, E2> selector2)
            where E1 : IComparable<E1>
            where E2 : IComparable<E2>
        {
            list.Sort((x, y) => {
                var case1 = selector1(x).CompareTo(selector1(y));
                if (case1 != 0) return case1;

                var case2 = selector2(x).CompareTo(selector2(y));
                return case2;
            });
        }

        public static void SortBy<T, E1, E2, E3>(this List<T> list, Func<T, E1> selector1, Func<T, E2> selector2, Func<T, E3> selector3)
            where E1 : IComparable<E1>
            where E2 : IComparable<E2>
            where E3 : IComparable<E3>
        {
            list.Sort((x, y) => {
                var case1 = selector1(x).CompareTo(selector1(y));
                if (case1 != 0) return case1;

                var case2 = selector2(x).CompareTo(selector2(y));
                if (case2 != 0) return case2;

                var case3 = selector3(x).CompareTo(selector3(y));
                return case3;
            });
        }

        [Obsolete("It is probably a better idea to use Dictionary instead, trust me")]
        public static int BinarySearchBy<T, E>(this List<T> list, Func<T, E> selector, E target) where E : IComparable<E>
        {
            int first = 0;
            int last = list.Count - 1;
            int mid;

            while (first <= last)
            {
                mid = (first + last) / 2;

                if (target.IsLessThan(selector(list[mid])))
                {
                    last = mid - 1;
                }
                else if (target.IsGreaterThan(selector(list[mid])))
                {
                    first = mid + 1;
                }
                else
                {
                    return mid;
                }
            }

            return -1;
        }

        [Obsolete("It is probably a better idea to use Dictionary instead, trust me")]
        public static int BinarySearchByDescending<T, E>(this List<T> list, Func<T, E> selector, E target) where E : IComparable<E>
        {
            int first = 0;
            int last = list.Count - 1;
            int mid;

            while (first <= last)
            {
                mid = (first + last) / 2;

                if (target.IsGreaterThan(selector(list[mid])))
                {
                    last = mid - 1;
                }
                else if (target.IsLessThan(selector(list[mid])))
                {
                    first = mid + 1;
                }
                else
                {
                    return mid;
                }
            }

            return -1;
        }

        private static System.Collections.Generic.Comparer<T> MakeComparer<T, E>(Func<T, E> func) where E : IComparable<E>
        {
            return System.Collections.Generic.Comparer<T>.Create((x, y) => func(x).CompareTo(func(y)));
        }

        public static bool IsLessThan<E>(this E first, E second) where E : IComparable<E>
        {
            return first.CompareTo(second) < 0;
        }

        public static bool IsGreaterThan<E>(this E first, E second) where E : IComparable<E>
        {
            return first.CompareTo(second) > 0;
        }

        public static T ParseJson<T>(this Stream stream)
        {
            using (var otherstream = stream)
            using (var sr = new StreamReader(otherstream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                T obj = serializer.Deserialize<T>(jsonTextReader);
                return obj;
            }
        }
    }
}

