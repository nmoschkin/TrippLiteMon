using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

using DataTools.Win32.Memory;
using DataTools.SystemInformation;
using System.Runtime.InteropServices;

using static TrippLite.TrippLiteCodeUtility;
using DataTools.Win32;
using DataTools.Win32.Usb;


namespace TrippLite
{

    #region TrippLite

    public class TrippLiteUPS : System.Runtime.ConstrainedExecution.CriticalFinalizerObject, INotifyPropertyChanged, IDisposable
    {

        /// <summary>
        /// Initialize a new TrippLiteUPS object.
        /// </summary>
        /// <param name="connect"></param>
        /// <remarks></remarks>
        public TrippLiteUPS(bool connect = true)
        {
            propBag = new TrippLitePropertyBag(this);

            if (connect)
                Connect();
        }

        protected MemPtr mm;
        protected IntPtr hHid;
        
        protected HidPowerDeviceInfo powerDevice;
        
        protected long[] values = new long[256];
        
        protected bool connected = false;
        protected bool isTrippLite = false;
        
        protected PowerStates powerState = PowerStates.Uninitialized;
        
        protected TrippLitePropertyBag propBag;
        
        protected long bufflen = 65L;

        public static int DefaultRetries { get; set; } = 2;

        public static int DefaultRetryDelay { get; set; } = 1000;


        public event PropertyChangedEventHandler PropertyChanged;
        public event PowerStateChangedEventHandler PowerStateChanged;

        public delegate void PowerStateChangedEventHandler(object sender, PowerStateChangedEventArgs e);

        #region Static

        /// <summary>
        /// Returns a list of all TrippLite devices.
        /// </summary>
        /// <param name="forceRefreshCache">Whether or not to refresh the internal HID power device cache.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static HidDeviceInfo[] FindAllTrippLiteDevices(bool forceRefreshCache = false)
        {
            var lOut = new List<HidDeviceInfo>();

            HidDeviceInfo[] devs;
            devs = HidFeatures.HidDevicesByUsage(HidUsagePage.PowerDevice1);

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

            var shex = new SHELLEXECUTEINFO();

            shex.cbSize = Marshal.SizeOf(shex);
            shex.fMask = (User32.SEE_MASK_UNICODE | User32.SEE_MASK_ASYNCOK | User32.SEE_MASK_FLAG_DDEWAIT);
            shex.hWnd = hwndOwner;
            shex.hInstApp = Process.GetCurrentProcess().Handle;
            shex.nShow = User32.SW_SHOW;

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
            User32.ShellExecuteEx(ref shex);
        }

