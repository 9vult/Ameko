using Ffms2CS.Enums;
using System.Runtime.InteropServices;

namespace Ffms2CS.Structures
{
    /// <summary>
    /// Stores information about errors
    /// </summary>
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
