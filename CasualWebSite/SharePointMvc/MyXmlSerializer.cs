using Model.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace SharePointMvc
{
    public class MyXmlSerializer
    {
        private readonly Type[] validTypes = new Type[] { typeof(string), typeof(int), typeof(long), typeof(float),
                typeof(double), typeof(bool), typeof(byte), typeof(char), typeof(DateTime) };

        private List<string> stringList = new List<string>();
        private int nestCount = 0;

        public string Serialize<T>(T obj)
        {
            Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");

            SerializeElement(obj, new List<Attribute>(), xmlNodeName: null);

            return string.Concat(stringList);
        }

        public static string EscapeXMLValue(string xmlString, bool xmlEncode)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

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

        private T GetAttribute<T>(List<Attribute> customAttributes) where T : Attribute
        {
            T attrribute = customAttributes.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

            return attrribute;
        }

        private void SerializeElement<T>(T obj, List<Attribute> customAttributes, string xmlNodeName = null, XmlTagAttribute classTagAttribute = null)
        {
            if (customAttributes.Any(x => x.GetType() == typeof(XmlIgnoreAttribute)))
                return;

            Type objType = obj.GetType();

            if (validTypes.Any(x => x == objType)) // If the type is one of the basics
            {
                XmlFormatAttribute formatAttribute = GetAttribute<XmlFormatAttribute>(customAttributes);

                string objToString = string.Format("{0" + (formatAttribute == null ? "" : ":" + formatAttribute.Format) + "}", obj);
                bool xmlEncode = GetAttribute<XmlCDataAttribute>(customAttributes) == null;
                objToString = EscapeXMLValue(objToString, xmlEncode);

                XmlElementAttribute elementAttribute = GetAttribute<XmlElementAttribute>(customAttributes);
                XmlTagAttribute tagAttribute = GetAttribute<XmlTagAttribute>(customAttributes);
                string xmlNodeRestylized = tagAttribute?.Format(xmlNodeName);
                string xmlNodeClassRestylized = classTagAttribute?.Format(xmlNodeName);
                xmlNodeName = elementAttribute?.ElementName ?? xmlNodeRestylized ?? xmlNodeClassRestylized ?? xmlNodeName;

                string formattedNode = xmlNodeName != null
                    ? string.Format("<{0}>{1}</{0}>\n", xmlNodeName, objToString)
                    : objToString;

                Append(formattedNode);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(objType)) // If the type is IEnumerable
            {
                Type underlyingType = objType.GetGenericArguments().FirstOrDefault() ?? objType.GetElementType();

                XmlArrayAttribute arrayAttribute = GetAttribute<XmlArrayAttribute>(customAttributes);
                XmlArrayItemAttribute arrayItemAttribute = GetAttribute<XmlArrayItemAttribute>(customAttributes);
                XmlElementAttribute arrayElementAttribute = GetAttribute<XmlElementAttribute>(customAttributes);

                string collectionNodeName;
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

                XmlRootAttribute rootAttribute = GetAttribute<XmlRootAttribute>(customAttributes);
                XmlElementAttribute elementAttribute = GetAttribute<XmlElementAttribute>(customAttributes);
                XmlTagAttribute tagAttribute = GetAttribute<XmlTagAttribute>(customAttributes);

                xmlNodeName = elementAttribute?.ElementName ?? rootAttribute?.ElementName ?? xmlNodeName ?? objType.Name;

                var properties = objType.GetProperties();
                var fields = objType.GetFields();

                int xmlRootIndex = Append("");
                nestCount++;

                Dictionary<string, string> xmlAttributes = new Dictionary<string, string>();

                foreach (var property in properties)
                {
                    List<Attribute> propertyAttributes = GetXmlAttributes(property);

                    XmlAttributeAttribute xmlAttribute = GetAttribute<XmlAttributeAttribute>(propertyAttributes);

                    if (xmlAttribute != null)
                    {
                        string name = xmlAttribute.AttributeName.Length != 0 ? xmlAttribute.AttributeName : property.Name;
                        xmlAttributes.Add(name, property.GetValue(obj, null).ToString());
                    }
                    else
                    {
                        SerializeElement(property.GetValue(obj, null), propertyAttributes, property.Name, tagAttribute ?? classTagAttribute);
                    }
                }

                foreach (var field in fields)
                {
                    List<Attribute> fieldAttributes = GetXmlAttributes(field);

                    XmlAttributeAttribute xmlAttribute = GetAttribute<XmlAttributeAttribute>(fieldAttributes);

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

                nestCount--;
                Append(string.Format("</{0}>\n", xmlNodeName));

                IEnumerable<string> wholeAttributes = xmlAttributes.Select(x => x.Key + "=\"" + x.Value + "\"");

                string xmlRoot = string.Format("<{0}{1}{2}>\n", xmlNodeName, (wholeAttributes.Any() ? " " : ""), string.Join(" ", wholeAttributes));

                stringList[xmlRootIndex] = xmlRoot;
            }
        }

        private List<Attribute> GetXmlAttributes(Type type)
        {
            return type.GetCustomAttributes(true).Select(x => (Attribute)x).Where(x => x.GetType().Name.StartsWith("Xml")).ToList();
        }

        private List<Attribute> GetXmlAttributes(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(true).Select(x => (Attribute)x).Where(x => x.GetType().Name.StartsWith("Xml")).ToList();
        }

        private List<Attribute> GetXmlAttributes(FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttributes(true).Select(x => (Attribute)x).Where(x => x.GetType().Name.StartsWith("Xml")).ToList();
        }
    }

}