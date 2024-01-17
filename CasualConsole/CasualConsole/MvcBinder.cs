using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CasualConsole
{
    public enum HttpMethod { Get, Post }
    class HttpAttribute : Attribute { }
    class GetAttribute : HttpAttribute { }
    class PostAttribute : HttpAttribute { }

    public class MvcBinder
    {
        public static void Test()
        {
            var callTestsGet = new Dictionary<string, object>
            {
                { "/MvcBinder/Number?num=34", 34 },
                { "/MvcBinder/ToString?num=27", "27" },
                { "/MvcBinder/ReturnString?str=%E2%98%91", "☑" },
            };

            foreach (var test in callTestsGet)
            {
                var result = Call(test.Key, HttpMethod.Get);
                if (!result.Equals(test.Value))
                {
                    throw new Exception("Test Fail!!!");
                }
            }

            var callTestsPost = new Dictionary<string, object>
            {
                { "/MvcBinder/Number?num=5", 5 },
                { "/MvcBinder/LoneFunction?", new DateTime(2018, 1, 1) },
                { "/MvcBinder/GetMethod?number=2", "2" },
            };

            foreach (var test in callTestsPost)
            {
                var result = Call(test.Key, HttpMethod.Post);
                if (!result.Equals(test.Value))
                {
                    throw new Exception("Test Fail!!!");
                }
            }
        }

        public static object Call(string url, HttpMethod callMethod)
        {
            var parts = url.Split('?');
            var left = parts[0];
            var right = parts.Length >= 1 ? parts[1] : null;

            var urlParts = left.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            Type attributeType = callMethod == HttpMethod.Get ? typeof(GetAttribute)
                : callMethod == HttpMethod.Post ? typeof(PostAttribute)
                : null;

            Type type = Type.GetType("CasualConsole." + urlParts[0] + "Controller");
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.Name == urlParts[1])
                .Where(x =>
                {
                    var existingAttribute = x.GetCustomAttribute<HttpAttribute>();
                    return existingAttribute == null || existingAttribute.GetType() == attributeType;
                })
                .ToList();

            if (methods.Count == 1)
            {
                var paramListEscaped = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(right))
                {
                    paramListEscaped = right.Split('&').Select(x =>
                    {
                        var paramParts = x.Split('=');
                        return new KeyValuePair<string, string>(paramParts[0].ToLowerInvariant(), Uri.UnescapeDataString(paramParts[1]));
                    }).ToDictionary(x => x.Key, x => x.Value);
                }

                var methodInfo = methods[0];
                var methodParameters = methodInfo.GetParameters();
                object[] invokeParameters = new object[methodParameters.Length];
                for (int i = 0; i < methodParameters.Length; i++)
                {
                    var info = methodParameters[i];

                    if (paramListEscaped.TryGetValue(info.Name.ToLowerInvariant(), out string value))
                    {
                        object v = Convert.ChangeType(value, info.ParameterType, CultureInfo.InvariantCulture);
                        invokeParameters[i] = v;
                    }
                    else
                    {
                        if (info.ParameterType != typeof(string) && info.ParameterType.BaseType == typeof(Object))
                        {
                            var allProps = info.ParameterType.GetProperties();
                            var matchingParams = paramListEscaped.Keys.Select(x => allProps.FirstOrDefault(y => y.Name.ToLowerInvariant() == x)).Where(x => x != null).ToList();
                            if (matchingParams.Any())
                            {
                                var model = info.ParameterType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                                foreach (var prop in matchingParams)
                                {
                                    string propName = prop.Name.ToLowerInvariant();
                                    prop.SetValue(model, Convert.ChangeType(paramListEscaped[propName], prop.PropertyType, CultureInfo.InvariantCulture));
                                }
                                invokeParameters[i] = model;
                            }
                        }
                    }
                }
                return methodInfo.Invoke(null, invokeParameters);
            }
            else if (methods.Count > 1)
            {
                throw new Exception("Too many methods match the description, try to be more specific");
            }
            else
            {
                throw new Exception("Method not found");
            }
        }
    }
}
