using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibassCS.Structures
{
    public struct Library
    {
        string FontsDir;
        bool ExtractFonts;
        IntPtr StyleOverrides;
        Fontdata Fontdata;
        UIntPtr NumFontdata;
    }
}
