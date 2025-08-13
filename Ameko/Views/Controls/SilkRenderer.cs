// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Ameko.DataModels.OpenGl;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Silk.NET.OpenGLES;
using OpenGlException = Ameko.DataModels.OpenGl.OpenGlException;
using Shader = Ameko.DataModels.OpenGl.Shader;

namespace Ameko.Views.Controls;

public class SilkRenderer : OpenGlControlBase
{
    [MemberNotNullWhen(true, nameof(_gl))]
    [MemberNotNullWhen(true, nameof(_vbo))]
    [MemberNotNullWhen(true, nameof(_ebo))]
    [MemberNotNullWhen(true, nameof(_vao))]
    [MemberNotNullWhen(true, nameof(_shader))]
    private new bool IsInitialized { get; set; }

    private GL? _gl;
    private BufferObject<float>? _vbo;
    private BufferObject<uint>? _ebo;
    private VertexArrayObject<float, uint>? _vao;
    private Shader? _shader;

    // csharpier-ignore
    private static readonly float[] Vertices =
    [
        // x,    y,    z       r,    g,    b,    a
        -0.5f,  0.5f, 0.0f,   1f,   0f,   0f,   1f,  // top-left, red
        -0.5f, -0.5f, 0.0f,   0f,   1f,   0f,   1f,  // bottom-left, green
         0.5f, -0.5f, 0.0f,   0f,   0f,   1f,   1f,  // bottom-right, blue
         0.5f,  0.5f, 0.0f,   1f,   1f,   0f,   1f   // top-right, yellow
    ];

    // csharpier-ignore
    private static readonly uint[] Indices =
    [
        0, 1, 2,  // first triangle
        0, 2, 3   // second triangle
    ];

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        _gl = GL.GetApi(gl.GetProcAddress);
        IsInitialized = true;

        _ebo = new BufferObject<uint>(_gl, Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(_gl, Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        // Tell the VAO object how to lay out the attribute pointers
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0); // Position
        _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3); // Color

        _shader = new Shader(
            _gl,
            gl.ContextInfo.Version,
            new Uri("avares://Ameko/Assets/Shaders/main.vert"),
            new Uri("avares://Ameko/Assets/Shaders/main.frag")
        );
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        if (!IsInitialized)
            throw new OpenGlException("OpenGL is not initialized.");

        _vbo.Dispose();
        _ebo.Dispose();
        _vao.Dispose();
        _shader.Dispose();
        base.OnOpenGlDeinit(gl);
        IsInitialized = false;
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (!IsInitialized)
            throw new OpenGlException("OpenGL is not initialized.");

        _gl.ClearColor(Color.Black);
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _gl.Enable(EnableCap.DepthTest);
        _gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);

        _ebo.Bind();
        _vbo.Bind();
        _vao.Bind();
        _shader.Use();

        _gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)Indices.Length,
            DrawElementsType.UnsignedInt,
            null
        );
        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
    }
}
