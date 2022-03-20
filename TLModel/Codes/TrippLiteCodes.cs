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
}
