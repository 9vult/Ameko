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
        private readonly AVManager _avmanager;

        static SKPoint _origin = new SKPoint(0, 0);
        public SKBitmapRenderer(AVManager avm)
        {
            _avmanager = avm;
            ClipToBounds = true;
            var text = "Current rendering API is not Skia";
            var glyphs = text.Select(ch => Typeface.Default.GlyphTypeface.GetGlyph(ch)).ToArray();
            _noSkia = new GlyphRun(Typeface.Default.GlyphTypeface, 12, text.AsMemory(), glyphs);
        }

        class CustomDrawOp : ICustomDrawOperation
        {
            private readonly IImmutableGlyphRunReference? _noSkia;
            private SKRect _rect;
            private SKBitmap? _b;

            public CustomDrawOp(Rect bounds, GlyphRun noSkia, SKBitmap? b)
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
                    if (_b != null)
                    {
                        canvas.DrawBitmap(_b, _rect);
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
            context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), _noSkia, _avmanager.GetFrame().Bitmap));
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}
