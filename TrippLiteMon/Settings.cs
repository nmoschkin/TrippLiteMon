
using DataTools.Win32.Memory;
using System.Drawing;

using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms.Design;
using System;
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
        public const string TrippLiteMonKey = @"Software\TrippLiteMon\TrippLiteMon";

        public static bool LoadLastConfigOnStartup
        {
            get
            {
                var ljc = LastJsonConfig;

                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                var res = ((int?)key.GetValue("LoadLastConfigOnStartup", string.IsNullOrEmpty(ljc) ? 0 : 1)) == 1;

                key.Close();

                return res;
            }
            set
            {
                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                key.SetValue("LastJsonConfig", value ? 1 : 0, RegistryValueKind.DWord);
                key.Close();
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

                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                var str = (string?)key.GetValue("LastJsonConfig", defFile);
                
                key.Close();

                return str ?? "";
            }
            set
            {
                var key = Registry.CurrentUser.CreateSubKey(TrippLiteMonKey + "", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
                key.SetValue("LastJsonConfig", value, RegistryValueKind.String);
                key.Close();
            }
        }

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
    #endregion


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


    #endregion

}

