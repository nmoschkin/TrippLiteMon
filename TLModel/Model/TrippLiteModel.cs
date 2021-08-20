using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

using DataTools.Win32.Memory;
using DataTools.SystemInformation;
using System.Runtime.InteropServices;

using static TrippLite.TrippLiteCodeUtility;

namespace TrippLite
{

    #region TrippLite

    public class TrippLiteUPS : System.Runtime.ConstrainedExecution.CriticalFinalizerObject, INotifyPropertyChanged, IDisposable
    {

        #endregion

        #region Constructor

        /// <summary>
    /// Initialize a new TrippLiteUPS object.
    /// </summary>
    /// <param name="connect"></param>
    /// <remarks></remarks>
        public TrippLiteUPS(bool connect = true)
        {
            _Bag = new TrippLitePropertyBag(this);
            if (connect)
                Connect();
        }

        protected MemPtr mm;
        protected IntPtr _hid;
        protected DataTools.Hardware.Usb.HidDeviceInfo _Power;
        protected long[] _Values = new long[256];
        protected bool _conn = false;
        protected bool _isTL = false;
        protected PowerStates _PowerState = PowerStates.Uninitialized;
        protected TrippLitePropertyBag _Bag;
        protected long _buffLen = 65L;

        public static int DefaultRetries { get; set; } = 2;
        public static int DefaultRetryDelay { get; set; } = 1000;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PowerStateChangedEventHandler PowerStateChanged;

        public delegate void PowerStateChangedEventHandler(object sender, PowerStateChangedEventArgs e);

        #region Shared

        /// <summary>
    /// Returns a list of all TrippLite devices.
    /// </summary>
    /// <param name="forceRefreshCache">Whether or not to refresh the internal HID power device cache.</param>
    /// <returns></returns>
    /// <remarks></remarks>
        public static DataTools.Hardware.Usb.HidDeviceInfo[] FindAllTrippLiteDevices(bool forceRefreshCache = false)
        {
            var lOut = new List<DataTools.Hardware.Usb.HidDeviceInfo>();
            DataTools.Hardware.Usb.HidDeviceInfo[] devs;
            devs = DataTools.Hardware.Usb.HidFeatures.HidDevicesByUsage(DataTools.Hardware.Usb.HidUsagePage.PowerDevice1);
            if (devs is object)
            {
                foreach (var dev in devs)
                {
                    if (dev.HidManufacturer == "Tripp Lite")
                    {
                        lOut.Add(dev);
                    }
                }
            }

            return lOut.ToArray();
        }

        /// <summary>
    /// List all TrippLite devices by serial number, only.
    /// </summary>
    /// <param name="forceRefreshCache">Whether or not to refresh the internal HID power device cache.</param>
    /// <returns></returns>
    /// <remarks></remarks>
        public static string[] ListTrippLiteDevicesBySerialNumber(bool forceRefreshCache = false)
        {
            var devs = FindAllTrippLiteDevices(forceRefreshCache);
            var l = new List<string>();
            foreach (var d in devs)
                l.Add(d.SerialNumber);
            return l.ToArray();
        }


        /// <summary>
    /// Opens the system power options.
    /// Opens either the control panel or the Windows 10 settings panel.
    /// </summary>
    /// <param name="hwndOwner">Optional pointer to the parent window (default is null).</param>
    /// <param name="win10">Open the Windows 10 settings panel, if available (default is True).</param>
        public static void OpenSystemPowerOptions(IntPtr hwndOwner = default, bool win10 = true)
        {
            bool do10 = win10 & SysInfo.OSInfo.IsWindows10;
            var shex = new DataTools.Win32.SHELLEXECUTEINFO();
            shex.cbSize = Marshal.SizeOf(shex);
            shex.fMask = (uint)(DataTools.Win32.User32.SEE_MASK_UNICODE | DataTools.Win32.User32.SEE_MASK_ASYNCOK | DataTools.Win32.User32.SEE_MASK_FLAG_DDEWAIT);
            shex.hWnd = hwndOwner;
            shex.hInstApp = Process.GetCurrentProcess().Handle;
            shex.nShow = DataTools.Win32.User32.SW_SHOW;
            if (do10)
            {
                shex.lpFile = "ms-settings:powersleep";
            }
            else
            {
                shex.lpDirectory = @"::{26EE0668-A00A-44D7-9371-BEB064C98683}\0";
                shex.lpFile = @"::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{025A5937-A6BE-4686-A844-36FE4BEC8B6D}";
            }

            shex.lpVerb = "";
            DataTools.Win32.User32.ShellExecuteEx(ref shex);
        }

