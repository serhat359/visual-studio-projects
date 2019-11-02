using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Web
{
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
}
