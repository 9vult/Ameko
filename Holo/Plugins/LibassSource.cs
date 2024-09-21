using AssCS;
using AssCS.IO;
using LibassCS;
using System;
using System.Collections.Generic;
using System.Text;

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

        private const int BGRA_WIDTH = 4;
        public unsafe void DrawSubtitles(ref VideoFrame frame, long time)
        {
            if (!_initialized) throw new InvalidOperationException("Libass is not initialized");
            if (_track is null) throw new InvalidOperationException("Track is null!");
            var image = _renderer!.RenderFrame(_track, time);
            if (image is null)
                return;

            byte* subtitleBuffer = (byte*)frame.Data.ToPointer();

            // Libass returns a linked list of alpha-masked monochrome images.
            // Loop through the list, blending each one into the frame

            // TODO: This is extremely slow lmao
            // Potentially move into OpenGL if possible, or C
            for (var img = image; img is not null; img = img.Next)
            {
                byte* bitmap = (byte*)img.Bitmap.ToPointer();

                uint opacity = 255 - img.Color & 0xFF;
                uint r = img.Color >> 24;
                uint g = (img.Color >> 16) & 0xFF;
                uint b = (img.Color >> 8) & 0xFF;
                uint a = img.Color & 0xFF;

                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        // source pixel index
                        int srcIndex = y * img.Stride + x;

                        // destination pixel index
                        int destX = x + img.DistX;
                        int destY = y + img.DistY;

                        if (destX >= 0 && destX < frame.Width && destY >= 0 && destY < frame.Height)
                        {
                            int destIndex = (destY * frame.Width + destX) * BGRA_WIDTH;

                            byte srcAlpha = bitmap[srcIndex];
                            byte k = (byte)(srcAlpha * opacity / 255);
                            byte ck = (byte)(255 - k);

                            subtitleBuffer[destIndex + 0] = (byte)((k * b + ck * subtitleBuffer[destIndex + 0]) / 255);
                            subtitleBuffer[destIndex + 1] = (byte)((k * g + ck * subtitleBuffer[destIndex + 1]) / 255);
                            subtitleBuffer[destIndex + 2] = (byte)((k * r + ck * subtitleBuffer[destIndex + 2]) / 255);
                            subtitleBuffer[destIndex + 3] = 255; // Set alpha to fully opaque
                        }
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
                _writer = new(file, string.Empty, ConsumerInfo, new UTF8Encoding(true));
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
                // TODO
                _renderer.SetFrameSize(1920, 1080);
                _renderer.SetStorageSize(1920, 1080);
                _renderer.SetFontScale(1.0d);
                _renderer.SetFonts(null, "Sans", LibassCS.Enums.DefaultFontProvider.AutoDetect, null, true);
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
