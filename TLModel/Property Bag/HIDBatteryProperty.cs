using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTools.Observable;

using DataTools.Win32.Usb;

namespace TrippLite.Property_Bag
{
    public class HIDBatteryProperty : ObservableBase
    {

        HidPowerDeviceInfo? powerDev;

        HidPowerUsageInfo? usageInfo;

        public string PropertyName
        {
            get => usageInfo?.UsageTypeDescription ?? "";
        }

        public HidPowerDeviceInfo? PowerDevice
        {
            get => powerDev;
        }

        public HidPowerUsageInfo? PowerUsageInfo
        {
            get => usageInfo;
            set
            {
                SetProperty(ref usageInfo, value);
            }
        }

        public HIDBatteryProperty(HidPowerDeviceInfo powerDevice, HidPowerUsageInfo usage)
        {
            powerDev = powerDevice;
            usageInfo = usage;
        }





    }
}
