﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using DataTools.Win32.Memory;
using DataTools.Win32.Usb;

using static TrippLite.TrippLiteCodeUtility;

namespace TrippLite
{

    #region TrippLiteProperty Class

    /// <summary>
    /// Encapsulates a USB HID feature property for a Tripp Lite Smart Battery.
    /// </summary>
    /// <remarks></remarks>
    public sealed class TrippLiteProperty : INotifyPropertyChanged, IDisposable, IChild<TrippLiteUPS>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PropertyChangedEventArgs propEvent = new PropertyChangedEventArgs("Value");
        private TrippLiteCodes propCode;

        internal long value = -1;

        private bool isSettable;

        internal TrippLiteUPS model;
        internal TrippLitePropertyBag propBag;

        private ushort byteLen = 4;

        /// <summary>
        /// Initialize a new TrippLiteProperty
        /// </summary>
        /// <param name="owner">The TrippLiteUPS model object that will own this property.</param>
        /// <param name="c">The property code.</param>
        /// <remarks></remarks>
        internal TrippLiteProperty(TrippLiteUPS owner, TrippLiteCodes c)
        {
            model = owner;
            propCode = c;
            byteLen = ByteLength;
        }

        /// <summary>
        /// Specify whether or not the property object will call to the device, directly, to retrieve (or set) the value of its property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool LiveInterface { get; set; } = false;

        /// <summary>
        /// Indicates whether this is a property that raises change events.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsActiveProperty { get; set; } = true;

        /// <summary>
        /// Gets the owner TrippLiteUPS object for this property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TrippLiteUPS Parent
        {
            get => model;
            internal set
            {
                model = value;
            }
        }

        TrippLiteUPS IChild<TrippLiteUPS>.Parent
        {
            get => model;
            set => model = value;
        }

        /// <summary>
        /// Gets the hard-coded number format for this property type.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string NumberFormat
        {
            get => GetEnumAttrVal<NumberFormatAttribute, string, TrippLiteCodes>(propCode, "Format");
        }

        /// <summary>
        /// Gets the multiplier of the property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double Multiplier
        {
            get => GetEnumAttrVal<MultiplierAttribute, double, TrippLiteCodes>(propCode, "Value");
        }

        /// <summary>
        /// Gets the length of the USB HID property, in bytes, as defined in TrippLiteCodes.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public ushort ByteLength
        {
            get
            {
                ushort bl = GetEnumAttrVal<ByteLengthAttribute, ushort, TrippLiteCodes>(propCode, "Length");

                if (bl == 0)
                    bl = 4;

                return bl;
            }
        }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long Value
        {
            get
            {
                if (LiveInterface)
                {
                    value = GetValue();
                }

                return value;
            }
            set
            {
                if (LiveInterface && IsSettable)
                {
                    switch (byteLen)
                    {
                        case var @case when @case <= 4:
                            SetValue((int)value);
                            break;

                        case var case1 when case1 <= 8:
                            SetValue(value);
                            break;

                        default:
                            throw new ArgumentException("This property cannot be set that way.");
                    }

                    if (IsActiveProperty)
                        PropertyChanged?.Invoke(this, propEvent);
                }
                else if (this.value != value || this.value == -1)
                {
                    this.value = value;

                    if (IsActiveProperty)
                        PropertyChanged?.Invoke(this, propEvent);
                }
            }
        }

        // Public ReadOnly Property RawValue As Byte()
        // Get

        // End Get
        // End Property

        /// <summary>
        /// Gets a value indicating whether or not this property supports setting on the device.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsSettable
        {
            get
            {
                if (model is null)
                    return false;

                return isSettable;
            }
            internal set
            {
                isSettable = value;
            }
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Name
        {
            get => propCode.ToString();
        }

        /// <summary>
        /// Gets the description of the property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Description
        {
            get => GetEnumAttrVal<DescriptionAttribute, string, TrippLiteCodes>(propCode, "Description");
        }

        /// <summary>
        /// Gets the unit of the property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public MeasureUnitTypes Unit
        {
            get => GetEnumAttrVal<MeasureUnitAttribute, MeasureUnitTypes, TrippLiteCodes>(propCode, "Unit");
        }

        /// <summary>
        /// Gets the Tripp Lite property code.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TrippLiteCodes Code
        {
            get => propCode;
        }

        /// <summary>
        /// Gets detailed information about the property unit as a MeasureUnit object.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public MeasureUnit UnitInfo
        {
            get => MeasureUnit.FindUnit(Unit);
        }

        /// <summary>
        /// Attempts to set a value or flag on the device using raw byte data.
        /// The number of bytes in the byte array must exactly match the byte length of the property.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool SetValue(byte[] value)
        {
            bool res;

            if (IsSettable == false || value is null || value.Length != byteLen)
                return false;

            var dev = DataTools.Win32.Usb.HidFeatures.OpenHid(model.Device);

            if (dev == IntPtr.Zero)
                return false;

            MemPtr mm = new MemPtr();

            mm.Alloc(1 + value.Length);
            mm.ByteAt(0) = (byte)propCode;

            res = UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)1 + value.Length);
            HidFeatures.CloseHid(dev);

