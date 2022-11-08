using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CasualConsoleCore.XmlParser
{
    public class XmlParser
    {
        public static MyXmlNode Parse(string xml)
        {
            ArraySegment<string> parts = GetParts(xml).ToArray();

            var topDocument = new MyXmlNode();

            int index = 0;
            while (true)
            {
                var (node, endIndex) = ReadNode(parts[index..]);
                topDocument.ChildNodes.Add(node);

                if (endIndex < 0)
                    throw new Exception();
                if (endIndex == parts.Count)
                    break;
            }

            return topDocument;
        }

        private static (MyXmlNode, int) ReadNode(ArraySegment<string> tokens)
        {
            if (!tokens[0].IsBeginTag()) throw new Exception(); // TODO remove later

            var parent = new MyXmlNode();

            var (tagName, attributes) = tokens[0].GetTagAndAttributes();
            parent.TagName = tagName;
            parent.Attributes = attributes;
            var index = 1;

            while (tokens[index].IsBeginTag())
            {
                var (node, endIndex) = ReadNode(tokens[index..]);
                index += endIndex;
                parent.ChildNodes.Add(node);
            }
            if (!tokens[index].IsTag())
            {
                var text = tokens[index];
                parent.InnerText = text;
                index++;
            }
            if (tokens[index] != "</" + tagName + ">")
                throw new Exception();

            return (parent, index + 1);
        }

        private static IEnumerable<string> GetParts(string xml)
        {
            int i = 0;
            while (true)
            {
                while (i < xml.Length && char.IsWhiteSpace(xml[i]))
                    i++;
                if (i == xml.Length)
                    break;

                if (xml[i] == '<')
                {
                    int start = i;
                    while (xml[i] != '>')
                        i++;

                    i++;
                    var token = xml[start..i];
                    yield return token;
                }
                else
                {
                    int start = i;
                    while (xml[i] != '<')
                        i++;
                    var token = xml[start..i];
                    yield return token;
                }
            }
        }
    }

    public class MyXmlNode
    {
        public string InnerText { get; set; }
        public string TagName { get; set; }
        public NameValueCollection Attributes { get; set; }
        public List<MyXmlNode> ChildNodes { get; set; } = new List<MyXmlNode>();
    }

    public static class Extensions
    {
        public static bool IsTag(this string s)
        {
            return s[0] == '<';
        }

        public static bool IsBeginTag(this string s)
        {
            return s[0] == '<' && s[1] != '/';
        }

        public static bool IsEndTag(this string s)
        {
            return s[0] == '<' && s[1] == '/';
        }

        public static (string, NameValueCollection) GetTagAndAttributes(this string s)
        {
            var attributes = new NameValueCollection();
            int i = 1;
            while (s[i] != ' ' && s[i] != '>')
                i++;

            var tagName = s[1..i];

            while (true)
            {
                if (s[i] == ' ')
                    i++;
                if (s[i] == '>')
                    return (tagName, attributes);
                int start = i;
                while (s[i] != '=' && s[i] != ' ' && s[i] != '>')
                    i++;
                var attrName = s[start..i];
                var attrValue = "";
                if (s[i] == '=')
                {
                    if (s[i + 1] != '"')
                        throw new Exception();
                    var attrValueStart = i += 2;
                    while (s[i] != '"')
                        i++;
                    attrValue = s[attrValueStart..i];
                    i++;
                }
                attributes[attrName] = attrValue;
            }
        }
    }
}
