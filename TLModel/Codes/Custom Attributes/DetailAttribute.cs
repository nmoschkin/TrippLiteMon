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
    /// Provides a detailed description.
    /// </summary>
    /// <remarks></remarks>
    public class DetailAttribute : Attribute
    {
        private string detail;

        public string Detail
        {
            get
            {
                return detail;
            }
        }

        public DetailAttribute(string detail)
        {
            this.detail = detail;
        }
    }
}
