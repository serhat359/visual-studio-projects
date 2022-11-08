using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

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

            var isSingleTag = tokens[0].IsSingleTag();

            var parent = new MyXmlNode();

            var (tagName, attributes) = GetTagAndAttributes(tokens[0]);
            parent.TagName = tagName;
            parent.Attributes = attributes;
            var index = 1;

            if (!isSingleTag)
            {
                while (tokens[index].IsBeginTag())
                {
                    var (node, endIndex) = ReadNode(tokens[index..]);
                    index += endIndex;
                    parent.ChildNodes.Add(node);
                }
                if (!tokens[index].IsTag())
                {
                    var text = tokens[index];
                    parent.InnerText = NormalizeXml(text);
                    index++;
                }
                while (tokens[index].IsBeginTag())
                {
                    var (node, endIndex) = ReadNode(tokens[index..]);
                    index += endIndex;
                    parent.ChildNodes.Add(node);
                }
                if (tokens[index] != "</" + tagName + ">")
                    throw new Exception();
            }

            return (parent, isSingleTag ? index : index + 1);
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
                    if (xml[i + 1] == '!' && xml[i + 2] == '[')
                    {
                        var lookupIndex = i + 3;
                        var cdataIndex = xml.IndexOf("CDATA[", lookupIndex);
                        if (cdataIndex != lookupIndex)
                            throw new Exception();

                        var cdataStartIndex = cdataIndex + "CDATA[".Length;
                        var cdataEndIndex = xml.IndexOf("]]>", cdataStartIndex);
                        if (cdataEndIndex < 0)
                            throw new Exception();

                        i = cdataEndIndex + 3;
                        var cdataToken = xml[(lookupIndex - 3)..i];
                        yield return cdataToken;
                    }
                    else
                    {
                        int start = i;
                        while (xml[i] != '>')
                            i++;

                        i++;
                        var token = xml[start..i];
                        yield return token;
                    }
                }
                else
                {
                    int start = i;
                    while (xml[i] != '<')
                        i++;

                    var end = i;
                    while (char.IsWhiteSpace(xml[end - 1]))
                        end--;

                    var token = xml[start..end];
                    yield return token;
                }
            }
        }

        private static (string, NameValueCollection) GetTagAndAttributes(string s)
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
                if (s[i] == '>' || s[i] == '/')
                    return (tagName, attributes);
                int start = i;
                while (s[i] != '=' && s[i] != ' ' && s[i] != '>')
                    i++;
                var attrName = s[start..i];
                string? attrValue = null;
                if (s[i] == '=')
                {
                    if (s[i + 1] != '"')
                        throw new Exception();
                    var attrValueStart = i += 2;
                    while (s[i] != '"')
                        i++;
                    attrValue = s[attrValueStart..i];
                    attrValue = NormalizeXml(attrValue);
                    i++;
                }
                attributes[attrName] = attrValue;
            }
        }

        private static readonly Regex htmlEncodedRegex = new Regex(@"&[0-9a-zA-Z]{3,7};", RegexOptions.Compiled);
        private static string NormalizeXml(string s)
        {
            if (s.Length >= 2 && s[0] == '<' && s[1] == '!')
            {
                return s[9..^3];
            }

            return htmlEncodedRegex.Replace(s, m =>
            {
                var unicode = HttpUtility.HtmlDecode(m.Value);
                return unicode;
            });
        }
    }

    public class MyXmlNode
    {
        private string? tagName;

        public string InnerText { get; set; } = "";
        public string TagName { get { return tagName ?? throw new Exception(); } set { tagName = value; } }
        public NameValueCollection Attributes { get; set; }
        public List<MyXmlNode> ChildNodes { get; set; } = new List<MyXmlNode>();
        public bool IsRoot => tagName == null;

        public void AppendChild(MyXmlNode node)
        {
            ChildNodes.Add(node);
        }

        public string Beautify(string indentChars = "  ", string newLineChars = "\r\n")
        {
            var node = this;
            if (node.IsRoot)
                node = node.ChildNodes[0];

            var sb = new StringBuilder();

            void Write(MyXmlNode node, int level)
            {
                for (int i = 0; i < level; i++)
                    sb.Append(indentChars);

                sb.Append("<");
                sb.Append(node.tagName);
                foreach (string key in node.Attributes)
                {
                    sb.Append(" ");
                    sb.Append(key);
                    var value = node.Attributes[key];
                    if (value != null)
                    {
                        sb.Append("=");
                        sb.Append("\"");
                        sb.Append(HttpUtility.HtmlAttributeEncode(value));
                        sb.Append("\"");
                    }
                }
                sb.Append(">");

                sb.Append(HttpUtility.HtmlEncode(node.InnerText));

                foreach (var item in node.ChildNodes)
                {
                    sb.Append(newLineChars);
                    Write(item, level + 1);
                }
                if (node.ChildNodes.Any())
                    sb.Append(newLineChars);

                if (node.ChildNodes.Any())
                    for (int i = 0; i < level; i++)
                        sb.Append(indentChars);
                sb.Append("</");
                sb.Append(node.tagName);
                sb.Append(">");
            }

            Write(node, 0);

            return sb.ToString();
        }
    }

    public static class Extensions
    {
        public static bool IsTag(this string s)
        {
            return s[0] == '<' && s[1] != '!';
        }

        public static bool IsBeginTag(this string s)
        {
            return s[0] == '<' && s[1] != '/' && s[1] != '!';
        }

        public static bool IsEndTag(this string s)
        {
            return s[0] == '<' && s[1] == '/';
        }

        public static bool IsSingleTag(this string s)
        {
            return s[^1] == '>' && s[^2] == '/';
        }
    }
}
