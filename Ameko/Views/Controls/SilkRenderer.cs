// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Diagnostics.CodeAnalysis;
using Ameko.DataModels.OpenGl;
using Ameko.Services;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Holo;
using Holo.Media.Providers;
using Holo.Providers;
using Silk.NET.OpenGLES;
using OpenGlException = Ameko.DataModels.OpenGl.OpenGlException;
using Shader = Ameko.DataModels.OpenGl.Shader;
using Texture = Ameko.DataModels.OpenGl.Texture;

namespace Ameko.Views.Controls;

public class SilkRenderer : OpenGlControlBase
{
    public static readonly StyledProperty<MediaController?> MediaControllerProperty =
        AvaloniaProperty.Register<SilkRenderer, MediaController?>(nameof(MediaController));

    public MediaController? MediaController
    {
        get => GetValue(MediaControllerProperty);
        set => SetValue(MediaControllerProperty, value);
    }

    [MemberNotNullWhen(true, nameof(_gl))]
    [MemberNotNullWhen(true, nameof(_vbo))]
    [MemberNotNullWhen(true, nameof(_ebo))]
    [MemberNotNullWhen(true, nameof(_vao))]
    [MemberNotNullWhen(true, nameof(_shader))]
    [MemberNotNullWhen(true, nameof(_texture))]
    private new bool IsInitialized { get; set; }

    private GL? _gl;
    private BufferObject<float>? _vbo;
    private BufferObject<uint>? _ebo;
    private VertexArrayObject<float, uint>? _vao;
    private Shader? _shader;
    private Texture? _texture;

    // csharpier-ignore
    private static readonly float[] Vertices =
    [
        // Position         Texture coords
         1.0f,  1.0f, 0.0f, 1.0f, 1.0f, // top right
         1.0f, -1.0f, 0.0f, 1.0f, 0.0f, // bottom right
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom left
        -1.0f,  1.0f, 0.0f, 0.0f, 1.0f  // top left
    ];

    // csharpier-ignore
    private static readonly uint[] Indices =
    [
        0, 1, 3,  // first triangle
        1, 2, 3   // second triangle
    ];

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);
        _gl = GL.GetApi(gl.GetProcAddress);
        IsInitialized = true;

        _ebo = new BufferObject<uint>(_gl, Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(_gl, Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        _shader = new Shader(
            _gl,
            gl.ContextInfo.Version,
            new Uri("avares://Ameko/Assets/Shaders/main.vert"),
            new Uri("avares://Ameko/Assets/Shaders/main.frag")
        );

        // Tell the VAO object how to lay out the attribute pointers
        var vertexLocation = _shader.GetAttribLocation("aPosition");
        var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
        _vao.VertexAttributePointer(vertexLocation, 3, VertexAttribPointerType.Float, 5, 0); // Position
        _vao.VertexAttributePointer(texCoordLocation, 2, VertexAttribPointerType.Float, 5, 3); // Texture coords

        _texture = new Texture(_gl);
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        if (!IsInitialized)
            throw new OpenGlException("OpenGL is not initialized.");

        _vbo.Dispose();
        _ebo.Dispose();
        _vao.Dispose();
        _shader.Dispose();
        _texture.Dispose();
        base.OnOpenGlDeinit(gl);
        IsInitialized = false;
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (!IsInitialized)
            throw new OpenGlException("OpenGL is not initialized.");

        if (MediaController is null || !MediaController.IsVideoLoaded)
        {
            _gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);
            Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
            return;
        }

        var width = (uint)MediaController.DisplayWidth;
        var height = (uint)MediaController.DisplayHeight;
        _gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);

        _ebo.Bind();
        _vbo.Bind();
        _vao.Bind();

        var frame = MediaController.GetVideoFrame();
        _texture.Bind();
        _texture.SetTexture(width, height, frame->Data);

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
