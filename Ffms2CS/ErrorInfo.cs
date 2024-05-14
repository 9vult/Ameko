using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ErrorInfo
    {
        public Error ErrorType;
        public Error SubType;
        public int BufferSize;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Buffer;
    }
}