        /// <summary>
    /// Connect to a TrippLite battery.
    /// </summary>
    /// <param name="device">Optional manually-selected device.</param>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool Connect(DataTools.Hardware.Usb.HidDeviceInfo device = null)
        {

            // ' we won't be connecting if it's already disposed.
            if (disposedValue)
                return false;
            DataTools.Hardware.Usb.HidDeviceInfo[] devs;
            int i = 0;
            if (device is null)
            {
                do
                {
                    devs = DataTools.Hardware.Usb.HidFeatures.HidDevicesByUsage(DataTools.Hardware.Usb.HidUsagePage.PowerDevice1);
                    if (devs is object)
                    {
                        foreach (var dev in devs)
                        {
                            if ((int)dev.Vid == 0x09AE)
                            {
                                _Power = dev;
                                _isTL = true;
                                break;
                            }
                        }

                        if (_Power is null)
                            _Power = devs[0];
                        _conn = true;
                        break;
                    }

                    i += 1;
                    if (i >= DefaultRetries)
                        break;
                    Thread.Sleep(DefaultRetryDelay);
                }
                while (true);
            }
            else
            {
                if ((int)device.Vid == 0x09AE)
                {
                    _isTL = true;
                }
                else
                {
                    _isTL = false;
                }

                _Power = device;
                _conn = true;
            }

            i = 0;
            if (_conn)
            {
                _hid = DataTools.Hardware.Usb.HidFeatures.OpenHid(_Power);
                mm.AllocZero(_buffLen);
            }

            return _conn & _hid.ToInt64() > 0L;
        }

