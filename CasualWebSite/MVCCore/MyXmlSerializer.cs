using System.Reflection;
using System.Xml.Serialization;
using System.Xml;
using MVCCore.Models.Attributes;
using System.Collections;
using System.Text;

namespace MVCCore
{
    public class MyXmlSerializer
    {
        private static readonly Type[] validTypes = new Type[] { typeof(string), typeof(int), typeof(long), typeof(float),
                typeof(double), typeof(bool), typeof(byte), typeof(char), typeof(DateTime) };

        private List<string> stringList = new List<string>();
        private int nestCount = 0;

        public string Serialize<T>(T obj, bool igroneXmlVersion = false)
        {
            if (!igroneXmlVersion)
                Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");

            SerializeElement(obj, new List<Attribute>(), xmlNodeName: null);

            return string.Concat(stringList);
        }

        public async Task SerializeToStreamAsync<T>(T obj, Stream stream, bool igroneXmlVersion = false)
        {
            if (!igroneXmlVersion)
                Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");

            SerializeElement(obj, new List<Attribute>(), xmlNodeName: null);

            var writer = new StreamWriter(stream);
            foreach (var item in stringList)
            {
                await writer.WriteAsync(item);
            }
            await writer.FlushAsync();
        }

        private static Dictionary<(string, bool), string> dicEscapeXMLValue = new Dictionary<(string, bool), string>();
        public static string EscapeXMLValue(string xmlString, bool xmlEncode)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            var tuple = (xmlString, xmlEncode);
            if (dicEscapeXMLValue.TryGetValue(tuple, out var res))
                return res;

            res = EscapeXMLValueNonCache(xmlString, xmlEncode);
            dicEscapeXMLValue[tuple] = res;
            return res;
        }

        private static string EscapeXMLValueNonCache(string xmlString, bool xmlEncode)
        {
            return xmlEncode ? xmlString
                .Replace("&", "&amp;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;")
                .Replace(">", "&gt;")
                .Replace("<", "&lt;") : xmlString;
        }

        private int Append(string text)
        {
            stringList.Add(new string('\t', nestCount));

            stringList.Add(text);

            return stringList.Count - 1;
        }

        private T? GetAttribute<T>(List<Attribute> customAttributes) where T : Attribute
        {
            T? attrribute = customAttributes.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

            return attrribute;
        }

