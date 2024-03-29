using DataTools.Win32.Memory;

using System;
using System.ComponentModel;

using static TrippLite.TrippLiteCodeUtility;

namespace TrippLite
{
    /// <summary>
    /// Encapsulates a USB HID feature property for a Tripp Lite Smart Battery.
    /// </summary>
    /// <remarks></remarks>
    public sealed class TrippLiteProperty : INotifyPropertyChanged, IDisposable, IChildOf<TrippLiteUPS>
    {
        #region Internal Fields

        internal TrippLiteUPS model;

        internal TrippLitePropertyBag propBag;

        internal long value = -1;

        #endregion Internal Fields

        #region Private Fields

        private string customDesc;

        private ushort byteLen = 4;

        private bool disposedValue;

        private bool isSettable;

        private BatteryPropertyCode propCode;

        private PropertyChangedEventArgs propEvent = new PropertyChangedEventArgs("Value");

        #endregion Private Fields

        #region Internal Constructors

        /// <summary>
        /// Initialize a new TrippLiteProperty
        /// </summary>
        /// <param name="owner">The TrippLiteUPS model object that will own this property.</param>
        /// <param name="c">The property code.</param>
        /// <remarks></remarks>
        internal TrippLiteProperty(TrippLiteUPS owner, BatteryPropertyCode c)
        {
            model = owner;
            propCode = c;
            byteLen = ByteLength;
        }

        #endregion Internal Constructors

        #region Private Destructors

        ~TrippLiteProperty()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);
        }

        #endregion Private Destructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

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
                ushort bl = model.PropertyMap.GetValueAttribute<ByteLengthAttribute, ushort>(propCode, "Length");

                if (bl == 0)
                    bl = 4;

                return bl;
            }
        }

        /// <summary>
        /// Gets the Tripp Lite property code.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public BatteryPropertyCode Code
        {
            get => propCode;
        }

        /// <summary>
        /// Gets the description of the property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Description
        {
            get => customDesc ?? GetLocalizedBatteryPropertyDescription(model.PropertyMap.GetNameFromCode(propCode));
            set
            {
                if (customDesc != value)
                {
                    customDesc = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
                }
            }
        }

        /// <summary>
        /// Indicates whether this is a property that raises change events.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsActiveProperty { get; set; } = true;

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
        /// Specify whether or not the property object will call to the device, directly, to retrieve (or set) the value of its property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool LiveInterface { get; set; } = false;

        /// <summary>
        /// Gets the multiplier of the property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public double Multiplier
        {
            get => model.PropertyMap.GetValueAttribute<MultiplierAttribute, double>(propCode, "Value");
        }

        // End Get
        // End Property
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
        /// Gets the hard-coded number format for this property type.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string NumberFormat
        {
            get => model.PropertyMap.GetValueAttribute<NumberFormatAttribute, string>(propCode, "Format");
        }

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

        TrippLiteUPS IChildOf<TrippLiteUPS>.Parent
        {
            get => model;
            set => model = value;
        }

        // Public ReadOnly Property RawValue As Byte()
        // Get
        /// <summary>
        /// Gets the unit of the property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public MeasureUnitTypes Unit
        {
            get => model.PropertyMap.GetValueAttribute<MeasureUnitAttribute, MeasureUnitTypes>(propCode, "Unit");
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

        #endregion Public Properties

        #region Public Methods

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

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Attempts to retrieve the value directly from the device.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public long GetValue()
        {
            long res = 0L;
            model.Device.HidGetFeature(propCode, out res);
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
        /// Attempts to set a value or flag on the device using raw byte data.
        /// The number of bytes in the byte array must exactly match the byte length of the property.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool SetValue(byte[] value)
        {
            var res = model.Device.HidSetFeature(propCode, value);

            if (res)
            {
                using (var mm = new SafePtr(value))
                {
                    if (value.Length >= 8)
                    {
                        this.value = mm.LongAt(0);
                    }
                    else
                    {
                        this.value = mm.IntAt(0);
                    }
                }

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
        public bool SetValue(long value)
        {
            var res = model.Device.HidSetFeature(propCode, value);

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
            var res = model.Device.HidSetFeature(propCode, value);

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
            var res = model.Device.HidSetFeature(propCode, value);

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
            var res = model.Device.HidSetFeature(propCode, value);

            if (res)
            {
                this.value = value;

                if (IsActiveProperty)
                    PropertyChanged?.Invoke(this, propEvent);
            }

            return res;
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

        #endregion Public Methods

        #region Internal Methods

        internal void SignalRefresh()
        {
            if (IsActiveProperty)
            {
                PropertyChanged?.Invoke(this, propEvent);
            }
        }

        #endregion Internal Methods

        // To detect redundant calls

        #region Private Methods

        // IDisposable
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                model = null;
                propBag = null;
            }

            disposedValue = true;
        }

        #endregion Private Methods
    }
}