        /// <summary>
    /// Disconnect the device and free all resources.
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool Disconnect()
        {
            try
            {
                mm.Free();
                DataTools.Hardware.Usb.HidFeatures.CloseHid(_hid);
                _hid = (IntPtr)(-1);
                _conn = false;
                _Power = null;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Public Properties

        /// <summary>
    /// Gets a value indicating whether or not the current object is connected to a battery.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool Connected
        {
            get
            {
                return _conn;
            }
        }

        /// <summary>
    /// Returns the current power state of the Tripp Lite device.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public PowerStates PowerState
        {
            get
            {
                return _PowerState;
            }

            protected set
            {
                if (_PowerState != value)
                {
                    var os = _PowerState;
                    _PowerState = value;
                    PowerStateChanged?.Invoke(this, new PowerStateChangedEventArgs(os, _PowerState));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PowerState"));
                }
            }
        }

        /// <summary>
    /// Gets the description of the current power state.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public string PowerStateDescription
        {
            get
            {

                // ' retrieve the 'Description' property (as string) of the DescriptionAttribute
                // ' associated with the specified field of the PowerStates enumeration.
                return GetEnumAttrVal<DescriptionAttribute, string, PowerStates>(_PowerState, "Description");
            }
        }

        /// <summary>
    /// Gets the detailed description of the current power state.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public string PowerStateDetail
        {
            get
            {
                return GetEnumAttrVal<DetailAttribute, string, PowerStates>(_PowerState, "Detail");
            }
        }

        /// <summary>
    /// Gets the title of the device.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public string Title
        {
            get
            {
                return Device.HidManufacturer + " " + Device.ClassName;
            }
        }

        /// <summary>
    /// Gets the model of the SMART Tripp Lite UPS
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public string ModelId
        {
            get
            {
                return "SMART" + _Bag.FindProperty(TrippLiteCodes.VARATING).GetValue() + "LCDx";
            }
        }

        /// <summary>
    /// Contains all properties exposed by the Tripp Lite HID Power Interface
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public TrippLitePropertyBag PropertyBag
        {
            get
            {
                return _Bag;
            }
        }

        /// <summary>
    /// Indicates whether this power interface is attached to a Tripp Lite power device.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool IsTrippLite
        {
            get
            {
                return _isTL;
            }
        }

        /// <summary>
    /// Contains detailed Operating System-Reported information about the device.
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
        public DataTools.Hardware.Usb.HidDeviceInfo Device
        {
            get
            {
                return _Power;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
    /// Refresh the data from the device, using the default number of tries and the default delay.
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool RefreshData(System.Windows.DependencyObject dep = null)
        {
            bool RefreshDataRet = default;
            RefreshDataRet = RefreshData(DefaultRetries, DefaultRetryDelay, dep);
            return RefreshDataRet;
        }

        /// <summary>
    /// Refresh the data from the device.
    /// </summary>
    /// <param name="tries">The number of times to attempt to collect data.</param>
    /// <param name="delay">The interval between each try, in milliseconds.</param>
    /// <returns></returns>
    /// <remarks></remarks>
        public bool RefreshData(int tries, int delay, System.Windows.DependencyObject dep = null)
        {
            bool b;
            int i = 0;
            do
            {
                b = _internalRefresh(dep);
                if (b == true)
                    return true;
                i += 1;
                if (i == tries)
                    return false;
                Thread.Sleep(delay);
            }
            while (true);
        }

        /// <summary>
    /// Signals the system to signal refresh events for all properties regardless of their changed state.
    /// Property events will always signal if a property changes.
    /// To refresh the status of the device, use the RefreshData() method.
    /// (This method does not trigger the PowerStateChanged event)
    /// </summary>
    /// <remarks></remarks>
        public void SignalRefresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PowerState"));
            _Bag.SignalRefresh();
        }

        #endregion

        #region Protected Methods
        
        static int lps;

        protected bool _internalRefresh(System.Windows.DependencyObject dep = null)
        {
            if (!_conn)
                return false;
            int v;
            int ret = 0;
            double max;
            int lpsMax = 0;
            double min;
            var volt = default(double);
            bool involtRet = false;
            var cex = new List<TrippLiteProperty>();
            try
            {
                foreach (var prop in _Bag)
                {
                    mm.LongAtAbsolute(1L) = 0L;
                    mm.ByteAt(0) = (byte)prop.Code;
                    var res = DataTools.Hardware.Usb.HidFeatures.GetHIDFeature(_hid, (int)prop.Code, (int)_buffLen);
                    if (res != null)
                    {
                        v = res.intVal;
                        switch (prop.Code)
                        {
                            case TrippLiteCodes.InputVoltage:
                                {
                                    volt = v * prop.Multiplier;
                                    involtRet = true;
                                    break;
                                }

                            case TrippLiteCodes.OutputLoad:
                                {
                                    if (v > 100 || v < 0)
                                        v = (int)prop.Value;
                                    break;
                                }
                        }

                        if (prop._Value != v || prop._Value == -1)
                        {
                            prop._Value = v;
                            cex.Add(prop);
                        }
                    }
                }
            }
            catch (ThreadAbortException thx)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
                // '
                // ' Check for power failure
            };

            if (involtRet == true)
            {
                min = _Bag.FindProperty(TrippLiteCodes.LowVoltageTransfer).Value;
                max = _Bag.FindProperty(TrippLiteCodes.HighVoltageTransfer).Value;
                switch (volt)
                {
                    case 0d:
                        {
                            if (PowerState != PowerStates.Battery)
                            {
                                if (lps >= lpsMax)
                                {
                                    if (dep is object)
                                    {
                                        dep.Dispatcher.BeginInvoke(new Action(() => PowerState = PowerStates.Battery));
                                    }
                                    else
                                    {
                                        PowerState = PowerStates.Battery;
                                    }

                                    lps = 0;
                                }
                            }
                            else
                            {
                                lps += 1;
                            }

                            break;
                        }

                    case var @case when @case <= min:
                        {
                            if (PowerState != PowerStates.BatteryTransferLow)
                            {
                                if (lps >= lpsMax)
                                {
                                    if (dep is object)
                                    {
                                        dep.Dispatcher.BeginInvoke(new Action(() => PowerState = PowerStates.BatteryTransferLow));
                                    }
                                    else
                                    {
                                        PowerState = PowerStates.BatteryTransferLow;
                                    }

                                    lps = 0;
                                }
                            }
                            else
                            {
                                lps += 1;
                            }

                            break;
                        }

                    case var case1 when case1 >= max:
                        {
                            if (PowerState != PowerStates.BatteryTransferHigh)
                            {
                                if (lps >= lpsMax)
                                {
                                    if (dep is object)
                                    {
                                        dep.Dispatcher.BeginInvoke(new Action(() => PowerState = PowerStates.BatteryTransferHigh));
                                    }
                                    else
                                    {
                                        PowerState = PowerStates.BatteryTransferHigh;
                                    }

                                    lps = 0;
                                }
                            }
                            else
                            {
                                lps += 1;
                            }

                            break;
                        }

                    default:
                        {
                            if (PowerState != PowerStates.Utility)
                            {
                                if (lps >= lpsMax)
                                {
                                    if (dep is object)
                                    {
                                        dep.Dispatcher.BeginInvoke(new Action(() => PowerState = PowerStates.Utility));
                                    }
                                    else
                                    {
                                        PowerState = PowerStates.Utility;
                                    }

                                    lps = 0;
                                }
                            }
                            else
                            {
                                lps += 1;
                            }

                            break;
                        }
                }
            }

            if (dep is object)
            {
                foreach (var prop in cex)
                    dep.Dispatcher.BeginInvoke(new Action(() => prop.SignalRefresh()));
            }
            else
            {
                foreach (var prop in cex)
                    prop.SignalRefresh();
            }

            return true;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Disconnect();
            }

            disposedValue = true;
        }

        ~TrippLiteUPS()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

    #endregion

    #region PowerStateChangedEventArgs

    public class PowerStateChangedEventArgs : EventArgs
    {
        private PowerStates _oldState;
        private PowerStates _newState;

        public PowerStates OldState
        {
            get
            {
                return _oldState;
            }
        }

        public PowerStates NewState
        {
            get
            {
                return _newState;
            }
        }

        public PowerStateChangedEventArgs(PowerStates o, PowerStates n)
        {
            _oldState = o;
            _newState = n;
        }
    }
}
#endregion
