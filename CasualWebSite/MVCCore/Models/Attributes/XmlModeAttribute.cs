using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCCore.Models.Attributes
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
