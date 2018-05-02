using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

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
        
        public static XmlNode GetChildNamed(this XmlNode node, string name)
        {
            return node.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == name);
        }
    }
}
