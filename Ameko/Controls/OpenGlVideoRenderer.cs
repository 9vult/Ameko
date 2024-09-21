using Holo;
using Ameko.DataModels;
using OpenTK.Graphics.ES30;

namespace Ameko.Controls
{
    internal class OpenGlVideoRenderer : BaseOpenTkControl
    { 
        private readonly float[] _vertices =
        [
        //   Position           Texture coordinates
             1.0f,  1.0f, 0.0f, 1.0f, 1.0f, // top right
             1.0f, -1.0f, 0.0f, 1.0f, 0.0f, // bottom right
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom left
            -1.0f,  1.0f, 0.0f, 0.0f, 1.0f  // top left
        ];

        private readonly uint[] _indices = [
            0, 1, 3, // First triangle
            1, 2, 3  // Second triangle
        ];

        private int _ebo; // Element Buffer Object
        private int _vbo; // Vertex Buffer Object
        private int _vao; // Vertex Array Object
        private int _tex; // Texture
        private Shader? _shader; // Shader program

        protected override void OpenTkInit()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader(vertexShaderSource, fragmentShaderSource);
            _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _tex = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _tex);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // OpenGL ES 3.0 doesn't support BGRA8888 by default, so swizzle B and R
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleR, (int)All.Blue);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleG, (int)All.Green);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleB, (int)All.Red);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleA, (int)All.Alpha);
        }

        protected override void OpenTkRender()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (!HoloContext.Instance.Workspace.WorkingFile.AVManager.IsVideoLoaded || _tex == 0)
                return;

            VideoFrame frame = HoloContext.Instance.Workspace.WorkingFile.AVManager.GetFrame();

            GL.BindVertexArray(_vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _tex);
            GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba, frame.Width, frame.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, frame.Data);
            
            _shader?.Use();

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        protected override void OpenTkDeinit()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vbo);
            GL.DeleteTexture(_tex);
            _shader?.Dispose();
        }

        private const string vertexShaderSource =
            @"#version 300 es
            in vec3 aPosition;
            in vec2 aTexCoord;

            out vec2 texCoord;

            void main(void)
            {
                texCoord = aTexCoord;
                texCoord.y = 1.0 - texCoord.y; // Flip image
                gl_Position = vec4(aPosition, 1.0);
            }";

        private const string fragmentShaderSource =
            @"#version 300 es
            precision mediump float; // Set the precision for floating-point operations

            in vec2 texCoord;
            out vec4 outputColor;

            uniform sampler2D texture0;

            void main()
            {
                outputColor = texture(texture0, texCoord);
            }";
    }
}
