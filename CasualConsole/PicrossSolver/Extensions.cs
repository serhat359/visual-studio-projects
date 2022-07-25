using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicrossSolver
{
    static class Extensions
    {
        public static T[] AddSize<T>(this T[] array, int increment)
        {
            T[] newArray = new T[array.Length + increment];

            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            return newArray;
        }

        public static T LastItem<T>(this List<T> list)
        {
            return list[list.Count - 1];
        }

        public static T LastItem<T>(this T[] array)
        {
            return array[array.Length - 1];
        }

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
    }
}
