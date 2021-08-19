using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace TrippLite
{

    #region Utility Functions

    static class TrippLiteCodeUtility
    {

        /// <summary>
    /// Gets the specified property of the specified attribute type for the specified enumeration member.
    /// </summary>
    /// <typeparam name="T">The attribute type from which to retrieve the property.</typeparam>
    /// <typeparam name="U">The type of the attribute property to retrieve.</typeparam>
    /// <typeparam name="V">The enum type.</typeparam>
    /// <param name="o">The enum value for which to retrieve the attribute.</param>
    /// <param name="valueName">The name of the property inside the Attribute object to retrieve.</param>
    /// <param name="b">Optional binding flags for member searching.</param>
    /// <returns>A value of type U that represents the value of the property of the attribute of the member of the enumeration.</returns>
    /// <remarks></remarks>
        public static U GetEnumAttrVal<T, U, V>(V o, string valueName, BindingFlags b = BindingFlags.Public | BindingFlags.Static) where T : Attribute
        {
            T da;
            var fi = o.GetType().GetFields(b);
            foreach (var fe in fi)
            {
                if ((o.ToString() ?? "") == (fe.GetValue(o).ToString() ?? ""))
                {
                    da = (T)fe.GetCustomAttribute(typeof(T));
                    if (da is object)
                    {
                        var f = da.GetType().GetProperty(valueName);
                        return (U)f.GetValue(da);
                    }
                }
            }

            return default;
        }

        public static List<KeyValuePair<T, string>> GetAllDescriptions<T>(T o, BindingFlags b = BindingFlags.Public | BindingFlags.Static)
        {
            DescriptionAttribute da;
            var fi = o.GetType().GetFields(b);
            int c = 0;
            var l = new List<KeyValuePair<T, string>>();
            KeyValuePair<T, string> kv;
            foreach (var fe in fi)
            {
                da = (DescriptionAttribute)fe.GetCustomAttribute(typeof(DescriptionAttribute));
                if (da is object)
                {
                    kv = new KeyValuePair<T, string>((T)fe.GetValue(o), da.Description);
                    l.Add(kv);
                }
            }

            return l;
        }

        public static List<KeyValuePair<T, string>> GetAllSymbols<T>(T o, BindingFlags b = BindingFlags.Public | BindingFlags.Static)
        {
            UnitSymbolAttribute da;
            var fi = o.GetType().GetFields(b);
            int c = 0;
            var l = new List<KeyValuePair<T, string>>();
            KeyValuePair<T, string> kv;
            foreach (var fe in fi)
            {
                da = (UnitSymbolAttribute)fe.GetCustomAttribute(typeof(UnitSymbolAttribute));
                if (da is object)
                {
                    kv = new KeyValuePair<T, string>((T)fe.GetValue(o), da.Symbol);
                    l.Add(kv);
                }
            }

            return l;
        }

        public static List<KeyValuePair<T, double>> GetAllMeasureUnits<T>(T o, BindingFlags b = BindingFlags.Public | BindingFlags.Static)
        {
            MeasureUnitAttribute da;
            var fi = o.GetType().GetFields(b);
            int c = 0;
            var l = new List<KeyValuePair<T, double>>();
            KeyValuePair<T, double> kv;
            foreach (var fe in fi)
            {
                da = (MeasureUnitAttribute)fe.GetCustomAttribute(typeof(MeasureUnitAttribute));
                if (da is object)
                {
                    kv = new KeyValuePair<T, double>((T)fe.GetValue(o), (double)da.Unit);
                    l.Add(kv);
                }
            }

            return l;
        }

        public static List<KeyValuePair<T, double>> GetAllMultipliers<T>(T o, BindingFlags b = BindingFlags.Public | BindingFlags.Static)
        {
            MultiplierAttribute da;
            var fi = o.GetType().GetFields(b);
            int c = 0;
            var l = new List<KeyValuePair<T, double>>();
            KeyValuePair<T, double> kv;
            foreach (var fe in fi)
            {
                da = (MultiplierAttribute)fe.GetCustomAttribute(typeof(MultiplierAttribute));
                if (da is object)
                {
                    kv = new KeyValuePair<T, double>((T)fe.GetValue(o), da.Value);
                    l.Add(kv);
                }
            }

            return l;
        }

        public static T[] GetAllEnumVals<T>()
        {
            var fi = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);
            T[] x;
            int i = 0;
            x = new T[fi.Length];
            foreach (var fe in fi)
            {
                x[i] = (T)fe.GetValue(x[i]);
                i += 1;
            }

            return x;
        }
    }

    #endregion

    #region Custom Attributes

    /// <summary>
