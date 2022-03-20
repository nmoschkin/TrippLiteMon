using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using static TrippLite.TrippLiteCodeUtility;

namespace TrippLite
{
    /// <summary>
    /// Represents a unit of measure.
    /// </summary>
    /// <remarks></remarks>
    public class MeasureUnit
    {
        protected string unitSymbol;
        protected string name;
        protected MeasureUnitTypes unitType;

        protected static Collection<MeasureUnit> units;

        public static MeasureUnit FindUnit(MeasureUnitTypes t)
        {
            foreach (var mu in units)
            {
                if (mu.UnitType == t)
                    return mu;
            }

            return null;
        }

        protected MeasureUnit(MeasureUnitTypes unit, string name, string symbol)
        {
            unitSymbol = symbol;
            unitType = unit;

            this.name = name;
        }

        static MeasureUnit()
        {
            units = new Collection<MeasureUnit>();
            var fi = typeof(MeasureUnitTypes).GetFields(BindingFlags.Public | BindingFlags.Static);

            MeasureUnit mu;
            var mt = MeasureUnitTypes.Amp;

            var kvc = GetAllDescriptions<MeasureUnitTypes>(MeasureUnitTypes.Amp);
            var mvc = GetAllSymbols<MeasureUnitTypes>(MeasureUnitTypes.Amp);

            int i = 0;

            int c = kvc.Count;

            KeyValuePair<MeasureUnitTypes, string> kv;

            int d;

            if (mvc is null)
                d = -1;
            else
                d = 0;

            string sym;
            string desc;
                        
            for (i = 0; i < c; i++)
            {
                kv = kvc[i];
                sym = "";

                if (d != -1)
                {
                    if (mvc[d].Key == kvc[i].Key)
                    {
                        sym = mvc[d].Value;

                        d += 1;

                        if (d >= mvc.Count)
                            d = -1;
                    }
                }

                desc = kvc[i].Value;
                mt = kv.Key;

                mu = new MeasureUnit(mt, desc, sym);

                units.Add(mu);
            }
        }

        public static Collection<MeasureUnit> Units
        {
            get => units;
        }

        public string UnitSymbol
        {
            get => unitSymbol;
        }

        public string Name
        {
            get => name;
        }

        public MeasureUnitTypes UnitType
        {
            get => unitType;
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}
