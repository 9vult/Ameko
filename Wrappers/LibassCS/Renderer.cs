using LibassCS.Enums;

namespace LibassCS
{
    public unsafe class Renderer
    {
        private readonly NativeRenderer* _handle;
        internal bool Invalid = false;

        internal NativeRenderer* GetHandle() => !Invalid ? _handle : throw new ObjectDisposedException(nameof(Renderer));

        public void SetFrameSize(int width, int height)
        {
            if (!Invalid)
                External.SetFrameSize(_handle, width, height);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetStorageSize(int width, int height)
        {
            if (!Invalid)
                External.SetStorageSize(_handle, width, height);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetShaper(ShapingLevel level)
        {
            if (!Invalid)
                External.SetShaper(_handle, level);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetMargins(int top, int bottom, int left, int right)
        {
            if (!Invalid)
                External.SetMargins(_handle, top, bottom, left, right);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetUseMargins(bool useMargins)
        {
            if (!Invalid)
                External.SetUseMargins(_handle, useMargins);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetPixelAspect(double par)
        {
            if (!Invalid)
                External.SetPixelAspect(_handle, par);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetFontScale(double scale)
        {
            if (!Invalid)
                External.SetFontScale(_handle, scale);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetHinting(Hinting hinting)
        {
            if (!Invalid)
                External.SetHinting(_handle, hinting);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetLineSpacing(double lineSpacing)
        {
            if (!Invalid)
                External.SetLineSpacing(_handle, lineSpacing);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetLinePosition(double linePosition)
        {
            if (!Invalid)
                External.SetLinePosition(_handle, linePosition);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetFonts(string defaultFont, string defaultFamily, DefaultFontProvider dfp, string config, int update)
        {
            if (!Invalid)
                External.SetFonts(_handle, defaultFont, defaultFamily, dfp, config, update);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetSelectiveStyleOverrideEnabled(OverrideBits bits)
        {
            if (!Invalid)
                External.SetSelectiveStyleOverrideEnabled(_handle, bits);
            else throw new ObjectDisposedException(nameof(Renderer));
        }

        public void SetSelectiveStyleOverride(Style style)
        {
            if (!Invalid)
                External.SetSelectiveStyleOverride(_handle, style.GetHandle());
        }

        public void Uninitialize()
        {
            if (!Invalid)
            {
                External.UninitRenderer(_handle);
                Invalid = true;
            }
        }

        internal Renderer(NativeRenderer* handle)
        {
            _handle = handle;
        }

        ~Renderer()
        {
            if (!Invalid)
                Uninitialize();
        }
    }
}
