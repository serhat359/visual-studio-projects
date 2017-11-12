﻿using System;

namespace Extensions
{
    public static class Extensions
    {
        public static T GetPropertyOrDefault<T, O>(this O obj, Func<O, T> selector) where O : class
        {
            return GetPropertyOrDefault(obj, selector, default(T));
        }

        public static T GetPropertyOrDefault<T, O>(this O obj, Func<O, T> selector, T defaultValue) where O : class
        {
            if (obj != null)
                return selector(obj);
            else
                return defaultValue;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return str == null || str.Equals("");
        }
    }
}