/// Provides a detailed description.
/// </summary>
/// <remarks></remarks>
    public class DetailAttribute : Attribute
    {
        private string _Detail;

        public string Detail
        {
            get
            {
                return _Detail;
            }
        }

        public DetailAttribute(string detail)
        {
            _Detail = detail;
        }
    }

    /// <summary>
/// Provides the byte length of the property.
/// </summary>
/// <remarks></remarks>
    public class ByteLengthAttribute : Attribute
    {
        private ushort _Length = 4;

        public ushort Length
        {
            get
            {
                return _Length;
            }
        }

        public ByteLengthAttribute()
        {
        }

        public ByteLengthAttribute(ushort value)
        {
            _Length = value;
        }
    }

    /// <summary>
/// Provides the multiplier of the property.
/// </summary>
/// <remarks></remarks>
    public class MultiplierAttribute : Attribute
    {
        private double _Value = 1.0d;

        public double Value
        {
            get
            {
                double ValueRet = default;
                ValueRet = _Value;
                return ValueRet;
            }
        }

        public MultiplierAttribute(double Value)
        {
            _Value = Value;
        }

        public MultiplierAttribute()
        {
            _Value = Value;
        }
    }

    /// <summary>
/// Provides the unit of measure of the property.
/// </summary>
/// <remarks></remarks>
    public class MeasureUnitAttribute : Attribute
    {
        private MeasureUnitTypes _Unit;

        public MeasureUnitTypes Unit
        {
            get
            {
                return _Unit;
            }
        }

        public MeasureUnitAttribute(MeasureUnitTypes Unit)
        {
            _Unit = Unit;
        }
    }

    /// <summary>
/// Provides the unit symbol of the property.
/// </summary>
/// <remarks></remarks>
    public class UnitSymbolAttribute : Attribute
    {
        private string _Symbol;

        public string Symbol
        {
            get
            {
                return _Symbol;
            }
        }

        public UnitSymbolAttribute(string Symbol)
        {
            _Symbol = Symbol;
        }
    }

    /// <summary>
/// Provides the number format for the property.
/// </summary>
/// <remarks></remarks>
    public class NumberFormatAttribute : Attribute
    {
        private string _Format;

        public string Format
        {
            get
            {
                return _Format;
            }
        }

        public NumberFormatAttribute(string Format)
        {
            _Format = Format;
        }

        public NumberFormatAttribute()
        {
            _Format = "0.0";
        }
    }

    #endregion

    #region Power Sources

    /// <summary>
/// Represents an enumeration of power source characteristics.
/// </summary>
/// <remarks></remarks>
    [DefaultValue(-1)]
    public enum PowerStates
    {

        /// <summary>
    /// Communication with the device has not been established.
    /// </summary>
    /// <remarks></remarks>
        [Description("Uninitialized.")]
        [Detail("Communication with the device has not been established.")]
        Uninitialized = -1,

        /// <summary>
    /// Operating on A/C power from the utility.
    /// </summary>
    /// <remarks></remarks>
        [Description("Utility Power")]
        [Detail("Operating on A/C power from the utility.")]
        Utility,

        /// <summary>
    /// Operating on battery power because there is no power coming from the utility.
    /// </summary>
    /// <remarks></remarks>
        [Description("Battery Power")]
        [Detail("Operating on battery power because there is no power coming from the utility.")]
        Battery,

        /// <summary>
    /// Operating on battery power because there is low voltage coming from the utility.
    /// </summary>
    /// <remarks></remarks>
        [Description("Battery Power Due To Low Voltage")]
        [Detail("Operating on battery power because the voltage coming from the utility is too low.")]
        BatteryTransferLow,

        /// <summary>
    /// Operating on battery power because there is high voltage coming from the utility.
    /// </summary>
    /// <remarks></remarks>
        [Description("Battery Power Due To High Voltage")]
        [Detail("Operating on battery power because the voltage coming from the utility is too high.")]
        BatteryTransferHigh
    }

    #endregion

    #region TrippLiteCodes

    /// <summary>
