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

        public static bool ContainsCaseInsensitive(this string source, string value)
        {
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) < 0;
        }
        
        public static XmlNode GetChildNamed(this XmlNode node, string name)
        {
            return node.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == name);
        }

        public static IEnumerable<XmlNode> GetAllNodes(this XmlNode node)
        {
            yield return node;

            foreach (XmlNode item in node.ChildNodes)
            {
                var allNodes = item.GetAllNodes();

                foreach (var newNode in allNodes)
                {
                    yield return newNode;
                }
            }
        }

        public static XmlNode SearchByTag(this XmlNode node, string name, string @class = null)
        {
            if (@class == null)
                return node.GetAllNodes().First(c => c.Name == name);
            else
                return node.GetAllNodes().First(c => c.Name == name && c.Attributes?["class"]?.Value == @class);
        }

        public static int IndexOfFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }
    }
}
