using DataTools.Win32.Usb.Power;
using DataTools.Win32.Usb;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using static TrippLite.TrippLiteCodeUtility;
using DataTools.Win32;

namespace TrippLite
{
    /// <summary>
    /// Represents feature command codes that can be sent to a Tripp Lite Smart battery.
    /// </summary>
    /// <remarks></remarks>
    public struct TrippLiteCodes 
    {
        private byte value;

        public static void Initialize(HidDeviceInfo deviceInfo)
        {
            var mapper = new FieldMapper(deviceInfo);

            fields = typeof(TrippLiteCodes).GetFields(BindingFlags.Public | BindingFlags.Static);

            mapper.MapField(out VARATING, 0x43);
            mapper.MapField(out NominalBatteryVoltage, 0x40, 0x12);
            mapper.MapField(out LowVoltageTransfer, 0x53);
            mapper.MapField(out HighVoltageTransfer, 0x54);
            mapper.MapField(out InputFrequency, 0x32, 0x1a);
            mapper.MapField(out OutputFrequency, 0x32, 0x1c);
            mapper.MapField(out InputVoltage, 0x30, 0x1a);
            mapper.MapField(out OutputVoltage, 0x30, 0x1c);
            mapper.MapField(out OutputCurrent, 0x31);
            mapper.MapField(out OutputPower, 0x34);
            mapper.MapField(out OutputLoad, 0x35);
            mapper.MapField(out TimeRemaining, 0x68, 1);
            mapper.MapField(out BatteryVoltage, 0x30, 0x12);
            mapper.MapField(out ChargeRemaining, 0x66, 1);
        }

        /// <summary>
        /// VA RATING
        /// </summary>
        [Description("VA RATING")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        public static TrippLiteCodes VARATING;

        /// <summary>
        /// Nominal Battery Voltage
        /// </summary>
        [Description("Nominal Battery Voltage")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        public static TrippLiteCodes NominalBatteryVoltage;

        /// <summary>
        /// Low Voltage Transfer
        /// </summary>
        [Description("Low Voltage Transfer")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        public static TrippLiteCodes LowVoltageTransfer;

        /// <summary>
        /// High Voltage Transfer
        /// </summary>
        [Description("High Voltage Transfer")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        public static TrippLiteCodes HighVoltageTransfer;

        /// <summary>
        /// Input Frequency
        /// </summary>
        [Description("Input Frequency")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Hertz)]
        [NumberFormat()]
        [ByteLength()]
        public static TrippLiteCodes InputFrequency;

        /// <summary>
        /// Output Frequency
        /// </summary>
        [Description("Output Frequency")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Hertz)]
        [NumberFormat()]
        [ByteLength()]
        public static TrippLiteCodes OutputFrequency;

        /// <summary>
        /// Input Voltage
        /// </summary>
        [Description("Input Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("000.0")]
        [ByteLength()]
        public static TrippLiteCodes InputVoltage;

        /// <summary>
        /// Output Voltage
        /// </summary>
        [Description("Output Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("000.0")]
        [ByteLength()]
        public static TrippLiteCodes OutputVoltage;

        /// <summary>
        /// Output Current
        /// </summary>
        [Description("Output Current")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Amp)]
        [NumberFormat()]
        [ByteLength()]
        public static TrippLiteCodes OutputCurrent;

        /// <summary>
        /// Output Power
        /// </summary>
        [Description("Output Power")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Watt)]
        [NumberFormat("0")]
        [ByteLength()]
        public static TrippLiteCodes OutputPower;

        /// <summary>
        /// Output Load
        /// </summary>
        [Description("Ouput Load")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Percent)]
        [NumberFormat("0")]
        [ByteLength()]
        public static TrippLiteCodes OutputLoad;

        /// <summary>
        /// Seconds Remaining Power
        /// </summary>
        [Description("Time Remaining")]
        [Multiplier(1d / 60d)]
        [MeasureUnit(MeasureUnitTypes.Time)]
        [NumberFormat()]
        [ByteLength()]
        public static TrippLiteCodes TimeRemaining;

        /// <summary>
        /// Seconds Remaining Power
        /// </summary>
        [Description("Battery Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat()]
        [ByteLength()]
        public static TrippLiteCodes BatteryVoltage;

        /// <summary>
        /// Charge Remaining
        /// </summary>
        [Description("Charge Remaining")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Percent)]
        [NumberFormat("0")]
        [ByteLength()]
        public static TrippLiteCodes ChargeRemaining;

        static FieldInfo[] fields;

        public override string ToString()
        {
            foreach (var fi in fields)
            {
                TrippLiteCodes? b = (TrippLiteCodes?)fi.GetValue(null);
                if (b == value)
                {
                    return fi.Name;
                }                
            }   

            return value.ToString();
        }

        public static implicit operator byte(TrippLiteCodes val)
        {
            return val.value;
        }

        public static implicit operator int(TrippLiteCodes val)
        {
            return val.value;
        }

        public static implicit operator short(TrippLiteCodes val)
        {
            return val.value;
        }

        public static implicit operator TrippLiteCodes(byte value)
        {
            return new TrippLiteCodes()
            {
                value = value
            };
        }

    }

    public class FieldMapper
    {

        bool initialized;
        HidPowerDeviceInfo deviceInfo;
        public bool Initialized => initialized;

        public HidPowerDeviceInfo DeviceInfo => deviceInfo;

        public FieldMapper(HidDeviceInfo hidDevice)
        {
            this.initialized = false;
            
            if (hidDevice is HidPowerDeviceInfo hpd)
            {
                this.deviceInfo = hpd;
            }
            else
            {
                this.deviceInfo = HidPowerDeviceInfo.CreateFromHidDevice(hidDevice);
            }
        }

        public void Initialize()
        {
            if (deviceInfo == null)
            {
                this.initialized = false;
                return;
            }

            this.initialized = true;
        }

        public void MapField(out TrippLiteCodes value, byte code, byte collection = 0x0)
        {
            value = 0;
            HidPValueCaps? caps = null;

            //var features = deviceInfo.GetFeatureValues(HidUsageType.CL | HidUsageType.CA | HidUsageType.CP, HidUsageType.DV | HidUsageType.SV);

            caps = deviceInfo.FeatureValueCaps.Where((fv) =>
            {
                if (fv.Usage == code)
                {
                    if (collection == 0 || (fv.LinkCollection == collection) || (fv.LinkUsage == collection))
                    {
                        return true;
                    }
                }

                return false;
            }).FirstOrDefault();

            if (caps == null)
            {
                caps = deviceInfo.InputValueCaps.Where((fv) =>
                {
                    if (fv.Usage == code)
                    {
                        if (collection == 0 || (fv.LinkCollection == collection) || (fv.LinkUsage == collection))
                        {
                            return true;
                        }
                    }

                    return false;
                }).FirstOrDefault();

                if (caps == null)
                {
                    caps = deviceInfo.OutputValueCaps.Where((fv) =>
                    {
                        if (fv.Usage == code)
                        {
                            if (collection == 0 || (fv.LinkCollection == collection) || (fv.LinkUsage == collection))
                            {
                                return true;
                            }
                        }

                        return false;
                    }).FirstOrDefault();

                }

            }

            if (caps != null)
            {
                value = caps.Value.ReportID;
            }
        }
    }
}
