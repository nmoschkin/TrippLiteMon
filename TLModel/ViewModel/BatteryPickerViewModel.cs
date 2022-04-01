using DataTools.Desktop;
using DataTools.Observable;
using DataTools.Win32.Usb;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TrippLite.ViewModel
{
    public class BatteryPickerViewModel : ObservableBase
    {
        private ObservableCollection<PowerDeviceIdEntry> deviceIds;

        private PowerDeviceIdEntry? selectedItem;

        private bool oneSelected;
        private bool selMulti = false;

        public BatteryPickerViewModel(bool multiSelect) : this()
        {
            MultiSelect = multiSelect;
        }

        public BatteryPickerViewModel() 
        {
            deviceIds = new ObservableCollection<PowerDeviceIdEntry>();

            var hids = HidDeviceInfo.EnumerateHidDevices(false, new[] { HidUsagePage.PowerDevice1, HidUsagePage.PowerDevice2 });

            foreach (var hid in hids)
            {
                var newDev = new PowerDeviceIdEntry(hid.ProductString, hid.DevicePath, hids.Length == 1) { Source = hid, Parent = this };
                deviceIds.Add(newDev);
            }

            CheckState();
        }
       
        protected internal void CheckState(PowerDeviceIdEntry? item = null)
        {
            OneSelected = ((deviceIds.Count((o) => o.Enabled)) > 0);

            if (item != null && item.Enabled && !selMulti)
            {
                int i, c = deviceIds.Count;

                for (i = 0; i < c; i++)
                {
                    if (item.DevicePath != deviceIds[i].DevicePath)
                    {
                        deviceIds[i].Enabled = false;
                    }
                    else if (deviceIds[i].Enabled != true)
                    {
                        deviceIds[i].Enabled = true;
                    }
                }
            }
        }

        public bool OneSelected
        {
            get => oneSelected;
            set
            {
                SetProperty(ref oneSelected, value);
            }
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
                        entry.Source = deviceIds[i].Source;
                        entry.Parent = this;

                        deviceIds[i] = entry;
                    }
                }        
            }

            CheckState();
        }


        public void SetEnabled(PowerDeviceIdEntry entry)
        {
            SetEnabled(new[] { entry });    
        }

        public void SetEnabled(string devicePath, bool enabled)
        {
            int i, c = deviceIds.Count;

            for (i = 0; i < c; i++)
            {
                if (deviceIds[i].DevicePath == devicePath)
                {
                    deviceIds[i].Enabled = enabled;

                    break;
                }
            }

            CheckState();
        }
        public IList<PowerDeviceIdEntry> GetEnabled(bool enabled = true)
        {
            return deviceIds.Where((d) => d.Enabled == enabled).ToArray();
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

        public ObservableCollection<PowerDeviceIdEntry> DeviceIds
        {
            get => deviceIds;
            private set
            {
                if (SetProperty(ref deviceIds, value))
                {
                    CheckState();
                }
            }
        }

        public bool MultiSelect
        {
            get => selMulti;
            set
            {
                if (SetProperty(ref selMulti, value) && selMulti == false)
                {
                    if (!(SelectedItem is null))
                    {
                        foreach (var dev in deviceIds)
                        {
                            if (dev != SelectedItem && dev.Enabled)
                            {
                                dev.Enabled = false;
                            }
                        }
                    }
                    else
                    {
                        int i, c = deviceIds.Count;
                        string? first = null;

                        for (i = 0; i < c; i++)
                        {
                            if (first is null && deviceIds[i].Enabled)
                            {
                                first = deviceIds[i].DevicePath;
                            }
                            
                            deviceIds[i].Enabled = (deviceIds[i].DevicePath == first);
                        }
                    }
                }
            }
        }

        public PowerDeviceIdEntry? SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }
    }
}
