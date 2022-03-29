using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrippLite
{
    public interface IChildOf<T>
    {
        T Parent { get; set; }
    }

}
