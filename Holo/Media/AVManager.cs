using AssCS;
using Holo.Plugins;
using Holo.Utilities;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Holo.Media
{
    public class AVManager : INotifyPropertyChanged
    {
        // Video
        private IVideoSourcePlugin _videosource;
        private ISubtitlePlugin _subtitlesource;

        private readonly PlaybackController _playbackController;
        private readonly VideoWrapper _videoWrapper;

        private bool _videoLoaded = false;
        private int _lastFrameIdx = -1;
        private VideoFrame _frame;


        public PlaybackController PlaybackController => _playbackController;
        public VideoWrapper Video => _videoWrapper;

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
            if (_playbackController.CurrentFrame == _lastFrameIdx)
            {
                _subtitlesource.DrawSubtitles(ref _frame, _playbackController.CurrentTimeEstimated.TotalMilliseconds);
                return _frame;
            }

            _frame.Copy = null;
            _videosource.GetFrame(_playbackController.CurrentFrame, ref _frame);
            _subtitlesource.DrawSubtitles(ref _frame, _playbackController.CurrentTimeEstimated.TotalMilliseconds);
            _lastFrameIdx = _playbackController.CurrentFrame;
            return _frame;
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
            _videosource.GetFrame(0, ref _frame);
            var sar = new Rational(_frame.Width, _frame.Height);
            var rate = _videosource.GetFrameRate();

            var frametimes = _videosource.GetFrameTimes();
            var frameIntervals = _videosource.GetFrameIntervals(frametimes);

            _videoWrapper.Scaffold(fc, sar, rate, frametimes, frameIntervals);
            VideoPath = filepath;
            return _videoLoaded;
        }

        public bool UpdateSubtitles(File file)
        {
            if (!_videoLoaded) return false;
            _subtitlesource.LoadSubtitles(file);
            return true;
        }

        public AVManager()
        {
            _videosource = new Ffms2Source();
            _videosource.Initialize();
            _videoLoaded = false;
            VideoPath = string.Empty;

            _videoWrapper = new VideoWrapper(25, new Rational(1920, 1080), new Rational(24000, 1001), [0], [0]);
            _playbackController = new(ref _videoWrapper);

            _subtitlesource = new LibassSource();
            _subtitlesource.Initialize();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
