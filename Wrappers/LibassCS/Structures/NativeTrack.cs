using LibassCS.Enums;
using System.Runtime.InteropServices;

namespace LibassCS.Structures
{
    /// <summary>
    /// Representation of an external script or a matroska subtitle stream. <br/>
    /// Used in rendering after headers are parsed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeTrack
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
        public void* Styles;
        /// <summary>
        /// Array of events
        /// </summary>
        public void* Events;
        /// <summary>
        /// Style format line (after "Format: ")
        /// </summary>
        public char* StyleFormat;
        /// <summary>
        /// Event format line (after "Format: ")
        /// </summary>
        public char* EventFormat;
        /// <summary>
        /// Type of track
        /// </summary>
        public TrackType TrackType;
        public int PlayResX;
        public int PlayResY;
        public double Timer;
        public int WrapStyle;
        public int ScaledBorderAndShadow;
        public int Kerning;
        public char* Language;
        public YCbCrMatrix YCbCrMatrix;
        public int DefaultStyle;
        public char* Name;
        public void* Library;
        public void* ParserPriv;
        public int LayoutResX;
        public int LayoutResY;
    }
}
