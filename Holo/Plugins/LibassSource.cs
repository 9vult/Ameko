using AssCS;
using AssCS.IO;
using LibassCS;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Holo.Plugins
{
    internal class LibassSource : ISubtitlePlugin
    {
        public string Name => "LibassSource";

        public string Description => "Subtitle provider plugin backed by Libass";

        public double Version => 0.1d;

        public string Author => "9volt";

        public string AuthorUrl => "https://ameko.moe";

        public bool IsInitialized => this._initialized;

        private bool _initialized = false;
        private Renderer? _renderer;
        private Track? _track;

        private static ConsumerInfo ConsumerInfo => new("Ameko LibassSource", "(Internal)", "https://ameko.moe");
        private AssWriter? _writer;

        // TODO: ???
        public unsafe void DrawSubtitles(ref VideoFrame frame, long time)
        {
            if (!_initialized) throw new InvalidOperationException("Libass is not initialized");
            if (_track is null) throw new InvalidOperationException("Track is null!");
            var image = _renderer!.RenderFrame(_track, time);
            if (image is null)
                return;
            var dst = frame.Bitmap;

            // Libass returns a linked list of alpha-masked monochrome images.
            // Loop through the list, blending each one into the frame

            for (; image is not null; image = image.Next)
            {
                uint opacity = 255 - image.Color & 0xFF;
                uint r = image.Color >> 24;
                uint g = (image.Color >> 16) & 0xFF;
                uint b = (image.Color >> 8) & 0xFF;
                uint a = image.Color & 0xFF;

                var srcBitmap = new SKBitmap(image.Width, image.Height, false);
                srcBitmap.SetPixels(new IntPtr(image.Bitmap));
                
                SKRectI dstRect = new(image.DistX, image.DistY, image.DistX + image.Width, image.DistY + image.Height);
                dstRect.Intersect(new SKRectI(0, 0, image.Width, image.Height));

                IntPtr srcPixels = srcBitmap.GetPixels();
                IntPtr dstPixels = dst.GetPixels();

                byte* srcPtr = (byte*)srcPixels.ToPointer();
                byte* dstPtr = (byte*)dstPixels.ToPointer();

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        int srcIndex = (y * srcBitmap.RowBytes) + x;
                        int dstIndex = ((y + image.DistX) * dst.RowBytes) + x + image.DistX;
                           
                        // Source is grayscale
                        byte srcGray = srcPtr[srcIndex];
                        uint k = srcGray * opacity / 255;
                        uint ck = 255 - k;

                        // Destination is BGRA
                        byte* dstPixel = &dstPtr[dstIndex * 4];
                        byte frameB = dstPixel[0];
                        byte frameG = dstPixel[1];
                        byte frameR = dstPixel[2];

                        // Blend the pixels
                        dstPixel[0] = (byte)((k * b + ck * frameB) / 255);
                        dstPixel[1] = (byte)((k * g + ck * frameG) / 255);
                        dstPixel[2] = (byte)((k * r + ck * frameR) / 255);
                        dstPixel[3] = 255; // Alpha
                    }
                }
            }
        }

        public void LoadSubtitles(File file, int time = -1)
        {
            if (_writer is not null)
            {
                LoadSubtitles(_writer.WriteString());
            }
            else
            {
                _writer = new(file, string.Empty, ConsumerInfo);
                LoadSubtitles(_writer.WriteString());
            }
        }

        public void LoadSubtitles(string data)
        {
            if (!_initialized) throw new InvalidOperationException("Libass is not initialized");
            _track?.Uninitialize();
            _track = Libass.ReadMemory(data, null);
        }

        public bool Initialize()
        {
            if (_initialized) return false;
            try
            {
                Libass.StartUp();

                _initialized = true;
                _renderer = Libass.CreateRenderer();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not initialize libass", ex);
            }
        }

        public bool Deinitialize()
        {
            if (!_initialized) return false;
            try
            {
                Libass.Shutdown();
                _initialized = false;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not deinitialize libass", ex);
            }
        }

        public double GetBackingVersion()
        {
            if (!_initialized) throw new InvalidOperationException("Libass is not initialized");
            return Libass.GetVersion();
        }

        public IDictionary<string, dynamic> GetProperties()
        {
            throw new NotImplementedException();
        }
    }
}
