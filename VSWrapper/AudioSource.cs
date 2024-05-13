using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
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

    public class AudioSource
    {
    }
}
