using DataTools.Observable;
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

        public BatteryPickerViewModel()
        {
            devices = new ObservableCollection<HidPowerDeviceInfo>();
            var hids = HidDeviceInfo.EnumerateHidDevices(false, new[] { HidUsagePage.PowerDevice1, HidUsagePage.PowerDevice2 });

            foreach (var hid in hids)
            {
                devices.Add(HidPowerDeviceInfo.CreateFromHidDevice(hid));
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
