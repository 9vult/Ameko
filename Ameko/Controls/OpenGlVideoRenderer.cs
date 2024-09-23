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

        // Masks
        private int _maskVao;
        private int _maskVbo;
        private Shader? _maskShader;

        private const int VERTEX_STRIDE = 8 * sizeof(float);

        protected override void OpenTkInit()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Masks
            _maskVao = GL.GenVertexArray();
            _maskVbo = GL.GenBuffer();

            _maskShader = new Shader(new System.Uri("avares://Ameko/Assets/Shaders/video.vert"), new System.Uri("avares://Ameko/Assets/Shaders/submask.frag"));
            _maskShader.Use();
        }

        protected override void OpenTkRender()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (!HoloContext.Instance.Workspace.WorkingFile.AVManager.IsVideoLoaded)
                return;

            VideoFrame frame = HoloContext.Instance.Workspace.WorkingFile.AVManager.GetFrame();

            // Masks
            var maskVertices = frame.Vertices.ToArray();
            GL.BindVertexArray(_maskVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _maskVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, maskVertices.Length * VERTEX_STRIDE, maskVertices, BufferUsageHint.DynamicDraw);
            // Vertex layout
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, VERTEX_STRIDE, 0); // Position
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VERTEX_STRIDE, 2 * sizeof(float)); // TexCoord
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, VERTEX_STRIDE, 4 * sizeof(float)); // Color

            GL.EnableVertexAttribArray(0); // Position
            GL.EnableVertexAttribArray(1); // TexCoord
            GL.EnableVertexAttribArray(2); // Color

            GL.DrawArrays(PrimitiveType.Triangles, 0, maskVertices.Length);
            GL.BindVertexArray(0);

        }

        protected override void OpenTkDeinit()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            _maskShader?.Dispose();
        }
    }
}
