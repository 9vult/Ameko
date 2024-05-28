using Holo.Data;
using Holo.Plugins;
using Holo.Utilities;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Holo
{
    public class AVManager : INotifyPropertyChanged
    {
        // Video
        private IVideoSourcePlugin _source;
        private readonly VideoWrapper _video;
        private bool _videoLoaded = false;
                
        public VideoWrapper Video => _video;
        public int VideoTrack { get; private set; }
        public string VideoPath { get; private set; }
        
        public bool IsVideoLoaded
        {
            get => _videoLoaded;
            private set
            {
                _videoLoaded = value;
                OnPropertyChanged(nameof(IsVideoLoaded));
            }
        }

        public VideoFrame GetFrame()
        {
            if (!IsVideoLoaded) throw new Exception("Cannot get frame when video is unloaded!");
            return _source.GetFrame(_video.CurrentFrame);
        }

        public bool LoadVideo(string filepath)
        {
            if (!_source.IsInitialized) throw new Exception("Failed to load ffms2source");

            if (IsVideoLoaded) _source.CloseFile();

            _source.OpenFile(filepath);
            int[] videoTracks = _source.GetVideoTracks();
            if (videoTracks.Length == 0) throw new Exception("No video tracks in file!");

            VideoTrack = videoTracks[0];
            IsVideoLoaded = _source.LoadTrack(videoTracks[0]);
            if (!IsVideoLoaded) throw new Exception("Failed to load track!");

            var fc = _source.GetFrameCount();
            if (fc <= 0) throw new Exception("No frames in this file!");
            var frame = _source.GetFrame(0);
            var sar = new Rational(frame.Size.X, frame.Size.Y);
            var rate = _source.GetFrameRate();
            _video.Scaffold(fc, sar, rate);
            VideoPath = filepath;
            return _videoLoaded;
        }

        public AVManager()
        {
            _source = new Ffms2Source();
            _source.Initialize();
            _videoLoaded = false;
            VideoPath = string.Empty;
            _video = new VideoWrapper(25, new Rational(1280, 720), new Rational(24000, 1001));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
