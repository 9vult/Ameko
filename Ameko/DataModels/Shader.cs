using System;
using System.IO;
using System.Reflection.Metadata;
using Avalonia.Platform;
using OpenTK.Graphics.ES30;

namespace Ameko.DataModels
{
    internal class Shader : IDisposable
    {
        private bool _disposed = false;
        private readonly int _program;
        private readonly int _vertexShader;
        private readonly int _fragmentShader;

        public void Use()
        {
            GL.UseProgram(_program);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(_program, attribName);
        }

        public Shader(Uri vertexFile, Uri fragmentFile)
            : this(new StreamReader(AssetLoader.Open(vertexFile)).ReadToEnd(),
                   new StreamReader(AssetLoader.Open(fragmentFile)).ReadToEnd()
        ) { }

        public Shader(string vertexSource, string fragmentSource)
        {
            _vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(_vertexShader, vertexSource);

            _fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(_fragmentShader, fragmentSource);

            GL.CompileShader(_vertexShader);
            GL.GetShader(_vertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
                throw new Exception(GL.GetShaderInfoLog(_vertexShader));

            GL.CompileShader(_fragmentShader);
            GL.GetShader(_fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
                throw new Exception(GL.GetShaderInfoLog(_fragmentShader));

            _program = GL.CreateProgram();
            GL.AttachShader(_program, _vertexShader);
            GL.AttachShader(_program, _fragmentShader);
            
            GL.LinkProgram(_program);
            GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
                throw new Exception(GL.GetProgramInfoLog(_program));

            GL.DetachShader(_program, _vertexShader);
            GL.DetachShader(_program, _fragmentShader);
            GL.DeleteShader(_vertexShader);
            GL.DeleteShader(_fragmentShader);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                GL.DeleteProgram(_program);
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Shader()
        {
            if (!_disposed)
                Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }
}
