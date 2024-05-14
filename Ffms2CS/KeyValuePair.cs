using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyValuePair
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Key;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Value;
    }
}
