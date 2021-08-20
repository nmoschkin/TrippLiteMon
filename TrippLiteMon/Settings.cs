using System;
using System.Drawing;

using DataTools.Win32.Memory;

using Microsoft.Win32;

namespace TrippLite
{

    #region Registry Settings

    /// <summary>
/// Specifies the last default window type of the active display.
/// </summary>
/// <remarks></remarks>
    public enum LastWindowType
    {
        /// <summary>
    /// The main window was the last to be displayed.
    /// </summary>
    /// <remarks></remarks>
        Main,

        /// <summary>
    /// The cool window was the last to be displayed.
    /// </summary>
    /// <remarks></remarks>
        Cool
    }

    public class Settings
    {
        public const string TrippLiteMonKey = @"Software\TrippLiteMon\TrippLiteMon";

        public static LastWindowType LastWindow
        {
            get
            {
                LastWindowType LastWindowRet = default;
                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                LastWindowRet = (LastWindowType)key.GetValue("LastWindow", 0);
                key.Close();
                return LastWindowRet;
            }

            set
            {
                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                key.SetValue("LastWindow", value, RegistryValueKind.DWord);
                key.Close();
            }
        }

        public static RectangleF PrimaryWindowBounds
        {
            get
            {
                RectangleF PrimaryWindowBoundsRet = default;
                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                var mm = new MemPtr(16);
                mm.FromByteArray((byte[])key.GetValue("PrimaryWindowBounds", (byte[])mm));
                key.Close();
                if (mm.SingleAt(2L) == 0f || mm.SingleAt(3L) == 0f)
                {
                    mm.Free();
                    return new RectangleF(0f, 0f, 0f, 0f);
                }

                PrimaryWindowBoundsRet = new RectangleF(mm.SingleAt(0L), mm.SingleAt(1L), mm.SingleAt(2L), mm.SingleAt(3L));
                mm.Free();
                return PrimaryWindowBoundsRet;
            }

            set
            {
                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                var mm = new MemPtr(16);
                mm.FromStruct(value);
                key.SetValue("PrimaryWindowBounds", mm.ToByteArray(0, 16), RegistryValueKind.Binary);
                key.Close();
                mm.Free();
            }
        }

        public static RectangleF CoolWindowBounds
        {
            get
            {
                RectangleF CoolWindowBoundsRet = default;
                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                var mm = new MemPtr(16);
                mm.FromByteArray((byte[])key.GetValue("CoolWindowBounds", (byte[])mm));
                key.Close();
                if (mm.SingleAt(2L) == 0f || mm.SingleAt(3L) == 0f)
                {
                    mm.Free();
                    return new RectangleF(0f, 0f, 0f, 0f);
                }

                CoolWindowBoundsRet = new RectangleF(mm.SingleAt(0L), mm.SingleAt(1L), mm.SingleAt(2L), mm.SingleAt(3L));
                mm.Free();
                return CoolWindowBoundsRet;
            }

            set
            {
                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                var mm = new MemPtr(16);
                mm.FromStruct(value);
                key.SetValue("CoolWindowBounds", mm.ToByteArray(0, 16), RegistryValueKind.Binary);
                key.Close();
                mm.Free();
            }
        }
    }
}

#endregion
