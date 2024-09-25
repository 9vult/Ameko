using AssCS;
using Holo.Data;
using Holo.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Holo.Media
{
    public class PlaybackController : INotifyPropertyChanged
    {
        private int _currentFrame;

        private readonly HighResolutionTimer _playback;
        private bool _isPlaying;
        private bool _isPaused;
        private int _playbackDestination;
        private bool _isAutoSeekEnabled = true;

        private double _displayWidth;
        private double _displayHeight;
        private ScalePercentage _displayScale = ScalePercentage.VS_100;

        private readonly VideoWrapper _video;

        /// <summary>
        /// The current frame in the video
        /// </summary>
        public int CurrentFrame
        {
            get => _currentFrame;
            set
            {
                _currentFrame = value;
                OnPropertyChanged(nameof(CurrentFrame));
                OnPropertyChanged(nameof(CurrentTimeEstimated));
            }
        }

        /// <summary>
        /// Current timestamp based on the current frame
        /// </summary>
        public Time CurrentTimeEstimated => _video.FrameToTime(_currentFrame);

        /// <summary>
        /// Scale the video is being displayed at
        /// </summary>
        public ScalePercentage DisplayScale
        {
            get => _displayScale;
            set
            {
                _displayScale = value;
                OnPropertyChanged(nameof(DisplayScale));
                DisplayWidth = value.Multiplier * _video.SAR.Numerator;
                DisplayHeight = value.Multiplier * _video.SAR.Denominator;
            }
        }

        /// <summary>
        /// Width the video is being displayed at
        /// </summary>
        public double DisplayWidth
        {
            get => _displayWidth;
            set
            {
                _displayWidth = value;
                OnPropertyChanged(nameof(DisplayWidth));
            }
        }

        /// <summary>
        /// Height the video is being displayed at
        /// </summary>
        public double DisplayHeight
        {
            get => _displayHeight;
            set
            {
                _displayHeight = value;
                OnPropertyChanged(nameof(DisplayHeight));
            }
        }

        /// <summary>
        /// If the video should automatically seek to the start of selected lines
        /// </summary>
        public bool IsAutoSeekEnabled
        {
            get => _isAutoSeekEnabled;
            set
            {
                _isAutoSeekEnabled = value;
                OnPropertyChanged(nameof(IsAutoSeekEnabled));
            }
        }

        /// <summary>
        /// If the video is currently playing
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        /// <summary>
        /// If the video is paused, and will resume
        /// playing to the current destination
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                _isPaused = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        }

        /// <summary>
        /// Stop video playback
        /// </summary>
        public void StopPlaying()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            _playback.Stop();
        }

        /// <summary>
        /// Start playing until the end of the file
        /// </summary>
        public void PlayToEnd()
        {
            StopPlaying();
            _playbackDestination = _video.FrameCount - 1;
            _playback.IntervalIndex = CurrentFrame;
            _playback.Start();
            IsPlaying = true;
            IsPaused = false;
        }

        /// <summary>
        /// Begin playing at the start of the selection,
        /// and play until the end of the selection
        /// </summary>
        /// <param name="selection">Events to play between</param>
        public void PlaySelection(IEnumerable<Event> selection)
        {
            var startTime = selection.Select(e => e.Start).Min();
            var endTime = selection.Select(e => e.End).Max();
            var startFrame = Math.Min(Math.Max(0, _video.TimeToFrame(startTime)), _video.FrameCount - 1);
            var endFrame = Math.Min(Math.Max(0, _video.TimeToFrame(endTime) - 1), _video.FrameCount - 1);

            CurrentFrame = startFrame;
            _playbackDestination = endFrame;
            _playback.IntervalIndex = CurrentFrame;
            _playback.Start();
            IsPlaying = true;
            IsPaused = false;
        }

        /// <summary>
        /// Resume playing, without changing the end point
        /// </summary>
        public void ResumePlaying()
        {
            _playback.IntervalIndex = CurrentFrame;
            _playback.Start();
            IsPlaying = true;
            IsPaused = false;
        }

        public void PausePlaying()
        {
            IsPaused = true;
            StopPlaying();
        }

        /// <summary>
        /// Seek to the start of an event
        /// </summary>
        /// <param name="e">Event to seek to</param>
        public void SeekTo(Event e)
        {
            // TODO: Added a +1 here because was showing the frame before start. Check back on this!
            CurrentFrame = Math.Min(Math.Max(0, _video.TimeToFrame(e.Start) + 1), _video.FrameCount - 1);
        }

        /// <summary>
        /// Seek to a frame
        /// </summary>
        /// <param name="f"></param>
        public void SeekTo(int f)
        {
            CurrentFrame = Math.Min(Math.Max(0, f), _video.FrameCount - 1);
        }

        /// <summary>
        /// Seek to a time
        /// </summary>
        /// <param name="t"></param>
        public void SeekTo(Time t)
        {
            CurrentFrame = Math.Min(Math.Max(0, _video.TimeToFrame(t)), _video.FrameCount - 1);
        }

        /// <summary>
        /// Move forward one frame, or if the end point has been reached, stop playing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tick(object? sender, HighResolutionTimerElapsedEventArgs e)
        {
            if (CurrentFrame < _playbackDestination) CurrentFrame++;
            else StopPlaying();
        }

        public PlaybackController(ref VideoWrapper video)
        {
            _video = video;

            _currentFrame = 0;
            DisplayScale = ScalePercentage.VS_50;
            // TODO: Update timer to use actual frame times instead 
            // of "frame rate"!!
            _playback = new HighResolutionTimer((float)_video.MsPerFrame);
            _playback.Elapsed += Tick;

            StopPlaying();
            IsPaused = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
