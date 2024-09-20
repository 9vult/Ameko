using Avalonia.Controls;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using OpenTK;
using System;
using Avalonia.Threading;
using OpenTK.Graphics.OpenGL;

namespace Ameko.Controls
{
    internal abstract class BaseOpenTkControl : OpenGlControlBase
    {
        private OpenTkContext? _context;
        private GlInterface? _gl;

        public GlInterface? GlInterface => _gl;

        protected abstract void OpenTkRender();

        protected abstract void OpenTkInit();

        protected abstract void OpenTkDeinit();

        protected override void OnOpenGlRender(GlInterface gl, int fb)
        {
            _gl = gl;

            var size = GetPixelSize();
            GL.Viewport(0, 0, size.Width, size.Height);

            if (Bounds.Width != 0 && Bounds.Height != 0)
                OpenTkRender();

            Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
        }

        protected sealed override void OnOpenGlInit(GlInterface gl)
        {
            _context = new OpenTkContext(gl);
            GL.LoadBindings(_context);

            OpenTkInit();
        }

        protected sealed override void OnOpenGlDeinit(GlInterface gl)
        {
            OpenTkDeinit();
        }

        private PixelSize GetPixelSize()
        {
            var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0d;
            return new PixelSize(
                Math.Max(1, (int)(Bounds.Width * scaling)),
                Math.Max(1, (int)(Bounds.Height * scaling))
            );
        }
    }

    internal class OpenTkContext(GlInterface glInterface) : IBindingsContext
    {
        private readonly GlInterface _glInterface = glInterface;

        public IntPtr GetProcAddress(string procName) => _glInterface.GetProcAddress(procName);
    }
}
