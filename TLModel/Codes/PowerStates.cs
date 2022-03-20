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
}
