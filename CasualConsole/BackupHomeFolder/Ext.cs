using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupHomeFolder
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
    }
}
