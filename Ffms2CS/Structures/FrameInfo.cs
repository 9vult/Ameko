using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS.Structures
{
    /// <summary>
    /// Information about a frame
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FrameInfo
    {
        public long PTS;
        public int RepeatPict;
        public int KeyFrame;
        public long OriginalPTS;
    }
}
