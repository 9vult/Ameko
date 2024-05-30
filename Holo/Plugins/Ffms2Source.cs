using Ffms2CS;
using Ffms2CS.Enums;
using Holo.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holo.Plugins
{
    public class Ffms2Source : IVideoSourcePlugin // TODO: , IAudioSourcePlugin
    {
        public string Name => "Ffms2Source";
        public string Description => "Audio/Video Source plugin backed by Ffms2";
        public double Version => 0.1d;
        public string Author => "9volt";
        public string AuthorUrl => "https://ameko.moe";
        public bool IsInitialized => this._initialized;

        private bool _initialized = false;
        private string? _filename;
        private int _trackNumber;
        private Ffms2CS.Indexer? _indexer;
        private Ffms2CS.Index? _index;
        private Ffms2CS.VideoSource? _source;

        public VideoFrame GetFrame(int frame)
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            if (_source == null) throw new InvalidOperationException("Video source is not initialized");
            
            try
            {
                var frameData = _source.GetFrame(frame);
                return new VideoFrame
                {
                    IsKeyframe = frameData.IsKeyframe,
                    Size = new VideoFrame.Dimension
                    {
                        X = frameData.OutputResolution.Width,
                        Y = frameData.OutputResolution.Height
                    },
                    Bitmap = frameData.SKBitmap
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get frame {frame}", ex);
            }
        }

        public int GetFrameCount()
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            if (_source == null) throw new InvalidOperationException("Video source is not initialized");
            return _source.FrameCount;
        }

        public Rational GetFrameRate()
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            if (_source == null) throw new InvalidOperationException("Video source is not initialized");
            return new Rational(_source.FPSNumerator, _source.FPSDenominator);
        }

        public long[] GetFrameTimes()
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            if (_index == null) throw new InvalidOperationException("Index is not initialized");
            if (_source == null) throw new InvalidOperationException("Video source is not initialized");

            var times = new long[_source.FrameCount];
            var track = _index.GetTrack(_trackNumber);

            for (int i = 0; i < track.FrameCount; i++)
            {
                var frame = track.GetFrameInfo(i);
                var time = (long)((frame.PTS * track.TimebaseNumerator) / (double)track.TimebaseDenomerator);
                times[i] = time; // Presentation timestamp
            }

            return times;
        }

        public float[] GetFrameIntervals(long[] frametimes)
        {
            var frameCount = frametimes.Length;
            var intervals = new float[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                if (i + 1 >= frameCount) intervals[i] = 0;
                else intervals[i] = frametimes[i + 1] - frametimes[i];
            }
            return intervals;
        }

        public int[] GetVideoTracks()
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            if (_indexer == null) throw new InvalidOperationException("Indexer is not initialized");

            List<int> tracks = new List<int>();
            var totalTrackCount = _indexer.TrackCount;
            for (int i = 0; i < totalTrackCount; i++)
            {
                if (_indexer.GetTrackType(i) == TrackType.Video)
                    tracks.Add(i);
            }
            return tracks.ToArray();
        }

        public bool LoadTrack(int track)
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            if (_indexer == null) throw new InvalidOperationException("Indexer is not initialized");

            try
            {
                var totalTrackCount = _indexer.TrackCount;
                for (int i = 0; i < totalTrackCount - 1; i++)
                {
                    if (i != track)
                        _indexer.SetTrackShouldIndex(i, false);
                }
                _index = _indexer.Index(IndexErrorHandling.Abort);
                _source = _index.VideoSource(_filename!, track);
                _trackNumber = track;

                // Set up output format
                if (_source.FrameCount > 0)
                {
                    var testframe = _source.GetFrame(0);
                    // TODO: option this somewhere
                    List<int> pixfmts = new List<int>
                    {
                        Ffms2.GetPixelFormat("bgra")
                    };
                    _source.SetOutputFormat(pixfmts, testframe.EncodedResolution.Width, testframe.EncodedResolution.Height, Resizer.Bicubic);
                }
                else
                {
                    return false; // ???
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load track {track}", ex);
            }
        }

        public bool OpenFile(string filename)
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            try
            {
                _filename = filename;
                _indexer = new Indexer(filename);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load file {filename} for indexing", ex);
            }
        }

        public bool CloseFile()
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            if (_indexer == null) throw new InvalidOperationException("Indexer is not initialized");
            
            try
            {
                _index?.Dispose();
                _indexer.Dispose();
                _filename = null;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to close file {_filename}", ex);
            }
        }

        public bool Initialize()
        {
            if (_initialized) return false;
            try
            {
                Ffms2.StartUp();
                _initialized = true;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not initialize Ffms2", ex);
            }
        }

        public bool Deinitialize()
        {
            if (!_initialized) return false;
            try
            {
                Ffms2.Shutdown();
                _initialized = false;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not deinitialize Ffms2", ex);
            }
        }

        public double GetBackingVersion()
        {
            if (!_initialized) throw new InvalidOperationException("Ffms2 is not initialized");
            return Ffms2.GetVersion();
        }

        public IDictionary<string, dynamic> GetProperties()
        {
            return new Dictionary<string, dynamic>(); // idk what to do with this tbh
        }
    }
}
