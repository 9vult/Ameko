using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Avalonia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform;
using Holo;

namespace Ameko.Controls
{
    public class SKBitmapRenderer : Control
    {
        private readonly GlyphRun _noSkia;

        static SKPoint _origin = new SKPoint(0, 0);
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

            public CustomDrawOp(Rect bounds, GlyphRun noSkia)
            {
                _noSkia = noSkia.TryCreateImmutableGlyphRunReference();
                Bounds = bounds;
                _rect = new SKRect((float)bounds.Left, (float)bounds.Top, (float)bounds.Right, (float)bounds.Bottom);
            }

            public void Dispose()
            {
                // No-op
            }

            public Rect Bounds { get; }
            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation? other) => false;
            static Stopwatch St = Stopwatch.StartNew();
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
                    if (HoloContext.Instance.AVManager.IsVideoLoaded)
                    {
                        canvas.DrawBitmap(HoloContext.Instance.AVManager.GetFrame().Bitmap, _rect);
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
            context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), _noSkia));
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}