/// Represents feature command codes that can be sent to a Tripp Lite Smart battery.
/// </summary>
/// <remarks></remarks>
    public enum TrippLiteCodes : byte
    {

        /// <summary>
    /// VA RATING
    /// </summary>
        [Description("VA RATING")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        VARATING = 0x3,

        /// <summary>
    /// Nominal Battery Voltage
    /// </summary>
        [Description("Nominal Battery Voltage")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        NominalBatteryVoltage = 0x4,

        /// <summary>
    /// Low Voltage Transfer
    /// </summary>
        [Description("Low Voltage Transfer")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        LowVoltageTransfer = 0x6,

        /// <summary>
    /// High Voltage Transfer
    /// </summary>
        [Description("High Voltage Transfer")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        HighVoltageTransfer = 0x9,

        /// <summary>
    /// Input Frequency
    /// </summary>
        [Description("Input Frequency")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Hertz)]
        [NumberFormat()]
        [ByteLength()]
        InputFrequency = 0x19,

        /// <summary>
    /// Output Frequency
    /// </summary>
        [Description("Output Frequency")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Hertz)]
        [NumberFormat()]
        [ByteLength()]
        OutputFrequency = 0x1C,

        /// <summary>
    /// Input Voltage
    /// </summary>
        [Description("Input Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("000.0")]
        [ByteLength()]
        InputVoltage = 0x31,

        /// <summary>
    /// Output Voltage
    /// </summary>
        [Description("Output Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("000.0")]
        [ByteLength()]
        OutputVoltage = 0x1B,

        /// <summary>
    /// Output Current
    /// </summary>
        [Description("Output Current")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Amp)]
        [NumberFormat()]
        [ByteLength()]
        OutputCurrent = 0x46,

        /// <summary>
    /// Output Power
    /// </summary>
        [Description("Output Power")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Watt)]
        [NumberFormat("0")]
        [ByteLength()]
        OutputPower = 0x47,

        /// <summary>
    /// Output Load
    /// </summary>
        [Description("Ouput Load")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Percent)]
        [NumberFormat("0")]
        [ByteLength()]
        OutputLoad = 0x1E,

        /// <summary>
    /// Seconds Remaining Power
    /// </summary>
        [Description("Time Remaining")]
        [Multiplier(1d / 60d)]
        [MeasureUnit(MeasureUnitTypes.Time)]
        [NumberFormat()]
        [ByteLength()]
        TimeRemaining = 0x35,

        /// <summary>
    /// Seconds Remaining Power
    /// </summary>
        [Description("Battery Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat()]
        [ByteLength()]
        BatteryVoltage = 0x20,

        /// <summary>
    /// Charge Remaining
    /// </summary>
        [Description("Charge Remaining")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Percent)]
        [NumberFormat("0")]
        [ByteLength()]
        ChargeRemaining = 0x34
    }

    #endregion

    #region MeasureUnitTypes

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

    #endregion

    #region MeasureUnit

    /// <summary>
/// Represents a unit of measure.
/// </summary>
/// <remarks></remarks>
    public class MeasureUnit
    {
        protected string _UnitSymbol;
        protected string _Name;
        protected MeasureUnitTypes _UnitType;
        protected static Collection<MeasureUnit> _Units;

        public static MeasureUnit FindUnit(MeasureUnitTypes t)
        {
            foreach (var mu in _Units)
            {
                if (mu.UnitType == t)
                    return mu;
            }

            return null;
        }

        protected MeasureUnit(MeasureUnitTypes unit, string name, string symbol)
        {
            _UnitSymbol = symbol;
            _UnitType = unit;
            _Name = name;
        }

        static MeasureUnit()
        {
            _Units = new Collection<MeasureUnit>();
            var fi = typeof(MeasureUnitTypes).GetFields(BindingFlags.Public | BindingFlags.Static);
            MeasureUnit mu;
            var mt = MeasureUnitTypes.Amp;
            var kvc = GetAllDescriptions<MeasureUnitTypes>(MeasureUnitTypes.Amp);
            var mvc = GetAllSymbols<MeasureUnitTypes>(MeasureUnitTypes.Amp);
            int i = 0;
            int c = kvc.Count - 1;
            KeyValuePair<MeasureUnitTypes, string> kv;
            int d;
            if (mvc is null)
                d = -1;
            else
                d = 0;
            string sym;
            string desc;
            var loopTo = c;
            for (i = 0; i <= loopTo; i++)
            {
                kv = kvc[i];
                sym = "";
                if (d != -1)
                {
                    if (mvc[d].Key == kvc[i].Key)
                    {
                        sym = mvc[d].Value;
                        d += 1;
                        if (d >= mvc.Count)
                            d = -1;
                    }
                }

                desc = kvc[i].Value;
                mt = kv.Key;
                mu = new MeasureUnit(mt, desc, sym);
                _Units.Add(mu);
            }
        }

        public static Collection<MeasureUnit> Units
        {
            get
            {
                return _Units;
            }
        }

        public string UnitSymbol
        {
            get
            {
                return _UnitSymbol;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public MeasureUnitTypes UnitType
        {
            get
            {
                return _UnitType;
            }
        }

        public override string ToString()
        {
            string ToStringRet = default;
            ToStringRet = Name;
            return ToStringRet;
        }
    }
}

#endregion



