using Holo.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Holo
{
    public class AVManager : INotifyPropertyChanged
    {
        // Video 
        // TODO: Extract to a class?
        private Ffms2Source _source;
        private bool _videoLoaded;
        private int _frameCount;
        private int _currentFrame;

        public bool IsVideoLoaded => _videoLoaded;
        public int FrameCount => _frameCount;
        public int CurrentFrame
        {
            get => _currentFrame;
            set
            {
                _currentFrame = value;
                OnPropertyChanged(nameof(CurrentFrame));
            }
        }

        public VideoFrame GetFrame()
        {
            return _source.GetFrame(_currentFrame);
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
            _frameCount = _source.GetFrameCount();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
