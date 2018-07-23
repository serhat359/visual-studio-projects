using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleSsmsEcosystemAddin
{
    public static class Extensions
    {
        public static object Prop(this object o, string propertyName)
        {
            return o.GetType().GetProperty(propertyName).GetValue(o, null);
        }
    }
}
