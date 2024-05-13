using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ErrorInfo
    {
        public int ErrorType;
        public int SubType;
        public int BufferSize;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Buffer;
    }
}
