using TrippLite.Globalization;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static DataTools.Text.TextTools;

namespace TrippLite.Codes
{
    public static class ExtendedDescriptions
    {
        static Dictionary<string, string> descriptions;

        public static IReadOnlyDictionary<string, string> Descriptions => descriptions;

        public static string? FindDescriptionByName(string name)
        {
            name = NoSpace(name).ToLower();

            foreach (var desc in descriptions)
            {
                var search = NoSpace(desc.Key).ToLower();
                if (search == name) return desc.Value;
            }

            return null;
        }

        static ExtendedDescriptions()
        {
            descriptions = JsonConvert.DeserializeObject<Dictionary<string, string>>(AppResources.ExtendedInfo);
        }

    }
}
