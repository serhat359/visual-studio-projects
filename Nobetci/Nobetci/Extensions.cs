using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nobetci
{
    public static class Extensions
    {
        public class IndexValuePair<T>
        {
            public int Index { get; set; }
            public T Value { get; set; }
        }

        public static IEnumerable<IndexValuePair<T>> WithIndex<T>(this IEnumerable<T> elems)
        {
            int i = 0;
            foreach (var elem in elems)
            {
                yield return new IndexValuePair<T> { Index = i++, Value = elem };
            }
        }

        public static int? CastInt(object o) {
            try
            {
                return (int)o;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
