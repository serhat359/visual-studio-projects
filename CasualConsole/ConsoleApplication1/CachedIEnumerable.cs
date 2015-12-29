using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ConsoleApplication1
{
    class CachedIEnumerable<T> : IEnumerable<T>
    {
        IEnumerable<T> source;
        List<T> list = null;

        public CachedIEnumerable(IEnumerable<T> enumerable)
        {
            source = enumerable;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (list != null)
                return list.GetEnumerator();
            else
                return GetValuesCaching().GetEnumerator();
        }

        private IEnumerable<T> GetValuesCaching()
        {
            list = new List<T>();

            foreach (T item in source)
            {
                list.Add(item);
                yield return item;
            }
        }
    }
}
