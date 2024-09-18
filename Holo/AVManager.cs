using AssCS;
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
        private IVideoSourcePlugin _videosource;
        private ISubtitlePlugin _subtitlesource;
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
            var frame = _videosource.GetFrame(_video.CurrentFrame);
            _subtitlesource.DrawSubtitles(ref frame, _video.CurrentTimeEstimated.TotalMilliseconds);
            return frame;
        }

        public bool LoadVideo(string filepath)
        {
            if (!_videosource.IsInitialized) throw new Exception("Failed to load ffms2source");

            if (IsVideoLoaded) _videosource.CloseFile();

            _videosource.OpenFile(filepath);
            int[] videoTracks = _videosource.GetVideoTracks();
            if (videoTracks.Length == 0) throw new Exception("No video tracks in file!");

            VideoTrack = videoTracks[0];
            IsVideoLoaded = _videosource.LoadTrack(videoTracks[0]);
            if (!IsVideoLoaded) throw new Exception("Failed to load track!");

            var fc = _videosource.GetFrameCount();
            if (fc <= 0) throw new Exception("No frames in this file!");
            var frame = _videosource.GetFrame(0);
            var sar = new Rational(frame.Size.X, frame.Size.Y);
            var rate = _videosource.GetFrameRate();

            var frametimes = _videosource.GetFrameTimes();
            var frameIntervals = _videosource.GetFrameIntervals(frametimes);

            _video.Scaffold(fc, sar, rate, frametimes, frameIntervals);
            VideoPath = filepath;
            return _videoLoaded;
        }

        public AVManager()
        {
            _videosource = new Ffms2Source();
            _videosource.Initialize();
            _videoLoaded = false;
            VideoPath = string.Empty;
            _video = new VideoWrapper(25, new Rational(1280, 720), new Rational(24000, 1001), new long[] {0}, new float[] {0});

            _subtitlesource = new LibassSource();
            _subtitlesource.Initialize();

            // TEMP
            var file = new File();
            file.StyleManager.Add(new Style(file.StyleManager.NextId));
            file.EventManager.AddFirst(new Event(file.EventManager.NextId) {
                Text = "Hello from Libass!",
                Start = Time.FromMillis(0),
                End = Time.FromSeconds(5) }
            );
            _subtitlesource.LoadSubtitles(file);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
