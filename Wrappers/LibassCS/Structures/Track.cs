using LibassCS.Enums;
using System.Runtime.InteropServices;

namespace LibassCS.Structures
{
    /// <summary>
    /// Representation of an external script or a matroska subtitle stream. <br/>
    /// Used in rendering after headers are parsed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Track
    {
        /// <summary>
        /// Number of styles in the track
        /// </summary>
        public int NumStyles;
        /// <summary>
        /// Number of styles allocated for use
        /// </summary>
        public int MaxStyles;
        /// <summary>
        /// Number of events in the track
        /// </summary>
        public int NumEvents;
        /// <summary>
        /// Number of events allocated for use
        /// </summary>
        public int MaxEvents;
        /// <summary>
        /// Array of styles
        /// </summary>
        public IntPtr Styles;
        /// <summary>
        /// Array of events
        /// </summary>
        public IntPtr Events;
        /// <summary>
        /// Style format line (after "Format: ")
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)] public string StyleFormat;
        /// <summary>
        /// Event format line (after "Format: ")
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)] public string EventFormat;
        /// <summary>
        /// Type of track
        /// </summary>
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
