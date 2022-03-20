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
    /// Provides the byte length of the property.
    /// </summary>
    /// <remarks></remarks>
    public class ByteLengthAttribute : Attribute
    {
        private ushort length = 4;

        public ushort Length
        {
            get
            {
                return length;
            }
        }

        public ByteLengthAttribute()
        {
        }

        public ByteLengthAttribute(ushort value)
        {
            length = value;
        }
    }
}
