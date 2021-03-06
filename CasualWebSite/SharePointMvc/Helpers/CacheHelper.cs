﻿using System;
using System.Web.Caching;

namespace SharePointMvc.Helpers
{
    public class CacheHelper
    {
        public const string TomsArticlesKey = nameof(TomsArticlesKey);
        public const string MyRssKey = nameof(MyRssKey);
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
    }
}