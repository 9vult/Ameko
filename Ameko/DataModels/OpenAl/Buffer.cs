// SPDX-License-Identifier: GPL-3.0-only

using System;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.EXT;

namespace Ameko.DataModels.OpenAl;

public class Buffer : IDisposable
{
    private readonly AL _al;

    /// <summary>
    /// Handle to this buffer
    /// </summary>
    public uint Handle { get; }

    /// <summary>
    /// Create a buffer
    /// </summary>
    /// <param name="al">OpenGL instance</param>
    public Buffer(AL al)
    {
        _al = al;
        Handle = _al.GenBuffer();
    }

    public unsafe void LoadData(
        short* data,
        long startingSample,
        long sampleCount,
        int sampleRate,
        int channels,
        BufferFormat format
    )
    {
        var dataStart = data + startingSample * channels;
        var dataSize = checked((int)(sampleCount * channels * sizeof(short)));
        _al.BufferData(Handle, format, dataStart, dataSize, sampleRate);
    }

    public unsafe void LoadData(
        float* data,
        long startingSample,
        long sampleCount,
        int sampleRate,
        int channels,
        FloatBufferFormat format
    )
    {
        var dataStart = data + startingSample * channels;
        var dataSize = checked((int)(sampleCount * channels * sizeof(float)));

        _al.BufferData(Handle, format, dataStart, dataSize, sampleRate);
    }

    /// <summary>
    /// Dispose of the buffer
    /// </summary>
    public void Dispose()
    {
        _al.DeleteBuffer(Handle);
    }
}
