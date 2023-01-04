using DataTools.Win32.Memory;

using Microsoft.Win32;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

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
        public const string ConfigRootKey = @"Software\TrippLiteMon\TrippLiteMon";

        public static bool LoadLastConfigOnStartup
        {
            get
            {
                using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                {
                    var ljc = LastJsonConfig;
                    var res = ((int?)key.GetValue("LoadLastConfigOnStartup", string.IsNullOrEmpty(ljc) ? 0 : 1)) == 1;

                    return res;
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                {
                    key.SetValue("LastJsonConfig", value ? 1 : 0, RegistryValueKind.DWord);
                }
            }
        }

        public static string LastJsonConfig
        {
            get
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + AppDomain.CurrentDomain.FriendlyName;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var defFile = dir + "\\settings.json";

                using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                {
                    var str = (string?)key.GetValue("LastJsonConfig", defFile);
                    return str ?? "";
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                {
                    key.SetValue("LastJsonConfig", value, RegistryValueKind.String);
                }
            }
        }

        public static LastWindowType LastWindow
        {
            get
            {
                using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                {
                    return (LastWindowType)key.GetValue("LastWindow", 0);
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                {
                    key.SetValue("LastWindow", value, RegistryValueKind.DWord);
                }
            }
        }

        public static RectangleF PrimaryWindowBounds
        {
            get
            {
                using (var mm = new SafePtr(16L))
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                    {
                        mm.FromByteArray((byte[])key.GetValue("PrimaryWindowBounds", (byte[])mm));
                        if (mm.SingleAt(2L) == 0f || mm.SingleAt(3L) == 0f)
                        {
                            mm.Free();
                            return new RectangleF(0f, 0f, 0f, 0f);
                        }

                        return new RectangleF(mm.SingleAt(0L), mm.SingleAt(1L), mm.SingleAt(2L), mm.SingleAt(3L));
                    }
                }
            }
            set
            {
                using (var mm = new SafePtr(16L))
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                    {
                        mm.FromStruct(value);
                        key.SetValue("PrimaryWindowBounds", mm.ToByteArray(0, 16), RegistryValueKind.Binary);
                    }
                }
            }
        }

        public static RectangleF CoolWindowBounds
        {
            get
            {
                using (var mm = new SafePtr(16L))
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                    {
                        mm.FromByteArray((byte[])key.GetValue("CoolWindowBounds", (byte[])mm));

                        if (mm.SingleAt(2L) == 0f || mm.SingleAt(3L) == 0f)
                        {
                            mm.Free();
                            return new RectangleF(0f, 0f, 0f, 0f);
                        }

                        return new RectangleF(mm.SingleAt(0L), mm.SingleAt(1L), mm.SingleAt(2L), mm.SingleAt(3L));
                    }
                }
            }
            set
            {
                using (var mm = new SafePtr(16L))
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                    {
                        mm.FromStruct(value);
                        key.SetValue("CoolWindowBounds", mm.ToByteArray(0, 16), RegistryValueKind.Binary);
                    }
                }
            }
        }

        public static PowerDeviceIdEntry[] PowerDevices
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + @"\PowerDevices", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
                {
                    string[] entries = key.GetValueNames();

                    List<PowerDeviceIdEntry> recents = new List<PowerDeviceIdEntry>();

                    int idx;
                    int ncount = 0;
                    int gcount = 0;

                    foreach (string entry in entries)
                    {
                        if (entry == null) continue;

                        if (int.TryParse(entry, out idx))
                        {
                            ncount++;

                            if (key.GetValue(entry) is string rfObj)
                            {
                                gcount++;
                                recents.Add(PowerDeviceIdEntry.Parse(rfObj));
                            }
                        }
                    }

                    if ((gcount != ncount))
                        SetPowerDeviceIdList(recents.ToArray());

                    return recents.ToArray();
                }
            }
            set
            {
                SetPowerDeviceIdList(value);
            }
        }

        private static void SetPowerDeviceIdList(PowerDeviceIdEntry[] deviceIds)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(ConfigRootKey + @"\PowerDevices", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None))
            {
                string[] values = key.GetValueNames();

                int c = 255;
                int i;
                int n;

                foreach (string value in values) key.DeleteValue(value);

                if (deviceIds == null || deviceIds.Length == 0)
                {
                    key.Close();
                    return;
                }

                c = c < deviceIds.Length ? c : deviceIds.Length;
                n = 0;

                for (i = 0; i <= c - 1; i++)
                {
                    if (!deviceIds[i].Enabled) continue;
                    key.SetValue(n++.ToString(), deviceIds[i].ToString(), RegistryValueKind.String);
                }
            }
        }
    }

    #endregion Registry Settings

    #region JSON Settings

    internal class ColorConverter : JsonConverter<System.Windows.Media.Color>
    {
        public override System.Windows.Media.Color ReadJson(JsonReader reader, Type objectType, System.Windows.Media.Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                var re = new Regex(@"#^([A-Fa-f0-9]{2}])([A-Fa-f0-9]{2}])([A-Fa-f0-9]{2}])([A-Fa-f0-9]{2}])$");
                var test = re.Match(s);

                if (test.Success)
                {
                    var a = byte.Parse(test.Groups[0].Value, System.Globalization.NumberStyles.HexNumber);
                    var r = byte.Parse(test.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                    var g = byte.Parse(test.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                    var b = byte.Parse(test.Groups[3].Value, System.Globalization.NumberStyles.HexNumber);

                    return new System.Windows.Media.Color()
                    {
                        A = a,
                        B = r,
                        G = g,
                        R = b
                    };
                }
                else
                {
                    re = new Regex(@"^#([A-Fa-f0-9]{2}])([A-Fa-f0-9]{2}])([A-Fa-f0-9]{2}])$");
                    test = re.Match(s);

                    if (test.Success)
                    {
                        byte a = 255;
                        var r = byte.Parse(test.Groups[0].Value, System.Globalization.NumberStyles.HexNumber);
                        var g = byte.Parse(test.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                        var b = byte.Parse(test.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);

                        return new System.Windows.Media.Color()
                        {
                            A = a,
                            B = r,
                            G = g,
                            R = b
                        };
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override void WriteJson(JsonWriter writer, System.Windows.Media.Color value, JsonSerializer serializer)
        {
            var s = $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}";
            writer.WriteValue(s);
        }
    }

    public class ModeSettings
    {
        [JsonProperty("leftHandProperty")]
        public string? LeftHandProperty { get; set; }

        [JsonProperty("leftHandLoadBar")]
        public string? LeftHandLoadBar { get; set; }

        [JsonProperty("rightHandProp1")]
        public string? RightHandProperty1 { get; set; }

        [JsonProperty("rightHandProp2")]
        public string? RightHandProperty2 { get; set; }

        [JsonProperty("background")]
        [JsonConverter(typeof(ColorConverter))]
        public System.Windows.Media.Color WindowBackground { get; set; }

        [JsonProperty("foreground")]
        [JsonConverter(typeof(ColorConverter))]
        public System.Windows.Media.Color WindowForeground { get; set; }

        [JsonProperty("opacity")]
        public double WindowOpacity { get; set; }
    }

    public class JsonSettings
    {
        [JsonProperty("utility")]
        public ModeSettings UtilityPower { get; set; } = new ModeSettings();

        [JsonProperty("blackout")]
        public ModeSettings BatteryPowerBlackout { get; set; } = new ModeSettings();

        [JsonProperty("brownout")]
        public ModeSettings UtilityPowerBrownout { get; set; } = new ModeSettings();

        [JsonProperty("surge")]
        public ModeSettings UtilityPowerSurge { get; set; } = new ModeSettings();

        [JsonIgnore]
        public string? FileName { get; set; } = Settings.LastJsonConfig;

        public static JsonSettings? LoadSettings(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            var json = File.ReadAllText(fileName);
            var results = JsonConvert.DeserializeObject<JsonSettings>(json) ?? throw new InvalidDataException();

            results.FileName = fileName;
            Settings.LastJsonConfig = fileName;

            return results;
        }

        public static JsonSettings? LoadSettings()
        {
            var dlg = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Javascript Object Notation Files|*.json|(All Files)|*.*",
                CheckFileExists = true,
                DefaultExt = "json",
                FileName = Settings.LastJsonConfig
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return LoadSettings(dlg.FileName);
            }
            else
            {
                return null;
            }
        }

        public void SaveSettings(string fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(this));
            FileName = fileName;
            Settings.LastJsonConfig = FileName;
        }

        public void SaveSettings(bool showDialog = true)
        {
            if (FileName == null)
            {
                if (!showDialog) throw new ArgumentNullException(nameof(FileName));

                var dlg = new System.Windows.Forms.SaveFileDialog()
                {
                    Filter = "Javascript Object Notation Files|*.json|(All Files)|*.*",
                    OverwritePrompt = true,
                    DefaultExt = "json",
                    FileName = Settings.LastJsonConfig
                };

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileName = dlg.FileName;
                }
                else
                {
                    return;
                }
            }

            SaveSettings(FileName);
        }
    }

    #endregion JSON Settings
}