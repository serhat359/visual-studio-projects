using System;

namespace MVCCore.Models.Attributes
{
    public class XmlFormatAttribute : Attribute
    {
        public string Format { get; private set; }

        public XmlFormatAttribute(string format)
        {
            this.Format = format;
        }
    }
}