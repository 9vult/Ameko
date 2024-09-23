using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Holo
{
    public struct VideoFrame
    {
        public bool IsKeyframe;
        public int Width;
        public int Height;
        public int Pitch;
        public bool Flipped;
        public IntPtr VideoPixelData;
        public byte[] SubtitlePixelData;
        public List<Vertex> Vertices;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector2 Position;
        public Vector2 TexCoord;
        public Vector4 Color;
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
