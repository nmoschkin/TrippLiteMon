using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using DataTools.Win32.Memory;

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

        private PropertyChangedEventArgs _propEvent = new PropertyChangedEventArgs("Value");
        private TrippLiteCodes _Code;
        internal long _Value = -1;
        private bool _IsSettable;
        internal TrippLiteUPS _Model;
        internal TrippLitePropertyBag _Bag;
        private ushort _byteLen = 4;

        /// <summary>
    /// Initialize a new TrippLiteProperty
    /// </summary>
    /// <param name="owner">The TrippLiteUPS model object that will own this property.</param>
    /// <param name="c">The property code.</param>
    /// <remarks></remarks>
        internal TrippLiteProperty(TrippLiteUPS owner, TrippLiteCodes c)
        {
            _Model = owner;
            _Code = c;
            _byteLen = ByteLength;
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
            get
            {
                return _Model;
            }

            internal set
            {
                _Model = value;
            }
        }

        TrippLiteUPS IChild<TrippLiteUPS>.Parent
        {
            get => _Model;
            set => _Model = value;
        }

        /// <summary>
        /// Gets the hard-coded number format for this property type.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string NumberFormat
        {
            get
            {
                return GetEnumAttrVal<NumberFormatAttribute, string, TrippLiteCodes>(_Code, "Format");
            }
        }

        /// <summary>
    /// Gets the multiplier of the property.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public double Multiplier
        {
            get
            {
                return GetEnumAttrVal<MultiplierAttribute, double, TrippLiteCodes>(_Code, "Value");
            }
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
                ushort ByteLengthRet = default;
                ByteLengthRet = GetEnumAttrVal<ByteLengthAttribute, ushort, TrippLiteCodes>(_Code, "Length");
                if (ByteLengthRet == 0)
                    ByteLengthRet = 4;
                return ByteLengthRet;
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
                long ValueRet = default;
                if (LiveInterface)
                {
                    _Value = GetValue();
                }

                ValueRet = _Value;
                return ValueRet;
            }

            set
            {
                if (LiveInterface && IsSettable)
                {
                    switch (_byteLen)
                    {
                        case var @case when @case <= 4:
                            {
                                SetValue((int)value);
                                break;
                            }

                        case var case1 when case1 <= 8:
                            {
                                SetValue(value);
                                break;
                            }

                        default:
                            {
                                throw new ArgumentException("This property cannot be set that way.");
                                return;
                            }
                    }

                    if (IsActiveProperty)
                        PropertyChanged?.Invoke(this, _propEvent);
                }
                else if (_Value != value || _Value == -1)
                {
                    _Value = value;
                    if (IsActiveProperty)
                        PropertyChanged?.Invoke(this, _propEvent);
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
                if (_Model is null)
                    return false;
                return _IsSettable;
            }

            internal set
            {
                _IsSettable = value;
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
            get
            {
                return _Code.ToString();
            }
        }

        /// <summary>
    /// Gets the description of the property.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public string Description
        {
            get
            {
                return GetEnumAttrVal<DescriptionAttribute, string, TrippLiteCodes>(_Code, "Description");
            }
        }

        /// <summary>
    /// Gets the unit of the property.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public MeasureUnitTypes Unit
        {
            get
            {
                return GetEnumAttrVal<MeasureUnitAttribute, MeasureUnitTypes, TrippLiteCodes>(_Code, "Unit");
            }
        }

        /// <summary>
    /// Gets the Tripp Lite property code.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public TrippLiteCodes Code
        {
            get
            {
                return _Code;
            }
        }

        /// <summary>
    /// Gets detailed information about the property unit as a MeasureUnit object.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public MeasureUnit UnitInfo
        {
            get
            {
                return MeasureUnit.FindUnit(Unit);
            }
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
            bool SetValueRet = default;
            if (IsSettable == false || value is null || value.Length != _byteLen)
                return false;
            var dev = DataTools.Hardware.Usb.HidFeatures.OpenHid(_Model.Device);
            if (dev == IntPtr.Zero)
                return false;
            MemPtr mm = new MemPtr();
            mm.Alloc(1 + value.Length);
            mm.ByteAt(0) = (byte)_Code;

            SetValueRet = DataTools.Win32.UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)1 + value.Length);
            DataTools.Hardware.Usb.HidFeatures.CloseHid(dev);

            if (SetValueRet)
            {
                if (value.Length >= 8)
                {
                    _Value = mm.LongAt(1L);
                }
                else
                {
                    _Value = mm.IntAt(1L);
                }

                mm.Free();
                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, _propEvent);
            }
            else
            {
                mm.Free();
            }

            return SetValueRet;
        }

        /// <summary>
    /// Attempts to retrieve the value directly from the device.
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
        public long GetValue()
        {
            long GetValueRet = default;
            GetValueRet = 0L;
            var dev = DataTools.Hardware.Usb.HidFeatures.OpenHid(_Model.Device);
            if (dev == IntPtr.Zero)
                return _Value;
            MemPtr mm = new MemPtr();
            mm.Alloc(_byteLen + 1);
            mm.ByteAt(0) = (byte)_Code;
            if (DataTools.Win32.UsbLibHelpers.HidD_GetFeature(dev, mm.Handle, (int)_byteLen + 1))
            {
                if (_byteLen == 8)
                {
                    GetValueRet = mm.LongAtAbsolute(1L);
                }
                else
                {
                    GetValueRet = mm.IntAtAbsolute(1L);
                }
            }

            DataTools.Hardware.Usb.HidFeatures.CloseHid(dev);
            mm.Free();
            return GetValueRet;
        }

        /// <summary>
    /// Attempt to set a value or flag on the device.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool SetValue(long value)
        {
            bool SetValueRet = default;
            if (_byteLen < 8)
                return SetValue((int)value);
            if (IsSettable == false)
                return false;
            if (Value == value)
                return false;
            var dev = DataTools.Hardware.Usb.HidFeatures.OpenHid(_Model.Device);
            if (dev == IntPtr.Zero)
                return false;
            MemPtr mm = new MemPtr();
            mm.Alloc(9L);
            mm.ByteAt(0L) = (byte)_Code;
            mm.LongAtAbsolute(1L) = value;
            SetValueRet = DataTools.Win32.UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)9);
            mm.Free();
            DataTools.Hardware.Usb.HidFeatures.CloseHid(dev);
            if (SetValueRet)
            {
                _Value = value;
                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, _propEvent);
            }

            return SetValueRet;
        }

        /// <summary>
    /// Attempt to set a value or flag on the device.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool SetValue(int value)
        {
            bool SetValueRet = default;
            if (_byteLen < 4)
                return SetValue((short)value);
            if (IsSettable == false)
                return false;
            if (Value == value)
                return false;
            var dev = DataTools.Hardware.Usb.HidFeatures.OpenHid(_Model.Device);
            if (dev == IntPtr.Zero)
                return false;
            MemPtr mm = new MemPtr();
            mm.Alloc(5L);
            mm.ByteAt(0L) = (byte)_Code;
            mm.IntAtAbsolute(1L) = value;
            SetValueRet = DataTools.Win32.UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)5);
            mm.Free();
            DataTools.Hardware.Usb.HidFeatures.CloseHid(dev);
            if (SetValueRet)
            {
                _Value = value;
                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, _propEvent);
            }

            return SetValueRet;
        }

        /// <summary>
    /// Attempt to set a value or flag on the device.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool SetValue(short value)
        {
            bool SetValueRet = default;
            if (_byteLen < 2)
                return SetValue((byte)value);
            if (IsSettable == false)
                return false;
            if (Value == value)
                return false;
            var dev = DataTools.Hardware.Usb.HidFeatures.OpenHid(_Model.Device);
            if (dev == IntPtr.Zero)
                return false;
            MemPtr mm = new MemPtr();
            mm.Alloc(3L);
            mm.ByteAt(0L) = (byte)_Code;
            mm.ShortAtAbsolute(1L) = value;
            SetValueRet = DataTools.Win32.UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)3);
            mm.Free();
            DataTools.Hardware.Usb.HidFeatures.CloseHid(dev);
            if (SetValueRet)
            {
                _Value = value;
                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, _propEvent);
            }

            return SetValueRet;
        }

        /// <summary>
    /// Attempt to set a value or flag on the device.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool SetValue(byte value)
        {
            bool SetValueRet = default;
            if (IsSettable == false)
                return false;
            if (Value == value)
                return false;
            var dev = DataTools.Hardware.Usb.HidFeatures.OpenHid(_Model.Device);
            if (dev == IntPtr.Zero)
                return false;
            MemPtr mm = new MemPtr();
            mm.Alloc(2L);
            mm.ByteAt(0L) = (byte)_Code;
            mm.ByteAt(1L) = value;
            SetValueRet = DataTools.Win32.UsbLibHelpers.HidD_SetFeature(dev, mm.Handle, (int)2);
            mm.Free();
            DataTools.Hardware.Usb.HidFeatures.CloseHid(dev);
            if (SetValueRet)
            {
                _Value = value;
                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, _propEvent);
            }

            return SetValueRet;
        }

        /// <summary>
    /// Attempt to move this item to another property bag.
    /// </summary>
    /// <param name="bag">The destination property bag.</param>
    /// <returns>True if successful.</returns>
    /// <remarks></remarks>
        public bool MoveTo(TrippLitePropertyBag bag)
        {
            if (_Model is null || ReferenceEquals(bag, _Bag))
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
            string ToStringRet = default;
            ToStringRet = ToString(NumberFormat);
            return ToStringRet;
        }

        /// <summary>
    /// Converts this property into its string representation using the provided number format.
    /// </summary>
    /// <param name="numFmt">The standard format string used to format the numeric value of this property.</param>
    /// <returns></returns>
    /// <remarks></remarks>
        public string ToString(string numFmt)
        {
            string ToStringRet = default;
            ToStringRet = ToString(numFmt, false);
            return ToStringRet;
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
            string ToStringRet = default;
            ToStringRet = ((float)(Value * Multiplier)).ToString(numFmt) + (!suppressUnit ? " " + UnitInfo.UnitSymbol : "");
            return ToStringRet;
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
                PropertyChanged?.Invoke(this, _propEvent);
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _Model = null;
                _Bag = null;
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
        private TrippLiteUPS _Model;

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
                return _Model;
            }

            internal set
            {
                _Model = value;
            }
        }

        TrippLiteUPS IChild<TrippLiteUPS>.Parent
        {
            get => _Model;
            set => _Model = value;
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
            _Model = owner;
            if (autoPopulate)
            {
                var t = GetAllEnumVals<TrippLiteCodes>();
                int i;
                int c = t.Length - 1;
                var loopTo = c;
                for (i = 0; i <= loopTo; i++)
                    Add(new TrippLiteProperty(owner, t[i]));
            }
        }

        public bool MoveTo(TrippLiteProperty item, TrippLitePropertyBag newBag)
        {
            if (Contains(item) && !newBag.Contains(item))
            {
                Remove(item);
                newBag.Add(item);
                item._Model = newBag._Model;
                item._Bag = newBag;
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

        // TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        // Protected Overrides Sub Finalize()
        // ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        // Dispose(False)
        // MyBase.Finalize()
        // End Sub

        // This code added by Visual Basic to correctly implement the disposable pattern.
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

