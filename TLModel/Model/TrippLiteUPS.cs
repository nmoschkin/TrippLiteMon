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
using DataTools.Win32.Usb.Power;

using System.IO;
using DataTools.MessageBoxEx;
using System.Linq;

namespace TrippLite
{
    public delegate void PowerStateChangedEventHandler(object sender, PowerStateChangedEventArgs e);

    public class TrippLiteUPS : System.Runtime.ConstrainedExecution.CriticalFinalizerObject, INotifyPropertyChanged, IDisposable
    {


        /// <summary>
        /// Initialize a new TrippLiteUPS object.
        /// </summary>
        /// <param name="connect"></param>
        /// <remarks></remarks>

        #region Protected Fields

        protected long bufflen = 65L;

        protected bool connected = false;

        protected int deviceIndex;

        protected bool isTrippLite = false;

        protected MemPtr mm;

        protected HidPowerDeviceInfo powerDevice;

        protected PowerStates powerState = PowerStates.Uninitialized;

        protected TrippLitePropertyBag propBag;

        protected PowerStateSignal? stateMonitor = null;

        protected long[] values = new long[256];

        #endregion Protected Fields

        #region Private Fields

        static int lps;

        private PowerDeviceIdEntry? deviceIdEntry = null;

        private bool disposedValue;

        #endregion Private Fields

        #region Public Constructors

        public TrippLiteUPS() : this(true)
        {
        }

        /// <summary>
        /// Initialize a new TrippLiteUPS object.
        /// </summary>
        /// <param name="connect"></param>
        /// <remarks></remarks>
        public TrippLiteUPS(bool connect) : this(connect, 0)
        {

        }


        /// <summary>
        /// Initialize a new TrippLiteUPS object.
        /// </summary>
        /// <param name="connect"></param>
        /// <remarks></remarks>
        public TrippLiteUPS(bool connect, int deviceIndex)
        {
            this.deviceIndex = deviceIndex;

            if (connect)
            {
                try
                {
                    Connect();
                }
                catch(Exception ex)
                {
                    MessageBoxEx.Show(
                        $"Error Opening HID Battery: {ex.Message}", 
                        "Initialization Failure", 
                        MessageBoxExType.OK, 
                        MessageBoxExIcons.Exclamation);
                }

            }

        }

        #endregion Public Constructors

        #region Private Destructors

        ~TrippLiteUPS()
        {
            Dispose(false);
        }

        #endregion Private Destructors

        #region Public Events

        public event PowerStateChangedEventHandler? PowerStateChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public static int DefaultRetries { get; set; } = 2;

        public static int DefaultRetryDelay { get; set; } = 1000;
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
        /// Gets the model of the SMART Tripp Lite UPS
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ModelId
        {
            get
            {
                return "SMART" + propBag.FindProperty(BatteryPropertyCodes.VARATING).GetValue() + "LCDx";
            }
        }

        public string Name
        {
            get => deviceIdEntry?.Name ?? powerDevice?.ProductString ?? powerDevice?.DevicePath ?? string.Empty;
            set
            {
                if (deviceIdEntry == null && powerDevice != null)
                {
                    deviceIdEntry = new PowerDeviceIdEntry(powerDevice.DevicePath, value);
                }

                if (deviceIdEntry != null)
                {
                    deviceIdEntry.Name = value;
                }
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

        #endregion Public Properties

        #region Public Methods

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

            if (do10)
            {
                ShellExecuteHelper.Execute("ms-settings:powersleep", "", "");
            }
            else
            {
                ShellExecuteHelper.Execute(@"::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{025A5937-A6BE-4686-A844-36FE4BEC8B6D}", "", @"::{26EE0668-A00A-44D7-9371-BEB064C98683}\0");
            }

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
            if (connected) return true;

            HidDeviceInfo? dev;
            PowerDeviceIdEntry? hidcfg = null;

            int i = 0;

            if (device is null)
            {

                try
                {
                    hidcfg = Settings.PowerDevices[deviceIndex];
                    deviceIdEntry = hidcfg;
                }
                catch
                {
                    return false;
                }

                
                do
                {
                    if (deviceIdEntry == null)
                    {
                        dev = HidFeatures.HidDevicesByUsage(HidUsagePage.PowerDevice1)[0];

                    }
                    else
                    {
                        dev = HidFeatures.HidDevicesByUsage(HidUsagePage.PowerDevice1).Where((d) => d.DevicePath == hidcfg.DevicePath).FirstOrDefault();
                    }

                    if (dev is object)
                    {
                        powerDevice = HidPowerDeviceInfo.CreateFromHidDevice(dev);

                        BatteryPropertyCodes.Initialize(powerDevice);

                        propBag = new TrippLitePropertyBag(this);
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
                connected &= powerDevice.OpenHid();
                mm.AllocZero(bufflen);
            }

            var result = connected & powerDevice.IsHidOpen;

            if (!connected) throw new FileLoadException("Could not connect to HID battery.");

            stateMonitor = new PowerStateSignal(powerDevice);
            stateMonitor.PowerStateChanged += StateMonitor_PowerStateChanged;
        
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

                stateMonitor.PowerStateChanged -= StateMonitor_PowerStateChanged;
                powerDevice?.CloseHid();
                stateMonitor?.Dispose();

                stateMonitor = null;
                connected = false;
                powerDevice = null;
                deviceIdEntry = null;
                propBag = null;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        #endregion Public Methods

        #region Protected Methods

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Disconnect();
            }

            disposedValue = true;
        }

        protected bool InternalRefresh(System.Windows.DependencyObject dep = null)
        {
            if (!connected)
                return false;

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

                    var res = powerDevice.HidGetFeature((byte)prop.Code, out int v);

                    if (res)
                    {
                        if (prop.Code == BatteryPropertyCodes.InputVoltage)
                        {
                            volt = v * prop.Multiplier;
                            involtRet = true;

                        }
                        else if (prop.Code == BatteryPropertyCodes.OutputLoad)
                        {
                            if (v > 100 || v < 0)
                                v = (int)prop.Value;
                        }

                        if (prop.value != v || prop.value == -1)
                        {
                            prop.value = v;
                            activeProps.Add(prop);
                        }
                    }

                }
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            };

            stateMonitor?.DeterminePowerState();

            if (dep is object)
            {
                foreach (var prop in activeProps)
                    dep.Dispatcher.BeginInvoke(() => prop.SignalRefresh());
            }
            else
            {
                foreach (var prop in activeProps)
                {
                    prop.SignalRefresh();
                }
            }

            return true;
        }

        #endregion Protected Methods

        #region Private Methods

        private void StateMonitor_PowerStateChanged(object? sender, PowerStateChangedEventArgs e)
        {
            PowerState = e.NewState;
            PowerStateChanged?.Invoke(this, e);
        }

        #endregion Private Methods

        // To detect redundant calls
    }
}
