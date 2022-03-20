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
    /// Provides the unit symbol of the property.
    /// </summary>
    /// <remarks></remarks>
    public class UnitSymbolAttribute : Attribute
    {
        private string symbol;

        public string Symbol
        {
            get
            {
                return symbol;
            }
        }

        public UnitSymbolAttribute(string symbol)
        {
            this.symbol = symbol;
        }
    }
}
