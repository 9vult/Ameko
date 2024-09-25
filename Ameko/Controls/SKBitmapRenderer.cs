using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Avalonia;
using SkiaSharp;
using System;
using System.Linq;
using Avalonia.Platform;
using Holo;

namespace Ameko.Controls
{
    public class SKBitmapRenderer : Control
    {
        private readonly GlyphRun _noSkia;

        public SKBitmapRenderer()
        {
            ClipToBounds = true;
            var text = "Current rendering API is not Skia";
            var glyphs = text.Select(ch => Typeface.Default.GlyphTypeface.GetGlyph(ch)).ToArray();
            _noSkia = new GlyphRun(Typeface.Default.GlyphTypeface, 12, text.AsMemory(), glyphs);
        }

        class CustomDrawOp : ICustomDrawOperation
        {
            private readonly IImmutableGlyphRunReference? _noSkia;
            private SKRect _rect;
            private VideoFrame _b;

            public CustomDrawOp(Rect bounds, GlyphRun noSkia, VideoFrame b)
            {
                _noSkia = noSkia.TryCreateImmutableGlyphRunReference();
                Bounds = bounds;
                _rect = new SKRect((float)bounds.Left, (float)bounds.Top, (float)bounds.Right, (float)bounds.Bottom);
                _b = b;
            }

            public void Dispose()
            {
                // No-op
            }

            public Rect Bounds { get; }
            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation? other) => false;
            public void Render(ImmediateDrawingContext context)
            {
                var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
                if (leaseFeature == null)
                {
                    if (_noSkia != null)
                        context.DrawGlyphRun(Brushes.Black, _noSkia);
                }
                else
                {
                    using var lease = leaseFeature.Lease();
                    var canvas = lease.SkCanvas;
                    if (_b.Data != IntPtr.Zero)
                    {
                        using var bitmap = new SKBitmap(_b.Width, _b.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                        // bitmap.InstallPixels();
                        canvas.DrawBitmap(bitmap, _rect);
                    }
                    else
                    {
                        canvas.DrawColor(SKColor.Empty, SKBlendMode.Src);
                    }
                }
            }
        }

        public override void Render(DrawingContext context)
        {
            if (HoloContext.Instance.Workspace.WorkingFile.AVManager.IsVideoLoaded)
            context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), _noSkia, 
                HoloContext.Instance.Workspace.WorkingFile.AVManager.GetFrame()));
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}
