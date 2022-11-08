using System;
using System.Threading.Tasks;
using System.Web.Caching;

namespace SharePointMvc.Helpers
{
    public class CacheHelper
    {
        public const string TomsArticlesKey = nameof(TomsArticlesKey);
        public const string TomsNewsKey = nameof(TomsNewsKey);
        public const string MyRssKey = nameof(MyRssKey);
        public const string MyTorrentRssKey = nameof(MyTorrentRssKey);
        public const string MALCookie = nameof(MALCookie);
        public static TimeSpan MALCookieTimeSpan = TimeSpan.FromDays(364);

        private static Cache _cache;
        public static Cache Cache
        {
            get { return _cache ?? (_cache = new Cache()); }
        }

        public static T Get<T>(string cacheKey, Func<T> initializer, TimeSpan timeSpan)
        {
            T obj = (T)Cache[cacheKey];

            if (obj == null)
            {
                var newValue = initializer();
                if (IsTaskType(newValue?.GetType()))
                {
                    throw new Exception($"Cannot insert task type into the cache, please use {nameof(GetAsync)} method instead");
                }
                Cache.Insert(cacheKey, newValue, null, Cache.NoAbsoluteExpiration, timeSpan);
                obj = newValue;
            }

            return obj;
        }

        public static async Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> initializer, TimeSpan timeSpan)
        {
            T obj = (T)Cache[cacheKey];

            if (obj == null)
            {
                var newValue = await initializer();
                Cache.Insert(cacheKey, newValue, null, Cache.NoAbsoluteExpiration, timeSpan);
                obj = newValue;
            }

            return obj;
        }

        public static void Set(string cacheKey, object newValue, TimeSpan timeSpan)
        {
            Cache.Insert(cacheKey, newValue, null, Cache.NoAbsoluteExpiration, timeSpan);
        }

        public static void Delete(string cacheKey)
        {
            Cache.Remove(cacheKey);
        }

        private static bool IsTaskType(Type t)
        {
            if (t == null)
                return false;
            if (t == typeof(Task))
                return true;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>))
                return true;
            return false;
        }
    }
}