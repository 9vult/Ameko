using System.Runtime.InteropServices;

namespace Ffms2CS.Structures
{
    /// <summary>
    /// Track time base
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TrackTimeBase
    {
        public long Num;
        public long Den;
    }
}
