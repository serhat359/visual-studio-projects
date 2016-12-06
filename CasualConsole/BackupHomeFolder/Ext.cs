using System;
using System.Collections.Generic;

namespace BackupHomeFolder
{
    public static class Ext
    {
        public static void Each<T>(this IEnumerable<T> list, Func<T, int, bool> action)
        {
            int i = 0;

            foreach (T t in list)
            {
                bool continueLoop = action(t, i++);

                if (continueLoop)
                    continue;
                else
                    break;
            }
        }
    }
}