        private void SerializeElement<T>(T obj, List<Attribute> customAttributes, string? xmlNodeName = null, XmlTagAttribute? classTagAttribute = null)
        {
            if (customAttributes.Any(x => x.GetType() == typeof(XmlIgnoreAttribute)))
                return;

            Type objType = obj.GetType();

            if (validTypes.Any(x => x == objType)) // If the type is one of the basics
            {
                XmlFormatAttribute? formatAttribute = GetAttribute<XmlFormatAttribute>(customAttributes);

                string objToString = string.Format("{0" + (formatAttribute == null ? "" : ":" + formatAttribute.Format) + "}", obj);
                bool xmlEncode = GetAttribute<XmlCDataAttribute>(customAttributes) == null;
                objToString = EscapeXMLValue(objToString, xmlEncode);

                XmlElementAttribute? elementAttribute = GetAttribute<XmlElementAttribute>(customAttributes);
                XmlTagAttribute? tagAttribute = GetAttribute<XmlTagAttribute>(customAttributes);
                string? xmlNodeRestylized = tagAttribute?.Format(xmlNodeName);
                string? xmlNodeClassRestylized = classTagAttribute?.Format(xmlNodeName);
                xmlNodeName = elementAttribute?.ElementName ?? xmlNodeRestylized ?? xmlNodeClassRestylized ?? xmlNodeName;

                string formattedNode = xmlNodeName != null
                    ? string.Format("<{0}>{1}</{0}>\n", xmlNodeName, objToString)
                    : objToString;

                Append(formattedNode);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objType)) // If the type is IEnumerable
            {
                Type underlyingType = objType.GetGenericArguments().FirstOrDefault() ?? objType.GetElementType();

                XmlArrayAttribute? arrayAttribute = GetAttribute<XmlArrayAttribute>(customAttributes);
                XmlArrayItemAttribute? arrayItemAttribute = GetAttribute<XmlArrayItemAttribute>(customAttributes);
                XmlElementAttribute? arrayElementAttribute = GetAttribute<XmlElementAttribute>(customAttributes);

                string? collectionNodeName;
                string itemNodeName;
                bool indent;

                if (arrayElementAttribute != null && (arrayAttribute != null || arrayItemAttribute != null))
                {
                    throw new XmlException("Cannot have both element attribute and array attribute/s");
                }
                else if (arrayElementAttribute != null)
                {
                    collectionNodeName = null;
                    itemNodeName = arrayElementAttribute.ElementName;
                    indent = false;
                }
                else
                {
                    collectionNodeName = arrayAttribute?.ElementName ?? xmlNodeName;
                    itemNodeName = arrayItemAttribute?.ElementName ?? underlyingType.Name;
                    indent = true;
                }

                if (indent)
                {
                    Append(string.Format("<{0}>\n", collectionNodeName));
                    nestCount++;
                }

                foreach (var item in obj as IEnumerable)
                {
                    SerializeElement(item, new List<Attribute>(), itemNodeName, classTagAttribute);
                }

                if (indent)
                {
                    nestCount--;
                    Append(string.Format("</{0}>\n", collectionNodeName));
                }
            }
            else // If the type is object
            {
                customAttributes.AddRange(GetXmlAttributes(objType));

                XmlRootAttribute? rootAttribute = GetAttribute<XmlRootAttribute>(customAttributes);
                XmlElementAttribute? elementAttribute = GetAttribute<XmlElementAttribute>(customAttributes);
                XmlTagAttribute? tagAttribute = GetAttribute<XmlTagAttribute>(customAttributes);

                xmlNodeName = elementAttribute?.ElementName ?? rootAttribute?.ElementName ?? xmlNodeName ?? objType.Name;

                var properties = GetProperties(objType);
                var fields = GetFields(objType);

                int xmlRootIndex = Append("");
                nestCount++;

                Dictionary<string, string> xmlAttributes = new Dictionary<string, string>();

                foreach (var property in properties)
                {
                    List<Attribute> propertyAttributes = GetXmlAttributes(property);

                    XmlAttributeAttribute? xmlAttribute = GetAttribute<XmlAttributeAttribute>(propertyAttributes);

                    if (xmlAttribute != null)
                    {
                        string name = xmlAttribute.AttributeName.Length != 0 ? xmlAttribute.AttributeName : property.Name;
                        xmlAttributes.Add(name, property.GetValue(obj).ToString());
                    }
                    else
                    {
                        SerializeElement(property.GetValue(obj), propertyAttributes, property.Name, tagAttribute ?? classTagAttribute);
                    }
                }

                foreach (var field in fields)
                {
                    List<Attribute> fieldAttributes = GetXmlAttributes(field);

                    XmlAttributeAttribute? xmlAttribute = GetAttribute<XmlAttributeAttribute>(fieldAttributes);

                    if (xmlAttribute != null)
                    {
                        string name = xmlAttribute.AttributeName.Length != 0 ? xmlAttribute.AttributeName : field.Name;
                        xmlAttributes.Add(name, field.GetValue(obj).ToString());
                    }
                    else
                    {
                        SerializeElement(field.GetValue(obj), fieldAttributes, field.Name, tagAttribute ?? classTagAttribute);
                    }
                }

                var xmlModeType = GetAttribute<XmlModeAttribute>(customAttributes);
                var isEnclosing = !(xmlModeType?.type == XmlModeAttribute.XmlModeType.NotEnclosing);

                var endingTagFormat = isEnclosing ? "</{0}>\n" : "";
                var beginningTagFormat = isEnclosing ? "<{0}{1}{2}>\n" : "<{0}{1}{2} />\n";

                nestCount--;
                Append(string.Format(endingTagFormat, xmlNodeName)); // this is appending the ending tag

                IEnumerable<string> wholeAttributes = xmlAttributes.Select(x => x.Key + "=\"" + x.Value + "\"");

                string xmlRoot = string.Format(beginningTagFormat, xmlNodeName, (wholeAttributes.Any() ? " " : ""), string.Join(" ", wholeAttributes));

                stringList[xmlRootIndex] = xmlRoot; // this is appending the beginning tag
            }
        }

        private Dictionary<Type, List<Attribute>> typeXmlAttributeDic = new Dictionary<Type, List<Attribute>>();
        private List<Attribute> GetXmlAttributes(Type type)
        {
            if (!typeXmlAttributeDic.TryGetValue(type, out var res))
            {
                res = type.GetCustomAttributes(true).Select(x => (Attribute)x).Where(x => x.GetType().Name.StartsWith("Xml")).ToList();
                typeXmlAttributeDic[type] = res;
            }
            return res;
        }

        private Dictionary<PropertyInfo, List<Attribute>> propXmlAttributeDic = new Dictionary<PropertyInfo, List<Attribute>>();
        private List<Attribute> GetXmlAttributes(PropertyInfo propertyInfo)
        {
            if (!propXmlAttributeDic.TryGetValue(propertyInfo, out var res))
            {
                res = propertyInfo.GetCustomAttributes(true).Select(x => (Attribute)x).Where(x => x.GetType().Name.StartsWith("Xml")).ToList();
                propXmlAttributeDic[propertyInfo] = res;
            }
            return res;
        }

        private Dictionary<FieldInfo, List<Attribute>> fieldXmlAttributeDic = new Dictionary<FieldInfo, List<Attribute>>();
        private List<Attribute> GetXmlAttributes(FieldInfo fieldInfo)
        {
            if (!fieldXmlAttributeDic.TryGetValue(fieldInfo, out var res))
            {
                res = fieldInfo.GetCustomAttributes(true).Select(x => (Attribute)x).Where(x => x.GetType().Name.StartsWith("Xml")).ToList();
                fieldXmlAttributeDic[fieldInfo] = res;
            }
            return res;
        }

        private Dictionary<Type, PropertyInfo[]> propDic = new Dictionary<Type, PropertyInfo[]>();
        private PropertyInfo[] GetProperties(Type t)
        {
            if (!propDic.TryGetValue(t, out var res))
            {
                res = t.GetProperties();
                propDic[t] = res;
            }
            return res;
        }

        private Dictionary<Type, FieldInfo[]> fieldDic = new Dictionary<Type, FieldInfo[]>();
        private FieldInfo[] GetFields(Type t)
        {
            if (!fieldDic.TryGetValue(t, out var res))
            {
                res = t.GetFields();
                fieldDic[t] = res;
            }
            return res;
        }
    }
}
