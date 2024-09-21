using System;
using System.Collections.Generic;

namespace Holo
{
    public struct VideoFrame
    {
        public bool IsKeyframe;
        public int Width;
        public int Height;
        public int Pitch;
        public bool Flipped;
        public IntPtr Data;
        //public byte[] Bytes;
    }

    public struct AudioFrame
    {
        public long SampleRate { get; }
        public long SampleCount { get; }
        public byte[] Data { get; set; }

        public AudioFrame(long sampleRate, long sampleCount)
        {
            SampleRate = sampleRate;
            SampleCount = sampleCount;
            Data = new byte[SampleRate * sampleCount];
        }
    }
}
