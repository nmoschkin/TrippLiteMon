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
    public struct BatteryPropertyCode 
    {
        private byte value;

        public static implicit operator byte(BatteryPropertyCode val)
        {
            return val.value;
        }

        public static implicit operator int(BatteryPropertyCode val)
        {
            return val.value;
        }

        public static implicit operator short(BatteryPropertyCode val)
        {
            return val.value;
        }

        public static implicit operator BatteryPropertyCode(byte value)
        {
            return new BatteryPropertyCode()
            {
                value = value
            };
        }
    }


    internal class FieldMapper
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

            Initialize();
        }

        private void Initialize()
        {
            if (deviceInfo == null)
            {
                this.initialized = false;
                return;
            }

            this.initialized = true;
        }

        public void MapField(out BatteryPropertyCode value, byte code, byte collection = 0x0)
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

    /// <summary>
    /// Represents feature command codes that can be sent to a Tripp Lite Smart battery.
    /// </summary>
    /// <remarks></remarks>
    public class BatteryPropertyMap
    {
        private PropertyInfo[] props;

        public BatteryPropertyMap(HidDeviceInfo deviceInfo)
        {
            var mapper = new FieldMapper(deviceInfo);

            if (mapper.Initialized)
            {

            }

            props = typeof(BatteryPropertyMap).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            BatteryPropertyCode c;

            mapper.MapField(out c, 0x43);
            VARATING = c;
            
            mapper.MapField(out c, 0x40, 0x12);
            NominalBatteryVoltage = c;
            
            mapper.MapField(out c, 0x53);
            LowVoltageTransfer = c;
            
            mapper.MapField(out c, 0x54);
            HighVoltageTransfer = c;
            
            mapper.MapField(out c, 0x32, 0x1a);
            InputFrequency = c;
            
            mapper.MapField(out c, 0x32, 0x1c);
            OutputFrequency = c;
            
            mapper.MapField(out c, 0x30, 0x1a);
            InputVoltage = c;
            
            mapper.MapField(out c, 0x30, 0x1c);
            OutputVoltage = c;
            
            mapper.MapField(out c, 0x31);
            OutputCurrent = c;
            
            mapper.MapField(out c, 0x34);
            OutputPower = c;
            
            mapper.MapField(out c, 0x35);
            OutputLoad = c;
            
            mapper.MapField(out c, 0x68, 1);
            TimeRemaining = c;
            
            mapper.MapField(out c, 0x30, 0x12);
            BatteryVoltage = c;
            
            mapper.MapField(out c, 0x66, 1);
            ChargeRemaining = c;
        }

        /// <summary>
        /// VA RATING
        /// </summary>
        [Description("VA RATING")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        public BatteryPropertyCode VARATING { get; private set; }

        /// <summary>
        /// Nominal Battery Voltage
        /// </summary>
        [Description("Nominal Battery Voltage")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        public BatteryPropertyCode NominalBatteryVoltage { get; private set; }

        /// <summary>
        /// Low Voltage Transfer
        /// </summary>
        [Description("Low Voltage Transfer")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        public BatteryPropertyCode LowVoltageTransfer { get; private set; }

        /// <summary>
        /// High Voltage Transfer
        /// </summary>
        [Description("High Voltage Transfer")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("0")]
        [ByteLength()]
        public BatteryPropertyCode HighVoltageTransfer { get; private set; }

        /// <summary>
        /// Input Frequency
        /// </summary>
        [Description("Input Frequency")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Hertz)]
        [NumberFormat()]
        [ByteLength()]
        public BatteryPropertyCode InputFrequency { get; private set; }

        /// <summary>
        /// Output Frequency
        /// </summary>
        [Description("Output Frequency")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Hertz)]
        [NumberFormat()]
        [ByteLength()]
        public BatteryPropertyCode OutputFrequency { get; private set; }

        /// <summary>
        /// Input Voltage
        /// </summary>
        [Description("Input Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("000.0")]
        [ByteLength()]
        public BatteryPropertyCode InputVoltage { get; private set; }

        /// <summary>
        /// Output Voltage
        /// </summary>
        [Description("Output Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat("000.0")]
        [ByteLength()]
        public BatteryPropertyCode OutputVoltage { get; private set; }

        /// <summary>
        /// Output Current
        /// </summary>
        [Description("Output Current")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Amp)]
        [NumberFormat()]
        [ByteLength()]
        public BatteryPropertyCode OutputCurrent { get; private set; }

        /// <summary>
        /// Output Power
        /// </summary>
        [Description("Output Power")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Watt)]
        [NumberFormat("0")]
        [ByteLength()]
        public BatteryPropertyCode OutputPower { get; private set; }

        /// <summary>
        /// Output Load
        /// </summary>
        [Description("Ouput Load")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Percent)]
        [NumberFormat("0")]
        [ByteLength()]
        public BatteryPropertyCode OutputLoad { get; private set; }

        /// <summary>
        /// Seconds Remaining Power
        /// </summary>
        [Description("Time Remaining")]
        [Multiplier(1d / 60d)]
        [MeasureUnit(MeasureUnitTypes.Time)]
        [NumberFormat()]
        [ByteLength()]
        public BatteryPropertyCode TimeRemaining { get; private set; }

        /// <summary>
        /// Seconds Remaining Power
        /// </summary>
        [Description("Battery Voltage")]
        [Multiplier(0.1d)]
        [MeasureUnit(MeasureUnitTypes.Volt)]
        [NumberFormat()]
        [ByteLength()]
        public BatteryPropertyCode BatteryVoltage { get; private set; }

        /// <summary>
        /// Charge Remaining
        /// </summary>
        [Description("Charge Remaining")]
        [Multiplier(1d)]
        [MeasureUnit(MeasureUnitTypes.Percent)]
        [NumberFormat("0")]
        [ByteLength()]
        public BatteryPropertyCode ChargeRemaining { get; private set; }


        /// <summary>
        /// Get all codes.
        /// </summary>
        /// <returns></returns>
        public BatteryPropertyCode[]? GetAllCodes()
        {
            var l = new List<BatteryPropertyCode>();

            foreach (var prop in props)
            {
                var bv = (BatteryPropertyCode?)prop.GetValue(this);

                if (bv != null)
                {
                    l.Add((BatteryPropertyCode)bv);
                }

            }

            return l.ToArray();
        }


        public U? GetValueAttribute<T, U>(BatteryPropertyCode code, string attrPropName) where T : Attribute
        {
            U? result = default;

            foreach (var prop in props)
            {
                var bv = (BatteryPropertyCode?)prop.GetValue(this);

                if (bv != null && bv == code)
                {

                    var attr = prop.GetCustomAttribute<T>();
                    if (attr != null)
                    {
                        var apInfo = attr.GetType().GetProperty(attrPropName);
                        if (apInfo != null)
                        {
                            result = (U?)apInfo.GetValue(attr);
                        }
                    }

                }

            }

            return result;

        }

        /// <summary>
        /// Gets the name of the property specified by the given code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetNameFromCode(BatteryPropertyCode code)
        {
            foreach (var prop in props)
            {
                var bv = (BatteryPropertyCode?)prop.GetValue(this);

                if (bv != null && bv == code)
                {
                    return prop.Name;
                }

            }

            return ((byte)code).ToString();
        }

    }


}
