using Holo;
using OpenTK.Graphics.OpenGL;
using System;

namespace Ameko.Controls
{
    internal class OpenGlVideoRenderer : BaseOpenTkControl
    {
        private int _textureId;
        private int _vbo;
        private int _vao;
        private int _shaderProgram;

        protected override void OpenTkInit()
        {
            // Generate the texture ID
            GL.GenTextures(1, out _textureId);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            float[] vertices = [
                -1.0f, -1.0f, 0.0f, 0.0f, // Bottom-left
                 1.0f, -1.0f, 1.0f, 0.0f, // Bottom-right
                 1.0f,  1.0f, 1.0f, 1.0f, // Top-right
                -1.0f,  1.0f, 0.0f, 1.0f  // Top-left
            ];

            GL.GenVertexArrays(1, out _vao);
            GL.GenBuffers(1, out _vbo);

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Define the layout of the vertex data
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            string version = GL.GetString(StringName.Version);
            bool isES = version.Contains("ES");

            _shaderProgram = CreateShaderProgram(isES);
            GL.UseProgram(_shaderProgram);
        }

        protected override void OpenTkRender()
        {
            // Clear the screen
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (!HoloContext.Instance.Workspace.WorkingFile.AVManager.IsVideoLoaded || _textureId == 0)
                return;

            VideoFrame frame = HoloContext.Instance.Workspace.WorkingFile.AVManager.GetFrame();

            // Bind the texture
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            var x = GL.GetError();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, frame.Width, frame.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, frame.Data);
            x = GL.GetError(); // ???? invalid operation
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
        }

        protected override void OpenTkDeinit()
        {
            if (_textureId == 0)
                return;
            GL.DeleteTextures(1, ref _textureId);
            GL.DeleteBuffers(1, ref _vbo);
            GL.DeleteVertexArrays(1, ref _vao);
            GL.DeleteProgram(_shaderProgram);
        }

        private int CreateShaderProgram(bool isOpenGLES)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            string vertexShaderSource = isOpenGLES ? 
                @"#version 300 es
                layout(location = 0) in vec2 aPos;
                layout(location = 1) in vec2 aTexCoord;
                out vec2 TexCoord;
                void main()
                {
                    gl_Position = vec4(aPos, 0.0, 1.0);
                    TexCoord = aTexCoord;
                }" :
                @"#version 330 core
                layout(location = 0) in vec2 aPos;
                layout(location = 1) in vec2 aTexCoord;
                out vec2 TexCoord;
                void main()
                {
                    gl_Position = vec4(aPos, 0.0, 1.0);
                    TexCoord = aTexCoord;
                }";

            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            // Check compilation
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vertexCompileStatus);
            if (vertexCompileStatus == 0)
            {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                throw new Exception($"Vertex Shader Compilation Error: {infoLog}");
            }

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            string fragmentShaderSource = isOpenGLES ?
                @"#version 300 es
                precision mediump float;
                out vec4 FragColor;
                in vec2 TexCoord;
                uniform sampler2D texture1;
                void main()
                {
                    FragColor = texture(texture1, TexCoord);
                }" :
                @"#version 330 core
                out vec4 FragColor;
                in vec2 TexCoord;
                uniform sampler2D texture1;
                void main()
                {
                    FragColor = texture(texture1, TexCoord);
                }";

            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            // Check compilation
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fragmentCompileStatus);
            if (fragmentCompileStatus == 0)
            {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                throw new Exception($"Fragment Shader Compilation Error: {infoLog}");
            }

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            // Check linking
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Shader Program Linking Error: {infoLog}");
            }

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

    }
}
