namespace MVCCore.Models.Attributes;

using System;

public class XmlTagAttribute : Attribute
{
    public enum XmlTagType
    {
        UpperCase, // EXAMPLESTRING
        LowerCase, // examplestring
        PascalCase, // ExampleString
        CamelCase, // exampleString
    }

    public XmlTagType Type { get; private set; }

    public XmlTagAttribute(XmlTagType type)
    {
        this.Type = type;
    }

    public string Format(string oldTag)
    {
        char firstChar;

        switch (Type)
        {
            case XmlTagType.PascalCase:
                firstChar = oldTag[0];
                firstChar = Char.ToUpperInvariant(firstChar);
                return firstChar + oldTag.Substring(1);
            case XmlTagType.CamelCase:
                firstChar = oldTag[0];
                firstChar = Char.ToLowerInvariant(firstChar);
                return firstChar + oldTag.Substring(1);
            case XmlTagType.UpperCase:
                return oldTag.ToUpper();
            case XmlTagType.LowerCase:
                return oldTag.ToLower();
            default:
                throw new Exception("This xml tag type is not recognized");
        }
    }
}
