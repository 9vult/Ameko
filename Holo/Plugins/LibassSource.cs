using AssCS;
using AssCS.IO;
using Holo.Utilities;
using LibassCS;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Holo.Plugins
{
    internal partial class LibassSource : ISubtitlePlugin
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
            if (_track is null) return;

            _renderer!.SetFrameSize(frame.Width, frame.Height);
            _renderer!.SetStorageSize(frame.Width, frame.Height);
            _renderer!.SetFontScale(1.0d);
            var image = _renderer!.RenderFrame(_track, time);
            if (image is null)
                return;

            if (frame.Copy is not null)
            {
                // Copy frame data from copy
                fixed (byte* ptr = frame.Copy)
                    SpeedDemonNative.CopyFrame(ptr, frame.Data, frame.Width * frame.Height * BGRA_WIDTH);
            }
            else
            {
                // Copy frame data to copy
                frame.Copy = new byte[frame.Width * frame.Height * BGRA_WIDTH];
                fixed (byte* ptr = frame.Copy)
                    SpeedDemonNative.CopyFrame(frame.Data, ptr, frame.Width * frame.Height * BGRA_WIDTH);
            }

            SpeedDemonNative.RenderSubs(frame.Data, frame.Width, frame.Height, image);
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
