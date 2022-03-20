using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using DataTools.Scheduler;

namespace TrippLite
{

    /// <summary>
    /// The object that represents the TrippLite property ViewModel
    /// </summary>
    /// <remarks></remarks>
    public class TrippLitePropertyViewModel : INotifyPropertyChanged, IDisposable, IChild<TrippLiteViewModel>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private TrippLiteProperty prop;

        private TrippLiteProperty PropSync
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return prop;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (prop != null)
                {
                    prop.PropertyChanged -= _Prop_PropertyChanged;
                }

                prop = value;
                if (prop != null)
                {
                    prop.PropertyChanged += _Prop_PropertyChanged;
                }
            }
        }

        private bool changed;
        private bool isProminent;

        private TrippLiteViewModel owner;

        private string displayValue = null;

        private PropertyChangedEventArgs dispEvent = new PropertyChangedEventArgs(nameof(DisplayValue));

        /// <summary>
        /// Creates a new object with the specified base object and owner.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="owner"></param>
        /// <remarks></remarks>
        internal TrippLitePropertyViewModel(TrippLiteProperty p, TrippLiteViewModel owner)
        {
            if (p is null)
            {
                throw new ArgumentException();
            }

            this.owner = owner;
            PropSync = p;

            if (PropSync.value == -1)
            {
                PropSync.value = 0L;
            }

            displayValue = PropSync.ToString(PropSync.NumberFormat, true);
        }

        /// <summary>
        /// Gets the parent of this object (IChild)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TrippLiteViewModel Parent
        {
            get
            {
                return owner;
            }
            internal set
            {
                owner = value;
            }
        }

        TrippLiteViewModel IChild<TrippLiteViewModel>.Parent
        {
            get => owner;
            set => owner = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this property will actively trigger change events.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsActiveProperty
        {
            get
            {
                return PropSync.IsActiveProperty;
            }
            set
            {
                PropSync.IsActiveProperty = value;

                if (value)
                    SetDisplayValue();
            }
        }

        /// <summary>
        /// Indicates whether or not this property belongs to the prominent list.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Prominent
        {
            get
            {
                return isProminent;
            }
            internal set
            {
                isProminent = value;
            }
        }

        /// <summary>
        /// Returns the raw value of the HID property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long Value
        {
            get
            {
                return PropSync.Value;
            }
        }

        /// <summary>
        /// Returns the HID feature code that the value of this property represents.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TrippLiteCodes Code
        {
            get
            {
                return PropSync.Code;
            }
        }

        /// <summary>
        /// Returns the header or title of this property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Header
        {
            get
            {
                return PropSync.Description;
            }
        }

        /// <summary>
        /// Returns the formatted display value of this property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string DisplayValue
        {
            get
            {
                return displayValue;
            }
        }

        /// <summary>
        /// Set the display value of the current value of the property.
        /// </summary>
        /// <remarks></remarks>
        public void SetDisplayValue()
        {
            if (isProminent)
            {
                displayValue = PropSync.ToString(PropSync.NumberFormat, true);
            }
            else
            {
                displayValue = PropSync.ToString();
            }
        }

        /// <summary>
        /// Returns the formatted display value of this property.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ToString()
        {
            return displayValue;
        }

        private void _Prop_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!PropSync.IsActiveProperty)
                return;
            SetDisplayValue();
            PropertyChanged?.Invoke(this, dispEvent);
        }

        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                PropSync.Dispose();
            }

            disposedValue = true;
        }

        ~TrippLitePropertyViewModel()
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

        public static explicit operator ulong(TrippLitePropertyViewModel val)
        {
            return (ulong)val.PropSync.value;
        }

        public static implicit operator long(TrippLitePropertyViewModel val)
        {
            return val.PropSync.value;
        }

        public static explicit operator uint(TrippLitePropertyViewModel val)
        {
            return (uint)val.PropSync.value;
        }

        public static explicit operator int(TrippLitePropertyViewModel val)
        {
            return (int)val.PropSync.value;
        }

    }



}
