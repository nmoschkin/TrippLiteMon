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
    /// Indicates a measure unit type.
    /// </summary>
    /// <remarks></remarks>
    public enum MeasureUnitTypes
    {
        [Description("Volts")]
        [UnitSymbol("V")]
        Volt,
        [Description("Amps")]
        [UnitSymbol("A")]
        Amp,
        [Description("Watts")]
        [UnitSymbol("W")]
        Watt,
        [Description("Hertz")]
        [UnitSymbol("Hz")]
        Hertz,
        [Description("Percent")]
        [UnitSymbol("%")]
        Percent,
        [Description("Minutes")]
        Time,
        [Description("Degrees Celsius")]
        [UnitSymbol("°C")]
        Temperature
    }
}