            if (res)
            {
                if (value.Length >= 8)
                {
                    this.value = mm.LongAt(1L);
                }
                else
                {
                    this.value = mm.IntAt(1L);
                }

                mm.Free();
            
                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, propEvent);
            }
            else
            {
                mm.Free();
            }

            return res;
        }

        /// <summary>
        /// Attempts to retrieve the value directly from the device.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public long GetValue()
        {
            long res = 0L;

            var dev = HidFeatures.OpenHid(model.Device);

            if (dev == IntPtr.Zero)
                return value;

            MemPtr mm = new MemPtr();

            mm.Alloc(byteLen + 1);
            mm.ByteAt(0) = (byte)propCode;

            if (UsbLibHelpers.HidD_GetFeature(dev, mm.Handle, (int)byteLen + 1))
            {
                if (byteLen == 8)
                {
                    res = mm.LongAtAbsolute(1L);
                }
                else
                {
                    res = mm.IntAtAbsolute(1L);
                }
            }

            HidFeatures.CloseHid(dev);
            
            mm.Free();
            
            return res;
        }

        /// <summary>
        /// Attempt to set a value or flag on the device.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool SetValue(long value)
        {
            bool res;

            if (byteLen < 8)
                return SetValue((int)value);

            if (IsSettable == false)
                return false;

            if (Value == value)
                return false;

            var dev = HidFeatures.OpenHid(model.Device);

            if (dev == IntPtr.Zero)
                return false;

            MemPtr mm = new MemPtr();
            
            mm.Alloc(9L);
            mm.ByteAt(0L) = (byte)propCode;
            
            mm.LongAtAbsolute(1L) = value;
            
            res = UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)9);
            
            mm.Free();
            HidFeatures.CloseHid(dev);
            
            if (res)
            {
                this.value = value;
            
                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, propEvent);
            }

            return res;
        }

        /// <summary>
        /// Attempt to set a value or flag on the device.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool SetValue(int value)
        {
            bool res;

            if (byteLen < 4)
                return SetValue((short)value);

            if (IsSettable == false)
                return false;

            if (Value == value)
                return false;

            var dev = HidFeatures.OpenHid(model.Device);

            if (dev == IntPtr.Zero)
                return false;

            MemPtr mm = new MemPtr();

            mm.Alloc(5L);
            mm.ByteAt(0L) = (byte)propCode;

            mm.IntAtAbsolute(1L) = value;

            res = UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)5);

            mm.Free();

            HidFeatures.CloseHid(dev);

            if (res)
            {
                this.value = value;

                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, propEvent);
            }

            return res;
        }

        /// <summary>
        /// Attempt to set a value or flag on the device.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool SetValue(short value)
        {
            bool res = default;

            if (byteLen < 2)
                return SetValue((byte)value);

            if (IsSettable == false)
                return false;

            if (Value == value)
                return false;

            var dev = HidFeatures.OpenHid(model.Device);

            if (dev == IntPtr.Zero)
                return false;

            MemPtr mm = new MemPtr();

            mm.Alloc(3L);
            mm.ByteAt(0L) = (byte)propCode;
            mm.ShortAtAbsolute(1L) = value;

            res = UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)3);

            mm.Free();

            HidFeatures.CloseHid(dev);

            if (res)
            {
                this.value = value;

                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, propEvent);
            }

            return res;
        }

        /// <summary>
        /// Attempt to set a value or flag on the device.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool SetValue(byte value)
        {
            bool res = default;

            if (IsSettable == false)
                return false;

            if (Value == value)
                return false;

            var dev = HidFeatures.OpenHid(model.Device);

            if (dev == IntPtr.Zero)
                return false;

            MemPtr mm = new MemPtr();

            mm.Alloc(2L);
            mm.ByteAt(0L) = (byte)propCode;
            mm.ByteAt(1L) = value;

            res = UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)2);

            mm.Free();

            HidFeatures.CloseHid(dev);

            if (res)
            {
                this.value = value;

                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, propEvent);
            }

            return res;
        }

        /// <summary>
        /// Attempt to move this item to another property bag.
        /// </summary>
        /// <param name="bag">The destination property bag.</param>
        /// <returns>True if successful.</returns>
        /// <remarks></remarks>
        public bool MoveTo(TrippLitePropertyBag bag)
        {
            if (model is null || ReferenceEquals(bag, propBag))
                return false;

            if (Parent.PropertyBag.Contains(this))
            {
                if (Parent.PropertyBag.MoveTo(this, bag))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts this property into its string representation
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ToString()
        {
            return ToString(NumberFormat);
        }

        /// <summary>
        /// Converts this property into its string representation using the provided number format.
        /// </summary>
        /// <param name="numFmt">The standard format string used to format the numeric value of this property.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ToString(string numFmt)
        {
            return ToString(numFmt, false);
        }

        /// <summary>
        /// Converts this property into its string representation using the provided number format.
        /// </summary>
        /// <param name="numFmt">The standard format string used to format the numeric value of this property.</param>
        /// <param name="suppressUnit">Sets a value indicating whether or not to suppress appending the unit symbol to the string value of this property.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ToString(string numFmt, bool suppressUnit)
        {
            return ((float)(Value * Multiplier)).ToString(numFmt) + (!suppressUnit ? " " + UnitInfo.UnitSymbol : "");
        }

        public static explicit operator double(TrippLiteProperty v)
        {
            return v.Value;
        }

        public static explicit operator int(TrippLiteProperty v)
        {
            return (int)v.Value;
        }

        public static explicit operator string(TrippLiteProperty v)
        {
            return v.ToString();
        }

        internal void SignalRefresh()
        {
            if (IsActiveProperty)
                PropertyChanged?.Invoke(this, propEvent);
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                model = null;
                propBag = null;
            }

            disposedValue = true;
        }

        ~TrippLiteProperty()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

    #endregion

    #region TrippLitePropertyBag

    /// <summary>
    /// Represents a collection of all USB HID feature properties for a Tripp Lite Smart Battery
    /// </summary>
    /// <remarks></remarks>
    public class TrippLitePropertyBag : ObservableCollection<TrippLiteProperty>, IDisposable, IChild<TrippLiteUPS>
    {
        private TrippLiteUPS model;

        /// <summary>
        /// Gets the owner TrippLiteUPS object for this property bag.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TrippLiteUPS Parent
        {
            get
            {
                return model;
            }

            internal set
            {
                model = value;
            }
        }

        TrippLiteUPS IChild<TrippLiteUPS>.Parent
        {
            get => model;
            set => model = value;
        }

        public TrippLiteProperty FindProperty(TrippLiteCodes c)
        {
            foreach (var fp in this)
            {
                if (fp.Code == c)
                    return fp;
            }

            return null;
        }

        /// <summary>
        /// Initialize a new TrippLitePropertyBag and automatically populate the property bag with known property values from the TrippLiteCodes enumeration.
        /// </summary>
        /// <param name="owner">The TrippLiteUPS object upon which to initialize.</param>
        /// <remarks></remarks>
        public TrippLitePropertyBag(TrippLiteUPS owner) : this(owner, true)
        {
        }

        /// <summary>
        /// Initialize a new TrippLitePropertyBag
        /// </summary>
        /// <param name="owner">The TrippLiteUPS object upon which to initialize.</param>
        /// <param name="autoPopulate">Specify whether to automatically populate the property bag with known property values from the TrippLiteCodes enumeration.</param>
        /// <remarks></remarks>
        public TrippLitePropertyBag(TrippLiteUPS owner, bool autoPopulate) : base()
        {
            model = owner;
        
            if (autoPopulate)
            {
                var t = GetAllEnumVals<TrippLiteCodes>();
            
                int i;
                int c = t.Length;
                
                for (i = 0; i < c; i++)
                    Add(new TrippLiteProperty(owner, t[i]));
            }
        }

        public bool MoveTo(TrippLiteProperty item, TrippLitePropertyBag newBag)
        {
            if (Contains(item) && !newBag.Contains(item))
            {
                Remove(item);
            
                newBag.Add(item);
                
                item.model = newBag.model;
                item.propBag = newBag;
                
                return true;
            }

            return false;
        }

        internal void SignalRefresh()
        {
            foreach (var p in this)
                p.SignalRefresh();
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    foreach (var x in this)
                        x.Dispose();
                
                    Clear();
                    
                    GC.Collect(0);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}

#endregion

