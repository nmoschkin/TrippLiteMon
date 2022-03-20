using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using DataTools.Scheduler;

namespace TrippLite
{

    public class TrippLitePropertyBagViewModel : DependencyCollectionBase<TrippLitePropertyViewModel, TrippLiteViewModel>
    {
        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TrippLitePropertyViewModel GetPropertyByCode(TrippLiteCodes c)
        {
            foreach (var v in this)
            {
                if (v.Code == c)
                    return v;
            }

            return null;
        }

        public TrippLitePropertyBagViewModel(TrippLiteViewModel owner) : base(owner)
        {
        }
    }

}
