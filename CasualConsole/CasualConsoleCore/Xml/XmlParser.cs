using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CasualConsoleCore.Xml;

public class XmlParser
{
    private static readonly IReadOnlySet<string> unclosedTags = new HashSet<string> { "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "source", "track", "wbr", };

    public static XmlNodeBase ParseHtml(ReadOnlySpan<char> xml)
    {
        return Parse(xml, isHtml: true);
    }

    public static XmlNodeBase Parse(ReadOnlySpan<char> xml, bool isHtml = false)
    {
        ReadOnlySpan<(string token, int lineNumber)> parts = CollectionsMarshal.AsSpan(GetParts(xml));
        if (isHtml)
        {
            if (parts.Length > 0 && parts[0].token.StartsWith("<!"))
                parts = parts[1..];
        }
        if (parts.Length > 0 && parts[0].token.StartsWith("<?xml"))
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

    private static (XmlNode, int) ReadNode(ReadOnlySpan<(string token, int lineNumber)> tokens, bool isHtml)
    {
        var isSingleTag = tokens[0].token.IsSingleTag();

        var (tagName, attributes) = GetTagAndAttributes(tokens[0].token, tokens[0].lineNumber);
        var parent = new XmlNode();
        parent.TagName = tagName;
        parent.Attributes = attributes;
        var index = 1;

        if (isHtml && unclosedTags.Contains(tagName))
        {
            if (index < tokens.Length && tokens[index].token == "</" + tagName + ">")
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

    public static List<(string token, int lineNumber)> GetParts(ReadOnlySpan<char> xml)
    {
        int i = 0;
        int lineNumber = 1;
        var list = new List<(string token, int lineNumber)>();

        static bool ContinuesWith(ReadOnlySpan<char> s, string text, int index)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != s[index + i])
                    return false;
            }
            return true;
        }
        int IndexOf(ReadOnlySpan<char> s, string smallText, int startLocation)
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
                    AddToList(list, cdataToken, lineNumberStart);
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
                    AddToList(list, token, lineNumberStart);

                    bool isScript = false;
                    bool isStyle = false;
                    if ((isScript = token.StartsWith("<script")) || (isStyle = token.StartsWith("<style")))
                    {
                        lineNumberStart = lineNumber;
                        var scriptEndTag = isScript ? "</script>"
                            : isStyle ? "</style>"
                            : throw new Exception();
                        var scriptEnd = IndexOf(xml, scriptEndTag, i);
                        if (scriptEnd < 0)
                            throw new Exception();
                        var scriptContent = xml[i..scriptEnd];
                        AddToList(list, scriptContent, lineNumberStart);
                        AddToList(list, scriptEndTag, lineNumber);
                        i = scriptEnd + scriptEndTag.Length;
                    }
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
                AddToList(list, token, lineNumberStart);
            }
        }

        return list;
    }

    private static void AddToList(List<(string token, int lineNumber)> list, ReadOnlySpan<char> token, int lineNumber)
    {
        if (!MemoryExtensions.IsWhiteSpace(token))
            list.Add((token.ToString(), lineNumber));
    }
    private static void AddToList(List<(string token, int lineNumber)> list, string token, int lineNumber)
    {
        if (!MemoryExtensions.IsWhiteSpace(token))
            list.Add((token, lineNumber));
    }

    private static (string, NameValueCollection) GetTagAndAttributes(string s, int line)
    {
        var attributes = new NameValueCollection();
        int i = 1;
        while (!IsWhiteSpace(s[i]) && s[i] != '>')
            i++;

        var tagName = s[1..i];

        while (true)
        {
            while (IsWhiteSpace(s[i]))
                i++;
            if (s[i] == '>' || s[i] == '/')
                return (tagName, attributes);
            int start = i;
            while (s[i] != '=' && s[i] != ' ' && s[i] != '>')
                i++;
            var attrName = s[start..i];
            string? attrValue = null;
            while (IsWhiteSpace(s[i]))
                i++;
            if (s[i] == '=')
            {
                i++;
                while (IsWhiteSpace(s[i]))
                    i++;
                char c = s[i];
                char startCharacter = c switch
                {
                    '"' => '"',
                    '\'' => '\'',
                    _ => throw new Exception($"unexpected characted: {c} at line: {line}"),
                };

                var attrValueStart = i += 1;
                while (s[i] != startCharacter)
                    i++;
                attrValue = s[attrValueStart..i];
                attrValue = NormalizeXml(attrValue);
                i++;
            }
            attributes[attrName] = attrValue;
        }
    }

    private static bool IsWhiteSpace(char c)
    {
        return c switch
        {
            ' ' => true,
            '\r' => true,
            '\n' => true,
            _ => false,
        };
    }

    private static readonly Regex htmlEncodedRegex = new Regex(@"&[0-9a-zA-Z]+;", RegexOptions.Compiled);
    private static readonly Regex htmlEncodedRegexInt = new Regex(@"&#([0-9]+);", RegexOptions.Compiled);
    private static readonly Regex htmlEncodedRegexHexInt = new Regex(@"&#x([0-9]+);", RegexOptions.Compiled);
    public static string NormalizeXml(string s)
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
    private readonly List<object> children = new();
    private readonly List<XmlNode> childNodes = new();

    public IReadOnlyList<object> Children => children;
    public IReadOnlyList<XmlNode> ChildNodes => childNodes;
    public string InnerText
    {
        get
        {
            var stringBuilder = new StringBuilder();
            WriteInnerText(stringBuilder);
            return stringBuilder.ToString().Trim();
        }
        set
        {
            children.Clear();
            childNodes.Clear();
            children.Add(value);
        }
    }

    private void WriteInnerText(StringBuilder stringBuilder)
    {
        foreach (var item in Children)
        {
            if (item is string s)
                stringBuilder.Append(s);
            else if (item is XmlNode node)
                node.WriteInnerText(stringBuilder);
            else
                throw new Exception();
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

public class XmlNode : XmlNodeBase
{
    private string? tagName;
    private NameValueCollection? attributes;

    public string TagName { get { return tagName ?? throw new Exception(); } set { tagName = value; } }
    public NameValueCollection Attributes { get { return attributes ?? throw new Exception(); } set { attributes = value; } }

    public XmlNode this[string key] => this.ChildNodes.Single(x => x.tagName == key);

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