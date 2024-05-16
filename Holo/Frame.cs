using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holo
{
    public struct VideoFrame
    {
        public Dimension Size { get; set; }
        public SKBitmap Bitmap { get; set; }

        public struct Dimension
        {
            public int X;
            public int Y;
            public readonly double SAR => X / (double)Y;
        }
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
