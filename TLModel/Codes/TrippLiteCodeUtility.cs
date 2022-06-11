using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using TrippLite.Globalization;

using static TrippLite.TrippLiteCodeUtility;

namespace TrippLite
{
    static class TrippLiteCodeUtility
    {

        /// <summary>
        /// Gets the specified property of the specified attribute type for the specified enumeration member.
        /// </summary>
        /// <typeparam name="T">The attribute type from which to retrieve the property.</typeparam>
        /// <typeparam name="U">The type of the attribute property to retrieve.</typeparam>
        /// <typeparam name="V">The enum type.</typeparam>
        /// <param name="o">The enum va ue for which to retrieve the attribute.</param>
        /// <param name="valueName">The name of the property inside the Attribute object to retrieve.</param>
        /// <param name="b">Optional binding flags for member searching.</param>
        /// <returns>A value of type U that represents the value of the property of the attribute of the member of the enumeration.</returns>
        /// <remarks></remarks>
        public static U GetEnumAttrVal<T, U, V>(V o, string valueName, BindingFlags b = BindingFlags.Public | BindingFlags.Static) where T : Attribute
        {
            T da;
            
            var fi = o.GetType().GetFields(b);

            foreach (var fe in fi)
            {
                if ((o.ToString() ?? "") == (fe.GetValue(o).ToString() ?? ""))
                {
                    da = (T)fe.GetCustomAttribute(typeof(T));
                    if (da is object)
                    {
                        var f = da.GetType().GetProperty(valueName);
                        return (U)f.GetValue(da);
                    }
                }
            }

            return default;
        }

        public static List<KeyValuePair<T, string>> GetAllDescriptions<T>(T o, BindingFlags b = BindingFlags.Public | BindingFlags.Static)
        {
            DescriptionAttribute da;
            
            var fi = o.GetType().GetFields(b);
            var l = new List<KeyValuePair<T, string>>();
            
            KeyValuePair<T, string> kv;
            
            foreach (var fe in fi)
            {
                da = (DescriptionAttribute)fe.GetCustomAttribute(typeof(DescriptionAttribute));
                if (da is object)
                {
                    kv = new KeyValuePair<T, string>((T)fe.GetValue(o), da.Description);
                    l.Add(kv);
                }
            }

            return l;
        }

        public static List<KeyValuePair<T, string>> GetAllSymbols<T>(T o, BindingFlags b = BindingFlags.Public | BindingFlags.Static)
        {
            UnitSymbolAttribute da;
            
            var fi = o.GetType().GetFields(b);
            var l = new List<KeyValuePair<T, string>>();
            
            KeyValuePair<T, string> kv;
            
            foreach (var fe in fi)
            {
                da = (UnitSymbolAttribute)fe.GetCustomAttribute(typeof(UnitSymbolAttribute));
            
                if (da is object)
                {
                    kv = new KeyValuePair<T, string>((T)fe.GetValue(o), da.Symbol);
                    l.Add(kv);
                }
            }

            return l;
        }

        public static List<KeyValuePair<T, double>> GetAllMeasureUnits<T>(T o, BindingFlags b = BindingFlags.Public | BindingFlags.Static)
        {
            MeasureUnitAttribute da;

            var fi = o.GetType().GetFields(b);

            var l = new List<KeyValuePair<T, double>>();

            KeyValuePair<T, double> kv;

            foreach (var fe in fi)
            {
                da = (MeasureUnitAttribute)fe.GetCustomAttribute(typeof(MeasureUnitAttribute));
                if (da is object)
                {
                    kv = new KeyValuePair<T, double>((T)fe.GetValue(o), (double)da.Unit);
                    l.Add(kv);
                }
            }

            return l;
        }

        public static List<KeyValuePair<T, double>> GetAllMultipliers<T>(T o, BindingFlags b = BindingFlags.Public | BindingFlags.Static)
        {
            MultiplierAttribute da;

            var fi = o.GetType().GetFields(b);

            var l = new List<KeyValuePair<T, double>>();

            KeyValuePair<T, double> kv;

            foreach (var fe in fi)
            {
                da = (MultiplierAttribute)fe.GetCustomAttribute(typeof(MultiplierAttribute));
                if (da is object)
                {
                    kv = new KeyValuePair<T, double>((T)fe.GetValue(o), da.Value);
                    l.Add(kv);
                }
            }

            return l;
        }

        public static T[] GetAllEnumVals<T>()
        {
            var fi = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);

            T[] x;

            int i = 0;

            x = new T[fi.Length];

            foreach (var fe in fi)
            {
                x[i] = (T)fe.GetValue(x[i]);
                i += 1;
            }

            return x;
        }

        /// <summary>
        /// Get the localized battery property description.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetLocalizedBatteryPropertyDescription(string name)
        {
            return AppResources.ResourceManager.GetString(name) ?? name;
        }
    }
}
