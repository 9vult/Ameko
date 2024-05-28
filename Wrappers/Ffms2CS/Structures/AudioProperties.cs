using System.Runtime.InteropServices;

namespace Ffms2CS.Structures
{
    /// <summary>
    /// Information about an audio track
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AudioProperties
    {
        public int SampleFormat;
        public int SampleRate;
        public int BitsPerSample;
        public int Channels;
        public long ChannelLayout;
        public long NumSamples;
        public double FirstTime;
        public double LastTime;
    }
}
