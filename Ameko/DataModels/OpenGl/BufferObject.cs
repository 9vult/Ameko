// SPDX-License-Identifier: GPL-3.0-only

using System;
using Silk.NET.OpenGLES;

namespace Ameko.DataModels.OpenGl;

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    private readonly uint _handle;
    private readonly BufferTargetARB _bufferTarget;
    private readonly GL _gl;

    /// <summary>
    /// Create a buffer object
    /// </summary>
    /// <param name="gl">OpenGL instance</param>
    /// <param name="data">The buffer</param>
    /// <param name="bufferTarget">Buffer target</param>
    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferTarget)
    {
        _gl = gl;
        _bufferTarget = bufferTarget;

        GLEnum error;
        do
        {
            error = _gl.GetError();
        } while (error != GLEnum.NoError);

        _handle = gl.GenBuffer();
        Bind();
        OpenGlException.ThrowIfError(_gl);

        fixed (void* d = data)
        {
            _gl.BufferData(
                _bufferTarget,
                (nuint)(data.Length * sizeof(TDataType)),
                d,
                BufferUsageARB.StaticDraw
            );
        }
        OpenGlException.ThrowIfError(_gl);
    }

    /// <summary>
    /// Bind the buffer
    /// </summary>
    public void Bind()
    {
        _gl.BindBuffer(_bufferTarget, _handle);
    }

    /// <summary>
    /// Dispose of the buffer
    /// </summary>
    public void Dispose()
    {
        _gl.DeleteBuffer(_handle);
    }
}
