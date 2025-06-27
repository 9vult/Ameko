// SPDX-License-Identifier: GPL-3.0-only

using System;
using Silk.NET.OpenGLES;

namespace Ameko.DataModels.OpenGl;

public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private readonly uint _handle;
    private readonly GL _gl;

    /// <summary>
    /// Instantiate a VAO with a VBO and EBO
    /// </summary>
    /// <param name="gl">OpenGL instance</param>
    /// <param name="vbo">Vertex Buffer Object</param>
    /// <param name="ebo">Element Buffer Object</param>
    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
    {
        _gl = gl;
        _handle = _gl.GenVertexArray();

        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    /// <summary>
    /// Define an array of vertex attribute data
    /// </summary>
    /// <param name="index">Index of the attribute to be modified</param>
    /// <param name="count">Number of components</param>
    /// <param name="type">Type of data in the array</param>
    /// <param name="vertexSize">Size of the vertex</param>
    /// <param name="offSet">Offset</param>
    public unsafe void VertexAttributePointer(
        uint index,
        int count,
        VertexAttribPointerType type,
        uint vertexSize,
        int offSet
    )
    {
        _gl.VertexAttribPointer(
            index,
            count,
            type,
            false,
            vertexSize * (uint)sizeof(TVertexType),
            (void*)(offSet * sizeof(TVertexType))
        );
        _gl.EnableVertexAttribArray(index);
    }

    /// <summary>
    /// Bind the VAO
    /// </summary>
    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    /// <summary>
    /// Dispose of the VAO
    /// </summary>
    public void Dispose()
    {
        _gl.DeleteVertexArray(_handle);
    }
}