        /// <summary>
        /// Connect to a TrippLite battery.
        /// </summary>
        /// <param name="device">Optional manually-selected device.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Connect(HidDeviceInfo? device = null)
        {
            if (disposedValue) return false;

            HidDeviceInfo[] devs;

            int i = 0;

            if (device is null)
            {
                do
                {
                    devs = HidFeatures.HidDevicesByUsage(HidUsagePage.PowerDevice1);

                    if (devs is object)
                    {
                        foreach (var dev in devs)
                        {
                            if (dev.Vid == 0x09AE)
                            {
                                powerDevice = HidPowerDeviceInfo.CreateFromHidDevice(dev);
                                isTrippLite = true;

                                break;
                            }
                        }

                        if (powerDevice is null)
                            powerDevice = HidPowerDeviceInfo.CreateFromHidDevice(devs[0]);

                        connected = true;

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
                if (device.Vid == 0x09AE)
                {
                    isTrippLite = true;
                }
                else
                {
                    isTrippLite = false;
                }

                powerDevice = HidPowerDeviceInfo.CreateFromHidDevice(device);
                connected = true;
            }

            if (connected)
            {
                hHid = HidFeatures.OpenHid(powerDevice);
                mm.AllocZero(bufflen);
            }

            var result = connected & hHid.ToInt64() > 0L;

            return result;
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

                HidFeatures.CloseHid(hHid);

                hHid = (IntPtr)(-1);

                connected = false;
                powerDevice = null;
            }
            catch 
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
                return connected;
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
                return powerState;
            }

            protected set
            {
                if (powerState != value)
                {
                    var os = powerState;
                    powerState = value;
                    PowerStateChanged?.Invoke(this, new PowerStateChangedEventArgs(os, powerState));
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
                return GetEnumAttrVal<DescriptionAttribute, string, PowerStates>(powerState, "Description");
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
                return GetEnumAttrVal<DetailAttribute, string, PowerStates>(powerState, "Detail");
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
                return "SMART" + propBag.FindProperty(TrippLiteCodes.VARATING).GetValue() + "LCDx";
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
                return propBag;
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
                return isTrippLite;
            }
        }

        /// <summary>
        /// Contains detailed Operating System-Reported information about the device.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public HidPowerDeviceInfo Device
        {
            get
            {
                return powerDevice;
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
            return RefreshData(DefaultRetries, DefaultRetryDelay, dep);
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
                b = InternalRefresh(dep);
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
            propBag.SignalRefresh();
        }

        #endregion

        #region Protected Methods

        static int lps;

        protected bool InternalRefresh(System.Windows.DependencyObject dep = null)
        {
            if (!connected)
                return false;

            int v;
            int ret = 0;

            double max;

            int lpsMax = 0;

            double min;

            var volt = default(double);

            bool involtRet = false;
            var activeProps = new List<TrippLiteProperty>();

            try
            {
                foreach (var prop in propBag)
                {
                    mm.LongAtAbsolute(1L) = 0L;
                    mm.ByteAt(0) = (byte)prop.Code;

                    var res = HidFeatures.GetHIDFeature(hHid, (int)prop.Code, (int)bufflen);

                    if (res != null)
                    {
                        v = res.intVal;
                        switch (prop.Code)
                        {
                            case TrippLiteCodes.InputVoltage:
                                volt = v * prop.Multiplier;
                                involtRet = true;

                                break;

                            case TrippLiteCodes.OutputLoad:
                                if (v > 100 || v < 0)
                                    v = (int)prop.Value;

                                break;
                        }

                        if (prop.value != v || prop.value == -1)
                        {
                            prop.value = v;
                            activeProps.Add(prop);
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
                min = propBag.FindProperty(TrippLiteCodes.LowVoltageTransfer).Value;
                max = propBag.FindProperty(TrippLiteCodes.HighVoltageTransfer).Value;

                switch (volt)
                {
                    case 0d:
                        if (PowerState != PowerStates.Battery)
                        {
                            if (lps >= lpsMax)
                            {
                                if (dep is object)
                                {
                                    dep.Dispatcher.BeginInvoke(() => PowerState = PowerStates.Battery);
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

                    case var @case when @case <= min:
                        if (PowerState != PowerStates.BatteryTransferLow)
                        {
                            if (lps >= lpsMax)
                            {
                                if (dep is object)
                                {
                                    dep.Dispatcher.BeginInvoke(() => PowerState = PowerStates.BatteryTransferLow);
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

                    case var case1 when case1 >= max:
                        if (PowerState != PowerStates.BatteryTransferHigh)
                        {
                            if (lps >= lpsMax)
                            {
                                if (dep is object)
                                {
                                    dep.Dispatcher.BeginInvoke(() => PowerState = PowerStates.BatteryTransferHigh);
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

                    default:
                        if (PowerState != PowerStates.Utility)
                        {
                            if (lps >= lpsMax)
                            {
                                if (dep is object)
                                {
                                    dep.Dispatcher.BeginInvoke(() => PowerState = PowerStates.Utility);
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

            if (dep is object)
            {
                foreach (var prop in activeProps)
                    dep.Dispatcher.BeginInvoke(() => prop.SignalRefresh());
            }
            else
            {
                foreach (var prop in activeProps)
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
        private PowerStates oldState;
        private PowerStates newState;

        public PowerStates OldState
        {
            get
            {
                return oldState;
            }
        }

        public PowerStates NewState
        {
            get
            {
                return newState;
            }
        }

        public PowerStateChangedEventArgs(PowerStates o, PowerStates n)
        {
            oldState = o;
            newState = n;
        }
    }
}
#endregion
