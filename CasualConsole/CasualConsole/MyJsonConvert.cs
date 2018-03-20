using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CasualConsole
{
    public class MyJsonConvert
    {
        public static T CustomJsonParse<T>(string jsonText)
        {
            JToken jval = JToken.Parse(jsonText);

            return (T)JsonCastValue(jval, typeof(T));
        }

        public static object CustomJsonParse(string jsonText, Type type)
        {
            JToken jval = JToken.Parse(jsonText);

            return JsonCastValue(jval, type);
        }

        private static object JsonCastValue(JToken jval, Type type)
        {
            if (type == typeof(string))
            {
                return jval.ToObject(type);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type)) // Expected type is IEnumerable
            {
                Type ienumerableType = type.GetGenericArguments().FirstOrDefault();
                Type underlyingType = ienumerableType ?? type.GetElementType();

                bool isArray = ienumerableType == null;

                if (jval is JArray jarr)
                {
                    int length = jarr.Count;

                    if (isArray)
                    {
                        dynamic arr = Activator.CreateInstance(type, new object[] { length });

                        for (int i = 0; i < length; i++)
                        {
                            dynamic subValue = JsonCastValue(jarr[i], underlyingType);
                            arr[i] = subValue;
                        }

                        return arr;
                    }
                    else
                    {
                        dynamic list = Activator.CreateInstance(type);

                        for (int i = 0; i < length; i++)
                        {
                            dynamic subValue = JsonCastValue(jarr[i], underlyingType);
                            list.Add(subValue);
                        }

                        return list;
                    }
                }
                else
                {
                    dynamic subValue = JsonCastValue(jval, underlyingType);

                    if (isArray)
                    {
                        dynamic arr = Activator.CreateInstance(type, new object[] { 1 });
                        arr[0] = subValue;
                        return arr;
                    }
                    else
                    {
                        dynamic list = Activator.CreateInstance(type, new object[] { 1 });
                        list.Add(subValue);
                        return list;
                    }
                }
            }
            else // Expected type is either a basic type or object
            {
                if (jval is JArray jarr)
                {
                    throw new Exception("The value needs to be IEnumerable");
                }
                else if (jval is JObject jobj) // The value is a JS object
                {
                    dynamic obj = Activator.CreateInstance(type);

                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        JToken subval = jval[prop.Name];

                        if (subval != null)
                        {
                            dynamic convertedVal = JsonCastValue(subval, prop.PropertyType);

                            prop.SetValue(obj, convertedVal);
                        }
                    }

                    foreach (FieldInfo field in type.GetFields())
                    {
                        JToken subval = jval[field.Name];

                        if (subval != null)
                        {
                            dynamic convertedVal = JsonCastValue(subval, field.FieldType);

                            field.SetValue(obj, convertedVal);
                        }
                    }

                    return obj;
                }
                else // The value is a basic type
                {
                    return jval.ToObject(type);
                }
            }
        }
    }
}
