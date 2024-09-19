using LibassCS.Enums;
using LibassCS.Structures;

namespace LibassCS
{
    public unsafe class Renderer
    {
        private readonly NativeRenderer* _handle;
        internal bool Invalid = false;

        internal NativeRenderer* GetHandle() => !Invalid ? _handle : throw new ObjectDisposedException(nameof(Renderer));

        public void SetFrameSize(int width, int height)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetFrameSize(_handle, width, height);
        }

        public void SetStorageSize(int width, int height)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetStorageSize(_handle, width, height);
        }

        public void SetShaper(ShapingLevel level)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetShaper(_handle, level);
        }

        public void SetMargins(int top, int bottom, int left, int right)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetMargins(_handle, top, bottom, left, right);
        }

        public void SetUseMargins(bool useMargins)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetUseMargins(_handle, useMargins);
        }

        public void SetPixelAspect(double par)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetPixelAspect(_handle, par);
        }

        public void SetFontScale(double scale)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetFontScale(_handle, scale);
        }

        public void SetHinting(Hinting hinting)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetHinting(_handle, hinting);
        }

        public void SetLineSpacing(double lineSpacing)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetLineSpacing(_handle, lineSpacing);
        }

        public void SetLinePosition(double linePosition)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetLinePosition(_handle, linePosition);
        }

        public void SetFonts(string? defaultFont, string? defaultFamily, DefaultFontProvider dfp, string? config, bool update)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetFonts(_handle, defaultFont, defaultFamily, dfp, config, update);
        }

        public void SetSelectiveStyleOverrideEnabled(OverrideBits bits)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetSelectiveStyleOverrideEnabled(_handle, bits);
        }

        public void SetSelectiveStyleOverride(Style style)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            External.SetSelectiveStyleOverride(_handle, style.GetHandle());
        }

        public Image? RenderFrame(Track track, long time, int detectChanges = 0)
        {
            ObjectDisposedException.ThrowIf(Invalid, nameof(Renderer));
            ObjectDisposedException.ThrowIf(track.Invalid, nameof(Track));
            NativeImage* nImg = External.RenderFrame(_handle, track.GetHandle(), time, detectChanges);
            if (nImg is not null)
                return new Image(nImg);
            return null;
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
