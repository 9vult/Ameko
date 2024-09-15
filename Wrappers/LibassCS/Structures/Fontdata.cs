using System.Runtime.InteropServices;

namespace LibassCS.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Fontdata
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Data;
        UIntPtr Size;
    }
}
