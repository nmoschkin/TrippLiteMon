using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTools.Observable;

using DataTools.Win32.Usb;

namespace TrippLite.Property_Bag
{
    public class HIDPropertyViewModel : ObservableBase
    {

        HidDeviceInfo? powerDev;

        HidUsageInfo? usageInfo;

        public string PropertyName
        {
            get => usageInfo?.UsageTypeDescription ?? "";
        }

        public HidDeviceInfo? PowerDevice
        {
            get => powerDev;
        }

        public HidUsageInfo? UsageInfo
        {
            get => usageInfo;
            set
            {
                SetProperty(ref usageInfo, value);
            }
        }

        public HIDPropertyViewModel(HidDeviceInfo powerDevice, HidUsageInfo usage)
        {
            powerDev = powerDevice;
            usageInfo = usage;
        }

        public static Dictionary<string, HIDPropertyViewModel> QueryDevice(HidDeviceInfo device)
        {
            var result = new Dictionary<string, HIDPropertyViewModel>();


            return result;
        }


    }
}
