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
    /// Provides the multiplier of the property.
    /// </summary>
    /// <remarks></remarks>
    public class MultiplierAttribute : Attribute
    {
        private double value = 1.0d;

        public double Value
        {
            get => value;
        }

        public MultiplierAttribute(double Value)
        {
            value = Value;
        }

        public MultiplierAttribute()
        {
            value = Value;
        }
    }
}
