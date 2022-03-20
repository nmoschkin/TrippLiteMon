using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using static TrippLite.TrippLiteCodeUtility;

namespace TrippLite
{
    /// <summary>
    /// Provides the number format for the property.
    /// </summary>
    /// <remarks></remarks>
    public class NumberFormatAttribute : Attribute
    {
        private string format;

        public string Format
        {
            get
            {
                return format;
            }
        }

        public NumberFormatAttribute(string format)
        {
            this.format = format;
        }

        public NumberFormatAttribute()
        {
            format = "0.0";
        }
    }
}
