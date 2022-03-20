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
    /// Provides the unit of measure of the property.
    /// </summary>
    /// <remarks></remarks>
    public class MeasureUnitAttribute : Attribute
    {
        private MeasureUnitTypes unit;

        public MeasureUnitTypes Unit
        {
            get => unit;
        }

        public MeasureUnitAttribute(MeasureUnitTypes Unit)
        {
            unit = Unit;
        }
    }
}
