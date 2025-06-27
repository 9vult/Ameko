// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.IO;
using Avalonia.Platform;
using Silk.NET.OpenGLES;

namespace Ameko.DataModels.OpenGl;

public class Shader : IDisposable
{
    private readonly uint _handle;
    private readonly GL _gl;

    public Shader(GL gl, Uri vertexPath, Uri fragmentPath)
    {
        _gl = gl;

        var vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        var fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);

        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
            throw new OpenGlException($"Shader failed to link: {_gl.GetProgramInfoLog(_handle)}");

        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    /// <summary>
    /// Use the shader
    /// </summary>
    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    /// <summary>
    /// Set an integer value
    /// </summary>
    /// <param name="name">Name of the value</param>
    /// <param name="value">Integer value</param>
    /// <exception cref="OpenGlException">If setting fails</exception>
    public void SetUniform(string name, int value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
            throw new OpenGlException($"Uniform '{name}' not found on shader.");

        _gl.Uniform1(location, value);
    }

    /// <summary>
    /// Set a float value
    /// </summary>
    /// <param name="name">Name of the value</param>
    /// <param name="value">Float value</param>
    /// <exception cref="OpenGlException">If setting fails</exception>
    public void SetUniform(string name, float value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
            throw new OpenGlException($"Uniform '{name}' not found on shader.");

        _gl.Uniform1(location, value);
    }

    /// <summary>
    /// Dispose of the shader
    /// </summary>
    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }

    /// <summary>
    /// Load the given shader
    /// </summary>
    /// <param name="type">Type of shader being loaded</param>
    /// <param name="uri">Path to the shader</param>
    /// <returns>ID of the shader</returns>
    /// <exception cref="OpenGlException">If loading fails</exception>
    private uint LoadShader(ShaderType type, Uri uri)
    {
        using var reader = new StreamReader(AssetLoader.Open(uri));

        var src = reader.ReadToEnd();
        var handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);

        var infoLog = _gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
            throw new OpenGlException($"Error compiling {type} shader: {infoLog}");

        return handle;
    }
}
