using System.Collections;
using System.Collections.Generic;

namespace TorrentSeedLeechCounter
{
    public static class Extensions
    {
        public static IEnumerable<T> AsIterable<T>(this IEnumerable values)
        {
            foreach (object item in values)
            {
                yield return (T)item;
            }
        }
    }
}
