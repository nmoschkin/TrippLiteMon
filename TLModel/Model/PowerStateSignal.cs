using DataTools.Essentials.Observable;
using DataTools.Win32;
using DataTools.Win32.Usb;
using DataTools.Win32.Usb.Power;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrippLite
{
    public class PowerStateSignal : ObservableBase, IDisposable
    {
        private PowerStates currentState = PowerStates.Uninitialized;
        private HidPowerDeviceInfo device;

        private Thread? updater = null;
        private object lockobj = new object();
        private int updateFreq = 500;

        public event EventHandler<PowerStateChangedEventArgs>? PowerStateChanged;

        public HidPowerDeviceInfo Device => device;

        private HidUsageInfo? huacPresent = null;
        private HidUsageInfo? huawaitPower = null;
        private HidUsageInfo? hubuck = null;
        private HidUsageInfo? huboost = null;
        private HidUsageInfo? huvoor = null;

        private bool acPresent = false;
        private bool awaitPower = false;
        private bool buck = false;
        private bool boost = false;
        private bool voor = false;
        private bool disposedValue;
        
        public bool UpdaterRunning => updater != null;

        public PowerStates CurrentState
        {
            get => currentState;
            private set
            {
                SetProperty(ref currentState, value);
            }
        }

        public bool ACPresent
        {
            get => acPresent;
            private set
            {
                SetProperty(ref acPresent, value);
            }
        }

        public bool AwaitingPower
        {
            get => awaitPower;
            set
            {
                SetProperty(ref awaitPower, value);
            }
        }

        /// <summary>
        /// Voltage Too High
        /// </summary>
        public bool Buck
        {
            get => buck;
            private set
            {
                SetProperty(ref buck, value);
            }
        }

        /// <summary>
        /// Voltage Too Low
        /// </summary>
        public bool Boost
        {
            get => boost;
            private set
            {
                SetProperty(ref boost, value);  
            }
        }

        /// <summary>
        /// Voltage Out Of Range
        /// </summary>
        public bool VoltageOutOfRange
        {
            get => voor;
            private set
            {
                SetProperty(ref voor, value);
            }
        }



        /// <summary>
        /// Gets or sets the update frequency in milliseconds.
        /// </summary>
        public int UpdateFrequency
        {
            get => updateFreq;
            set
            {
                lock (lockobj)
                {
                    SetProperty(ref updateFreq, value); 
                }
            }
        }

        public PowerStateSignal(HidPowerDeviceInfo device)
        {
            this.device = device;
            
            huacPresent = device.LookupValue((byte)HidBatteryUsageCode.ACPresent);
            huawaitPower = device.LookupValue((byte)HidPowerUsageCode.AwaitingPower);
            hubuck = device.LookupValue((byte)HidPowerUsageCode.Buck);
            huboost = device.LookupValue((byte)HidPowerUsageCode.Boost);
            huvoor = device.LookupValue((byte)HidPowerUsageCode.VoltageOutOfRange);

            DeterminePowerState();
        }

        public bool StartUpdater()
        {
            lock (lockobj)
            {
                if (updater != null) return false;

                updater = new Thread(UpdaterFunc);
                updater.Start();
                updater.IsBackground = true;

                return true;
            }
        }

        public bool StopUpdater()
        {
            lock (lockobj)
            {
                if (updater == null) return false;
                updater = null;

                return true;
            }
        }

        private async void UpdaterFunc()
        {
            try
            {
                while (true)
                {
                    lock (lockobj)
                    {
                        if (updater == null)
                        {
                            return;
                        }
                    }

                    DeterminePowerState();
            
                    await Task.Delay(UpdateFrequency);
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch 
            {
                return;
            }
        }

        public void DeterminePowerState()
        {
            lock (lockobj)
            {
                bool ch = false;

                if (!device.IsHidOpen)
                {
                    ch = true;
                    device.OpenHid();
                }

                if (huacPresent != null)
                    ACPresent = (bool?)device.RetrieveValue(huacPresent) ?? false;

                if (hubuck != null)
                    Buck = (bool?)device.RetrieveValue(hubuck) ?? false;

                if (huboost != null)
                    Boost = (bool?)device.RetrieveValue(huboost) ?? false;

                if (huawaitPower != null)
                    AwaitingPower = (bool?)device.RetrieveValue(huawaitPower) ?? false;

                if (huvoor != null)
                    VoltageOutOfRange = (bool?)device.RetrieveValue(huvoor) ?? false;

                if (ch)
                {
                    device.CloseHid();
                }

                ACPresent = ACPresent | !AwaitingPower;

                PowerStates newState;

                if (Boost)
                {
                    newState = PowerStates.BatteryTransferLow;
                }
                else if (Buck)
                {
                    newState = PowerStates.BatteryTransferHigh;
                }
                else if (ACPresent)
                {
                    newState = PowerStates.Utility;
                }
                else
                {
                    newState = PowerStates.Battery;
                }

                if (newState != CurrentState)
                {
                    var ostate = CurrentState;
                    CurrentState = newState;

                    _ = Task.Run(() => PowerStateChanged?.Invoke(this, new PowerStateChangedEventArgs(ostate, newState)));
                }
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                StopUpdater();
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~PowerStateSignal()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
