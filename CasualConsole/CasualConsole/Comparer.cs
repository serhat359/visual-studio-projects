using System;
using System.Collections.Generic;

namespace CasualConsole
{
    public class Comparer<T> : IComparer<T>
    {
        Func<T, T, Comparer.CompareResult> comparer;

        public Comparer(Func<T, T, Comparer.CompareResult> comparer)
        {
            this.comparer = comparer;
        }

        public int Compare(T t1, T t2)
        {
            return (int)comparer(t1, t2);
        }
    }

    public class Comparer {
        public enum CompareResult : int
        {
            Greater = 1,
            Less = -1,
            Equal = 0
        }
    }
}
