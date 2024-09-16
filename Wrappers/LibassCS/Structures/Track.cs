using LibassCS.Enums;
using System.Runtime.InteropServices;

namespace LibassCS.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Track
    {
        public int NumStyles;
        public int MaxStyles;
        public int NumEvents;
        public int MaxEvents;
        public IntPtr Styles;
        public IntPtr Events;
        [MarshalAs(UnmanagedType.LPStr)] public string StyleFormat;
        [MarshalAs(UnmanagedType.LPStr)] public string EventFormat;
        public TrackType TrackType;
        public int PlayResX;
        public int PlayResY;
        public double Timer;
        public int WrapStyle;
        [MarshalAs(UnmanagedType.Bool)] public bool ScaledBorderAndShadow;
        public int Kerning;
        [MarshalAs(UnmanagedType.LPStr)] public string Language;
        public YCbCrMatrix YCbCrMatrix;
        public int DefaultStyle;
        [MarshalAs(UnmanagedType.LPStr)] public string Name;
        public IntPtr Library;
        public IntPtr ParserPriv;
        public int LayoutResX;
        public int LayoutResY;
    }
}
