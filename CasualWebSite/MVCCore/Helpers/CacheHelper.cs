using Microsoft.Extensions.Caching.Memory;

namespace MVCCore.Helpers
{
    public class CacheHelper
    {
        public const string TomsArticlesKey = nameof(TomsArticlesKey);
        public const string TomsNewsKey = nameof(TomsNewsKey);
        public const string MyRssKey = nameof(MyRssKey);
        public const string MyTorrentRssKey = nameof(MyTorrentRssKey);
        public const string MALCookie = nameof(MALCookie);
        public static TimeSpan MALCookieTimeSpan = TimeSpan.FromDays(364);

        private readonly IMemoryCache _cache;

        public CacheHelper(IMemoryCache cache)
        {
            _cache = cache;
        }

        public IMemoryCache Cache => _cache;

        public T GetNotInit<T>(string cacheKey)
        {
            return Cache.Get<T>(cacheKey);
        }

        public T Get<T>(string cacheKey, Func<T> initializer, TimeSpan timeSpan)
        {
            T obj = Cache.Get<T>(cacheKey);

            if (obj == null)
            {
                var newValue = initializer();
                if (IsTaskType(newValue?.GetType()))
                {
                    throw new Exception($"Cannot insert task type into the cache, please use {nameof(GetAsync)} method instead");
                }
                Cache.Set(cacheKey, newValue, timeSpan);
                obj = newValue;
            }

            return obj;
        }

        public async Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> initializer, TimeSpan timeSpan)
        {
            T obj = Cache.Get<T>(cacheKey);

            if (obj == null)
            {
                var newValue = await initializer();
                Cache.Set(cacheKey, newValue, timeSpan);
                obj = newValue;
            }

            return obj;
        }

        public void Set(string cacheKey, object newValue, TimeSpan timeSpan)
        {
            Cache.Set(cacheKey, newValue, timeSpan);
        }

        public void Delete(string cacheKey)
        {
            Cache.Remove(cacheKey);
        }

        private static bool IsTaskType(Type? t)
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
