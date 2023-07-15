using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CasualConsoleCore.XmlParser;

public class XmlParser
{
    private static readonly IReadOnlySet<string> unclosedTags = new HashSet<string> { "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "source", "track", "wbr", };

    public static XmlRoot ParseHtml(string xml)
    {
        return Parse(xml, isHtml: true);
    }

    public static XmlRoot Parse(string xml, bool isHtml = false)
    {
        var partsEnumerable = GetParts(xml);
        if (isHtml)
        {
            if (partsEnumerable.FirstOrDefault().token.StartsWith("<!"))
                partsEnumerable = partsEnumerable.Skip(1);
        }
        ArraySegment<(string, int)> parts = partsEnumerable.ToArray();

        var topDocument = new XmlRoot();

        if (parts.Count > 0)
        {
            int index = 0;
            while (true)
            {
                var (node, endIndex) = ReadNode(parts[index..], isHtml);
                topDocument.ChildNodes.Add(node);

                if (endIndex < 0)
                    throw new Exception();
                if (endIndex + index == parts.Count)
                    break;

                index += endIndex;
            }
        }

        return topDocument;
    }

    private static (XmlNode, int) ReadNode(ArraySegment<(string token, int lineNumber)> tokens, bool isHtml)
    {
        if (!tokens[0].token.IsBeginTag()) throw new Exception(); // TODO remove later

        var isSingleTag = tokens[0].token.IsSingleTag();

        var (tagName, attributes) = GetTagAndAttributes(tokens[0].token);
        var parent = new XmlNode();
        parent.TagName = tagName;
        parent.Attributes = attributes;
        var index = 1;

        if (isHtml && unclosedTags.Contains(tagName))
            return (parent, index);

        if (isSingleTag)
        {
            return (parent, index);
        }

        while (tokens[index].token.IsBeginTag())
        {
            var (node, endIndex) = ReadNode(tokens[index..], isHtml);
            index += endIndex;
            parent.ChildNodes.Add(node);
        }
        if (!tokens[index].token.IsTag())
        {
            var text = tokens[index];
            parent.InnerText = NormalizeXml(text.token);
            index++;
        }
        while (tokens[index].token.IsBeginTag())
        {
            var (node, endIndex) = ReadNode(tokens[index..], isHtml);
            index += endIndex;
            parent.ChildNodes.Add(node);
        }
        if (tokens[index].token != "</" + tagName + ">")
        {
            throw new Exception($"Error on line: {tokens[index].lineNumber}. Expected '{"</" + tagName + ">"}' but encountered '{tokens[index].token}'");
        }

        return (parent, index + 1);
    }

    private static IEnumerable<(string token, int lineNumber)> GetParts(string xml)
    {
        int i = 0;
        int lineNumber = 1;

        bool IsWhiteSpace(char c)
        {
            if (c == '\n') lineNumber++;
            return char.IsWhiteSpace(c);
        }
        static bool IsWhiteSpaceNoIncrement(char c)
        {
            return char.IsWhiteSpace(c);
        }
        static bool ContinuesWith(string s, string text, int index)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != s[index + i])
                    return false;
            }
            return true;
        }
        int IndexOf(string s, string smallText, int startLocation)
        {
            for (int i = startLocation; i <= s.Length - smallText.Length; i++)
            {
                if (s[i] == '\n') lineNumber++;
                for (int j = 0; j < smallText.Length; j++)
                {
                    if (smallText[j] != s[i + j])
                        goto notfound;
                }
                return i;
            notfound:;
            }
            return -1;
        }

        while (true)
        {
            while (i < xml.Length && IsWhiteSpace(xml[i]))
                i++;
            if (i == xml.Length)
                break;

            if (xml[i] == '<')
            {
                int lineNumberStart = lineNumber;
                if (xml[i + 1] == '!' && xml[i + 2] == '-' && xml[i + 3] == '-')
                {
                    var commentEndIndex = IndexOf(xml, "-->", i + 3);
                    if (commentEndIndex < 0)
                        throw new Exception();
                    i = commentEndIndex + 3;
                    continue;
                }

                if (xml[i + 1] == '!' && xml[i + 2] == '[')
                {
                    var lookupIndex = i + 3;
                    if (!ContinuesWith(xml, "CDATA[", lookupIndex))
                        throw new Exception();

                    var cdataStartIndex = lookupIndex + "CDATA[".Length;
                    var cdataEndIndex = IndexOf(xml, "]]>", cdataStartIndex);
                    if (cdataEndIndex < 0)
                        throw new Exception();

                    i = cdataEndIndex + 3;
                    var cdataToken = xml[(lookupIndex - 3)..i];
                    yield return (cdataToken, lineNumberStart);
                }
                else
                {
                    int start = i;
                    while (true)
                    {
                        if (xml[i] == '\n') lineNumber++;
                        if (xml[i] == '>')
                            break;
                        i++;
                    }

                    if (xml[i] == '\n') lineNumber++;
                    i++;
                    var token = xml[start..i];
                    yield return (token, lineNumberStart);
                }
            }
            else
            {
                int lineNumberStart = lineNumber;
                int start = i;
                while (true)
                {
                    if (xml[i] == '\n') lineNumber++;
                    if (xml[i] == '<')
                        break;
                    i++;
                }

                var end = i;
                while (IsWhiteSpaceNoIncrement(xml[end - 1]))
                    end--;

                var token = xml[start..end];
                yield return (token, lineNumberStart);
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
                char startCharacter = s[i + 1] switch
                {
                    '"' => '"',
                    '\'' => '\'',
                    _ => throw new Exception(),
                };

                var attrValueStart = i += 2;
                while (s[i] != startCharacter)
                    i++;
                attrValue = s[attrValueStart..i];
                attrValue = NormalizeXml(attrValue);
                i++;
            }
            attributes[attrName] = attrValue;
        }
    }

    private static readonly Regex htmlEncodedRegex = new Regex(@"&[0-9a-zA-Z]+;", RegexOptions.Compiled);
    private static readonly Regex htmlEncodedRegexInt = new Regex(@"&#([0-9]+);", RegexOptions.Compiled);
    private static readonly Regex htmlEncodedRegexHexInt = new Regex(@"&#x([0-9]+);", RegexOptions.Compiled);
    private static string NormalizeXml(string s)
    {
        if (s.Length >= 2 && s[0] == '<' && s[1] == '!')
        {
            return s[9..^3];
        }

        s = htmlEncodedRegex.Replace(s, m => HttpUtility.HtmlDecode(m.Value));
        s = htmlEncodedRegexInt.Replace(s, m => ((char)int.Parse(m.Groups[1].Value)).ToString());
        s = htmlEncodedRegexHexInt.Replace(s, m => ((char)Convert.ToUInt32(m.Groups[1].Value, 16)).ToString());
        return s;
    }
}

