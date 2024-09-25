using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibassCS.Enums
{
    public enum YCbCrMatrix
    {
        Default = 0,
        Unknown,
        None,
        BT601_TV,
        BT601_PC,
        BT709_TV,
        BT709_PC,
        SMPTE240M_TV,
        SMPTE240M_PC,
        FCC_TV,
        FCC_PC
    }
}
