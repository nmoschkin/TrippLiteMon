using DataTools.Observable;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrippLite
{
    /// <summary>
    /// Represents a Power Device Registry Entry
    /// </summary>
    public class PowerDeviceIdEntry : ObservableBase
    {
        string name;
        bool enabled;

        /// <summary>
        /// Gets or sets the friendly name of the device.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the device is enabled (being observed by the application.)
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                SetProperty(ref enabled, value);
            }
        }

        /// <summary>
        /// Gets the internal device path.
        /// </summary>
        public string DevicePath { get; }

        public PowerDeviceIdEntry(string name, string path, bool enabled)
        {
            Name = this.name = name;
            DevicePath = path;
            Enabled = this.enabled = enabled;
        }

        public PowerDeviceIdEntry(string name, string path)
        {
            Name = this.name = name;
            DevicePath = path;
            Enabled = this.Enabled = false;
        }

        public PowerDeviceIdEntry(string path, bool enabled)
        {
            DevicePath = path;
            Name = this.name = "Power Device";
            Enabled = this.Enabled = enabled;
        }

        public PowerDeviceIdEntry(string path)
        {
            DevicePath = path;
            Name = this.name = "Power Device";
            Enabled = this.enabled = false;
        }

        /// <summary>
        /// Returns the Name and DevicePath separated by a Tab character.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}\t{DevicePath}";
        }

        /// <summary>
        /// Parse a string from the registry into a new instance.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="enabled"></param>
        /// <returns>A new instance of <see cref="PowerDeviceIdEntry"/>.</returns>
        /// <remarks>
        /// The value is stored as a pair of strings separated by the Tab (\t) character.
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public static PowerDeviceIdEntry Parse(string s, bool enabled = true)
        {
            var sp = s.Split('\t');
            if (sp.Length == 2)
            {
                return new PowerDeviceIdEntry(sp[0], sp[1], enabled);
            }
            else if (sp.Length == 1)
            {
                return new PowerDeviceIdEntry(sp[0], enabled);
            }
            else throw new ArgumentException("String could not be parsed.");
        }
    }

}
