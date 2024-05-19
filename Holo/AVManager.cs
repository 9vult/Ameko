using Holo.Data;
using Holo.Plugins;
using Holo.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Holo
{
    public class AVManager
    {
        // Video 
        // TODO: Extract to a class?
        private IVideoSourcePlugin _source;
        private bool _videoLoaded = false;
        private VideoWrapper _video;
        public bool IsVideoLoaded => _videoLoaded;
        public VideoWrapper Video => _video;

        public VideoFrame GetFrame()
        {
            return _source.GetFrame(_video.CurrentFrame);
        }

        // TODO
        public AVManager()
        {
            _source = new Ffms2Source();

            _source.Initialize();
            if (!_source.IsInitialized) throw new Exception("Failed to load ffms2source");
            _source.OpenFile("test.mkv");
            int[] videoTracks = _source.GetVideoTracks();
            if (videoTracks.Length == 0) throw new Exception("No video tracks in file!");

            _videoLoaded = _source.LoadTrack(videoTracks[0]);

            var fc = _source.GetFrameCount();
            if (fc <= 0) throw new Exception("No frames in this file!");
            var frame = _source.GetFrame(0);
            var sar = new Rational(frame.Size.X, frame.Size.Y);
            var rate = _source.GetFrameRate();
            _video = new VideoWrapper(fc, sar, rate);
        }

    }
}
