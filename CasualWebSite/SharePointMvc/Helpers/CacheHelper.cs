using System;
using System.Web.Caching;

namespace SharePointMvc.Helpers
{
    public class CacheHelper<T>
    {
        public const string TomsArticlesKey = nameof(TomsArticlesKey);

        private static Cache _cache;
        public static Cache Cache
        {
            get { return _cache ?? (_cache = new Cache()); }
        }

        public static T GetTomsArticlesKey(Func<T> initializer)
        {
            var cacheKey = TomsArticlesKey;

            T obj = (T)Cache[cacheKey];

            if (obj == null)
            {
                var newValue = initializer();
                Cache.Insert(cacheKey, newValue, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(2));
                obj = newValue;
            }

            return obj;
        }
    }
}