using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    public class Ffms2CS
    {
        public delegate int TIndexCallback(long current, CatalogLocation total, IntPtr icPrivate);
    }
}
