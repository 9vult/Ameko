using System.Runtime.InteropServices;

namespace Ffms2CS.Structures
{
    /// <summary>
    /// Information about a video track
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VideoProperties
    {
        public int FPSDenominator;
        public int FPSNumerator;
        public int RFFDenominator;
        public int RFFNumerator;
        public int NumFrames;
        public int SARNum;
        public int SARDen;
        public int CropTop;
        public int CropBottom;
        public int CropLeft;
        public int CropRight;
        public int TopFieldFirst;
        public int ColorSpace;
        public int ColorRange;
        public double FirstTime;
        public double LastTime;
    }
}
