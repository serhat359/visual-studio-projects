using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocalWebServerMVC
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