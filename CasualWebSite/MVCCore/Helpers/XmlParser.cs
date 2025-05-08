using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MVCCore.Helpers;

public class XmlParser
{
    private static readonly IReadOnlySet<string> unclosedTags = new HashSet<string> { "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "source", "track", "wbr", };

    public static XmlNodeBase ParseHtml(string xml)
    {
        return Parse(xml, isHtml: true);
    }

    public static XmlNodeBase Parse(string xml, bool isHtml = false)
    {
        var partsEnumerable = GetParts(xml).Where(x => !string.IsNullOrWhiteSpace(x.token));
        if (isHtml)
        {
            if (partsEnumerable.FirstOrDefault().token.StartsWith("<!"))
                partsEnumerable = partsEnumerable.Skip(1);
        }
        (string, int)[] parts = partsEnumerable.ToArray();
        if (parts.Length > 0 && parts[0].Item1.StartsWith("<?xml"))
            parts = parts[1..];

        var topDocument = new XmlNodeBase();

        if (parts.Length > 0)
        {
            int index = 0;
            while (true)
            {
                var (node, endIndex) = ReadNode(parts[index..], isHtml);
                topDocument.AddXmlNode(node);

                if (endIndex < 0)
                    throw new Exception();
                if (endIndex + index == parts.Length)
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
        {
            if (index < tokens.Count && tokens[index].token == "</" + tagName + ">")
                index++;
            return (parent, index);
        }

        if (isSingleTag)
        {
            return (parent, index);
        }

        while (true)
        {
            if (tokens[index].token.IsBeginTag())
            {
                var (node, endIndex) = ReadNode(tokens[index..], isHtml);
                index += endIndex;
                parent.AddXmlNode(node);
                continue;
            }
            if (!tokens[index].token.IsTag())
            {
                var text = tokens[index];
                parent.AddTextNode(NormalizeXml(text.token));
                index++;
                continue;
            }
            break;
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
            if (i < xml.Length && xml[i] == '\n') lineNumber++;
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
                while (i < xml.Length)
                {
                    if (xml[i] == '\n') lineNumber++;
                    if (xml[i] == '<')
                        break;
                    i++;
                }

                var end = i;

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
                char c = s[i + 1];
                char startCharacter = c switch
                {
                    '"' => '"',
                    '\'' => '\'',
                    _ => throw new Exception($"unexpected characted: {c}"),
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

public class XmlNodeBase
{
    private readonly List<object> children = new List<object>();
    private readonly List<XmlNode> childNodes = new List<XmlNode>();
    public string xmlHeader = "";

    public IReadOnlyList<object> Children => children;
    public IReadOnlyList<XmlNode> ChildNodes => childNodes;
    public string InnerText
    {
        get
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in Children)
            {
                if (item is string s)
                    stringBuilder.Append(s);
                else if (item is XmlNode node)
                    stringBuilder.Append(node.InnerText);
                else
                    throw new Exception();
            }
            return stringBuilder.ToString().Trim();
        }
        set
        {
            children.Clear();
            childNodes.Clear();
            children.Add(value);
        }
    }

    public void AddXmlNode(XmlNode node)
    {
        children.Add(node);
        childNodes.Add(node);
    }

    public void AddTextNode(string text)
    {
        children.Add(text);
    }

    public void ClearChildren()
    {
        children.Clear();
        childNodes.Clear();
    }

    public string Beautify(string indentChars = "  ", string newLineChars = "\r\n")
    {
        var sb = new StringBuilder();
        sb.Append(xmlHeader);

        foreach (var item in ChildNodes)
        {
            item.Beautify(sb, indentChars, newLineChars);
        }

        return sb.ToString();
    }
}

public class XmlNode : XmlNodeBase
{
    private string? tagName;
    private NameValueCollection? attributes;

    public string TagName { get { return tagName ?? throw new Exception(); } set { tagName = value; } }
    public NameValueCollection Attributes { get { return attributes ?? throw new Exception(); } set { attributes = value; } }

    public XmlNode()
    {
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

            bool isSingleAndString = node.Children.Count == 1 && node.ChildNodes.Count == 0;
            foreach (var item in node.Children)
            {
                if (!isSingleAndString)
                    sb.Append(newLineChars);
                if (item is string s)
                    sb.Append(HttpUtility.HtmlEncode(s));
                else if (item is XmlNode n)
                    Write(n, level + 1);
                else
                    throw new Exception();
            }

            if (!isSingleAndString)
            {
                sb.Append(newLineChars);
                for (int i = 0; i < level; i++)
                    sb.Append(indentChars);
            }
            sb.Append("</");
            sb.Append(node.tagName);
            sb.Append('>');
        }

        Write(node, 0);
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
    public static IEnumerable<XmlNode> AllInnerNodes(this XmlNodeBase root)
    {
        foreach (var item in root.ChildNodes)
        {
            yield return item;
            foreach (var node in item.AllInnerNodes())
                yield return node;
        }
    }
}
