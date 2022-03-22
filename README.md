# TrippLite Desktop Monitor

This is a native Windows desktop app that reads the settings from various USB HID-enabled UPS Battery system devices by TrippLite, APC, et al.

The original goal of this project was just to support the one TrippLite battery, but I have recently finished adding some universal support by querying HID usage pages (for which I have developed a pretty rich library over at https://github.com/nmoschkin/DataTools). 

## Update March 21, 2022

This is a work in progress.  Eventually I will probably rename the project to __BatteryMon__, and attempt to possibly re-engineer it into WinUI 3/Reunion.

Until then, it will be in WPF.  There will be a packaged release, soon.  Whether it is the __TrippLiteMon__ project or the new __BatteryMon__ project is TBD.

