﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections;

namespace LocalWebServerMVC
{
    public class MyXmlSerializer
    {
        readonly Type[] validTypes = new Type[] { typeof(string), typeof(int), typeof(long), typeof(float),
                typeof(double), typeof(bool), typeof(byte), typeof(char), typeof(DateTime) };

        private List<string> stringList = new List<string>();
        private int nestCount = 0;

        public string Serialize<T>(T obj)
        {
            Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");

            SerializeElement(obj, new List<Attribute>(), null);

            return string.Join("", stringList);
        }

        private static string EscapeXMLValue(string xmlString)
        {
            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            return xmlString
                .Replace("&", "&amp;")
                .Replace("'", "&apos;")
                .Replace("\"", "&quot;")
                .Replace(">", "&gt;")
                .Replace("<", "&lt;");
        }

        private int Append(string text)
        {
            for (int i = 0; i < nestCount; i++)
                stringList.Add("\t");

            stringList.Add(text);

            return stringList.Count - 1;
        }

        private T GetAttribute<T>(List<Attribute> customAttributes) where T : Attribute
        {
            T attrribute = customAttributes.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

            return attrribute;
        }

        private void SerializeElement<T>(T obj, List<Attribute> customAttributes, string xmlNodeName = null)
        {
            if (customAttributes.Any(x => x.GetType() == typeof(XmlIgnoreAttribute)))
                return;

            Type objType = obj.GetType();

            if (validTypes.Any(x => x == objType)) // If the type is one of the basics
            {
                XmlFormatAttribute formatAttribute = GetAttribute<XmlFormatAttribute>(customAttributes);

                string objToString = string.Format("{0" + (formatAttribute == null ? "" : ":" + formatAttribute.Format) + "}", obj);

                XmlElementAttribute elementAttribute = GetAttribute<XmlElementAttribute>(customAttributes);

                string elementName = elementAttribute != null ? elementAttribute.ElementName : null;

                xmlNodeName = elementName ?? xmlNodeName;

                objToString = EscapeXMLValue(objToString);

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
                    collectionNodeName = arrayAttribute != null ? arrayAttribute.ElementName : xmlNodeName;
                    itemNodeName = arrayItemAttribute != null ? arrayItemAttribute.ElementName : underlyingType.Name;
                    indent = true;
                }

                if (indent)
                {
                    Append(string.Format("<{0}>\n", collectionNodeName));
                    nestCount++;
                }

                foreach (var item in obj as IEnumerable)
                {
                    SerializeElement(item, new List<Attribute>(), itemNodeName);
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

                xmlNodeName = xmlNodeName ?? objType.Name;

                XmlRootAttribute rootAttribute = GetAttribute<XmlRootAttribute>(customAttributes);

                if (rootAttribute != null)
                    xmlNodeName = rootAttribute.ElementName;

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
                        SerializeElement(property.GetValue(obj, null), propertyAttributes, property.Name);
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
                        SerializeElement(field.GetValue(obj), fieldAttributes, field.Name);
                    }
                }

                nestCount--;
                Append(string.Format("</{0}>\n", xmlNodeName));

                IEnumerable<string> wholeAttributes = xmlAttributes.Select(x => x.Key + "=\"" + x.Value + "\"");

                string xmlRoot = string.Format("<{0}{1}{2}>\n", xmlNodeName, wholeAttributes.Any() ? " " : "", string.Join(" ", wholeAttributes));

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