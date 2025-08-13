// SPDX-License-Identifier: GPL-3.0-only

using System;
using Silk.NET.OpenGLES;

namespace Ameko.DataModels.OpenGl;

public class Texture : IDisposable
{
    private readonly uint _handle;
    private readonly GL _gl;

    public Texture(GL gl)
    {
        _gl = gl;
        _handle = _gl.GenTexture();
        Bind();
        SetTextureParams();
    }

    /// <summary>
    /// Bind the texture
    /// </summary>
    /// <param name="slot">Texture slot to use</param>
    public void Bind(TextureUnit slot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(slot);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    /// <summary>
    /// Set the texture
    /// </summary>
    /// <param name="width">Width of the texture</param>
    /// <param name="height">Height of the texture</param>
    /// <param name="data">Pixels</param>
    public void SetTexture(uint width, uint height, nint data)
    {
        _gl.TexImage2D(
            GLEnum.Texture2D,
            0,
            InternalFormat.Rgba,
            width,
            height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            data
        );
    }

    /// <summary>
    /// Set the texture
    /// </summary>
    /// <param name="width">Width of the texture</param>
    /// <param name="height">Height of the texture</param>
    /// <param name="data">Pixels</param>
    public unsafe void SetTexture(uint width, uint height, Span<byte> data)
    {
        fixed (byte* ptr = data)
        {
            _gl.TexImage2D(
                GLEnum.Texture2D,
                0,
                InternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                ptr
            );
        }
    }

    private void SetTextureParams()
    {
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
        );

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge
        );

        // Swizzle B and R to get BGRA8888
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureSwizzleR,
            (int)GLEnum.Blue
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureSwizzleG,
            (int)GLEnum.Green
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureSwizzleB,
            (int)GLEnum.Red
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureSwizzleA,
            (int)GLEnum.Alpha
        );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _gl.DeleteTexture(_handle);
        }
    }
}
