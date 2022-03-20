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
