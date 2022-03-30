﻿using DataTools.Observable;
using DataTools.Win32.Usb;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrippLite.ViewModel
{
    public class BatteryPickerViewModel : ObservableBase
    {
        private ObservableCollection<HidPowerDeviceInfo> devices;
        private ObservableCollection<PowerDeviceIdEntry> deviceIds;

        public BatteryPickerViewModel()
        {
            devices = new ObservableCollection<HidPowerDeviceInfo>();
            deviceIds = new ObservableCollection<PowerDeviceIdEntry>();

            var hids = HidDeviceInfo.EnumerateHidDevices(false, new[] { HidUsagePage.PowerDevice1, HidUsagePage.PowerDevice2 });

            foreach (var hid in hids)
            {
                devices.Add(HidPowerDeviceInfo.CreateFromHidDevice(hid));
                deviceIds.Add(new PowerDeviceIdEntry(hid.ProductString, hid.DevicePath));
            }
        }

        public IList<PowerDeviceIdEntry> GetEnabled(bool enabled = true)
        {
            return deviceIds.Where((d) => d.Enabled == enabled).ToArray();
        }

        public void SetEnabled(IList<PowerDeviceIdEntry> entries)
        {
            int i, c = deviceIds.Count;

            foreach(var entry in entries)
            {
                for(i = 0; i < c; i++)
                {
                    if (deviceIds[i].DevicePath == entry.DevicePath)
                    {
                        deviceIds[i] = entry;
                        break;
                    }
                }        
            }
        }

        public void SetEnabled(PowerDeviceIdEntry entry)
        {
            SetEnabled(new[] { entry });    
        }

        public bool GetEnabled(PowerDeviceIdEntry entry)
        {
            int i, c = deviceIds.Count;

            for (i = 0; i < c; i++)
            {
                if (deviceIds[i].DevicePath == entry.DevicePath)
                {
                    return deviceIds[i].Enabled;
                }
            }

            return false;
        }

        public bool GetEnabled(string devicePath)
        {
            int i, c = deviceIds.Count;

            for (i = 0; i < c; i++)
            {
                if (deviceIds[i].DevicePath == devicePath)
                {
                    return deviceIds[i].Enabled;
                }
            }

            return false;
        }

        public void SetEnabled(string devicePath, bool enabled)
        {
            int i, c = deviceIds.Count;

            for (i = 0; i < c; i++)
            {
                if (deviceIds[i].DevicePath == devicePath)
                {
                    var entry = deviceIds[i];

                    entry.Enabled = enabled;
                    deviceIds[i] = entry;

                    return;
                }
            }
        }

        public ObservableCollection<PowerDeviceIdEntry> DeviceIds
        {
            get => deviceIds;
            set
            {
                SetProperty(ref deviceIds, value);
            }
        }

        public ObservableCollection<HidPowerDeviceInfo> Devices
        {
            get => devices;
            protected set
            {
                SetProperty(ref devices, value);
            }
        }
    }
}
