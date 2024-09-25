using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibassCS.Enums
{
    public enum DefaultFontProvider
    {
        None = 0,
        AutoDetect = 1,
        CoreText,
        FontConfig,
        DirectWrite
    }
}
