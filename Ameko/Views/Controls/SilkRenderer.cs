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
        //X    Y      Z     R  G  B  A
         0.5f,  0.5f, 0.0f, 1, 0, 0, 1,
         0.5f, -0.5f, 0.0f, 0, 0, 0, 1,
        -0.5f, -0.5f, 0.0f, 0, 0, 1, 1,
        -0.5f,  0.5f, 0.5f, 0, 0, 0, 1
    ];

    // csharpier-ignore
    private static readonly uint[] Indices =
    [
        0, 1, 3,
        1, 2, 3
    ];

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        _gl = GL.GetApi(gl.GetProcAddress);
        IsInitialized = true;

        var glVersion = gl.ContextInfo.Version;
        var glslVersion =
            glVersion.Type == GlProfileType.OpenGLES ? "es300"
            : glVersion.Major == 4 ? "410"
            : throw new OpenGlException(
                $"OpenGL version {glVersion.Major}.{glVersion.Minor} is not supported!"
            );

        _ebo = new BufferObject<uint>(_gl, Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(_gl, Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        // Tell the VAO object how to lay out the attribute pointers
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0);
        _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3);

        _shader = new Shader(
            _gl,
            new Uri($"avares://Ameko/Assets/Shaders/{glslVersion}.vert"),
            new Uri($"avares://Ameko/Assets/Shaders/{glslVersion}.frag")
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

        _gl.ClearColor(Color.Firebrick);
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _gl.Enable(EnableCap.DepthTest);
        _gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);

        _ebo.Bind();
        _vbo.Bind();
        _vao.Bind();
        _shader.Use();
        _shader.SetUniform("uBlue", (float)Math.Sin(DateTime.Now.Millisecond / 1000f * Math.PI));

        _gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)Indices.Length,
            DrawElementsType.UnsignedInt,
            null
        );
        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
    }
}
