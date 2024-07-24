using System;

namespace MVCCore.Models.Attributes;

public class XmlModeAttribute : Attribute
{
    public enum XmlModeType
    {
        Enclosing,
        NotEnclosing
    }

    public XmlModeType type;

    public XmlModeAttribute(XmlModeType type)
    {
        this.type = type;
    }
}
