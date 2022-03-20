using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using DataTools.Win32.Memory;
using DataTools.Win32.Usb;

using static TrippLite.TrippLiteCodeUtility;

namespace TrippLite
{
    /// <summary>
    /// Represents a collection of all USB HID feature properties for a Tripp Lite Smart Battery
    /// </summary>
    /// <remarks></remarks>
    public class TrippLitePropertyBag : ObservableCollection<TrippLiteProperty>, IDisposable, IChild<TrippLiteUPS>
    {
        private TrippLiteUPS model;

        /// <summary>
        /// Gets the owner TrippLiteUPS object for this property bag.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TrippLiteUPS Parent
        {
            get
            {
                return model;
            }

            internal set
            {
                model = value;
            }
        }

        TrippLiteUPS IChild<TrippLiteUPS>.Parent
        {
            get => model;
            set => model = value;
        }

        public TrippLiteProperty FindProperty(TrippLiteCodes c)
        {
            foreach (var fp in this)
            {
                if (fp.Code == c)
                    return fp;
            }

            return null;
        }

        /// <summary>
        /// Initialize a new TrippLitePropertyBag and automatically populate the property bag with known property values from the TrippLiteCodes enumeration.
        /// </summary>
        /// <param name="owner">The TrippLiteUPS object upon which to initialize.</param>
        /// <remarks></remarks>
        public TrippLitePropertyBag(TrippLiteUPS owner) : this(owner, true)
        {
        }

        /// <summary>
        /// Initialize a new TrippLitePropertyBag
        /// </summary>
        /// <param name="owner">The TrippLiteUPS object upon which to initialize.</param>
        /// <param name="autoPopulate">Specify whether to automatically populate the property bag with known property values from the TrippLiteCodes enumeration.</param>
        /// <remarks></remarks>
        public TrippLitePropertyBag(TrippLiteUPS owner, bool autoPopulate) : base()
        {
            model = owner;
        
            if (autoPopulate)
            {
                var t = GetAllEnumVals<TrippLiteCodes>();
            
                int i;
                int c = t.Length;
                
                for (i = 0; i < c; i++)
                    Add(new TrippLiteProperty(owner, t[i]));
            }
        }

        public bool MoveTo(TrippLiteProperty item, TrippLitePropertyBag newBag)
        {
            if (Contains(item) && !newBag.Contains(item))
            {
                Remove(item);
            
                newBag.Add(item);
                
                item.model = newBag.model;
                item.propBag = newBag;
                
                return true;
            }

            return false;
        }

        internal void SignalRefresh()
        {
            foreach (var p in this)
                p.SignalRefresh();
        }

        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    foreach (var x in this)
                        x.Dispose();
                
                    Clear();
                    
                    GC.Collect(0);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
