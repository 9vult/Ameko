using System.Runtime.InteropServices;

namespace Ffms2CS.Structures
{
    /// <summary>
    /// A simple key-value pair
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyValuePair
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Key;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Value;
    }
}