public class XmlRoot
{
    public List<XmlNode> ChildNodes { get; set; } = new List<XmlNode>();

    public string Beautify(string indentChars = "  ", string newLineChars = "\r\n")
    {
        var sb = new StringBuilder();

        foreach (var item in ChildNodes)
        {
            item.Beautify(sb, indentChars, newLineChars);
        }

        return sb.ToString();
    }
}

public class XmlNode
{
    private string? tagName;
    private NameValueCollection? attributes;

    public string InnerText { get; set; } = "";
    public string TagName { get { return tagName ?? throw new Exception(); } set { tagName = value; } }
    public NameValueCollection Attributes { get { return attributes ?? throw new Exception(); } set { attributes = value; } }
    public List<XmlNode> ChildNodes { get; set; } = new List<XmlNode>();

    public XmlNode()
    {
    }

    public XmlNode(string tagName, NameValueCollection attributes)
    {
        this.tagName = tagName;
        this.attributes = attributes;
    }

    public void AppendChild(XmlNode node)
    {
        ChildNodes.Add(node);
    }

    public void Beautify(StringBuilder sb, string indentChars = "  ", string newLineChars = "\r\n")
    {
        var node = this;

        void Write(XmlNode node, int level)
        {
            for (int i = 0; i < level; i++)
                sb.Append(indentChars);

            sb.Append('<');
            sb.Append(node.tagName);
            foreach (string key in node.Attributes)
            {
                sb.Append(' ');
                sb.Append(key);
                var value = node.Attributes[key];
                if (value != null)
                {
                    sb.Append('=');
                    sb.Append('\"');
                    sb.Append(HttpUtility.HtmlAttributeEncode(value));
                    sb.Append('\"');
                }
            }
            sb.Append('>');

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
            sb.Append('>');
        }

        Write(node, 0);
    }

    public string Beautify(string indentChars = "  ", string newLineChars = "\r\n")
    {
        var sb = new StringBuilder();

        Beautify(sb, indentChars, newLineChars);

        return sb.ToString();
    }
}

public static class XmlParserExtensions
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

public static class XmlNodeExtensions
{
    public static IEnumerable<XmlNode> AllInnerNodes(this XmlRoot root)
    {
        foreach (var item in root.ChildNodes)
        {
            yield return item;
            foreach (var node in item.AllInnerNodes())
                yield return node;
        }
    }

    public static IEnumerable<XmlNode> AllInnerNodes(this XmlNode xmlNode)
    {
        foreach (var item in xmlNode.ChildNodes)
        {
            yield return item;
            foreach (var node in item.AllInnerNodes())
                yield return node;
        }
    }
}