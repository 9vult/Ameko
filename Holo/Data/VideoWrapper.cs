using AssCS;
using Holo.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;

namespace Holo.Data
{
    public class VideoWrapper : INotifyPropertyChanged
    {
        private int _currentFrame;
        private readonly int _frameCount;
        private readonly Rational _sar;
        private readonly Rational _frameRate;
        private readonly double _msPerFrame;

        private double _displayWidth;
        private double _displayHeight;
        private ScalePercentage _displayScale = ScalePercentage.VS_100;

        private bool _isPlaying;
        private bool _isPaused;
        private Timer _playback;
        private int _playbackDestination;

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
        public Time CurrentTimeEstimated => FrameToTime(_currentFrame);

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
                DisplayWidth = value.Multiplier * SAR.Numerator;
                DisplayHeight = value.Multiplier * SAR.Denominator;
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

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        }

        // Primaries
        public int FrameCount => _frameCount;
        public Rational SAR => _sar;
        public Rational FrameRate => _frameRate;

        // Methods

        /// <summary>
        /// Get the frame closest to a millisecond value (Rounding Method)
        /// </summary>
        /// <remarks>Based on https://github.com/moi15moi/PyonFX/blob/FrameUtility-V3/docs/Proof%20algorithm%20-%20ms_to_frames.md</remarks>
        /// <param name="milliseconds">Milliseconds</param>
        /// <returns>Frame number</returns>
        public int MillisToFrame(long milliseconds)
        {
            return (int)Math.Round((milliseconds + 0.5) * _frameRate.Ratio * (1 / (double)1000));
        }

        /// <summary>
        /// Get the milliseconds for a frame
        /// </summary>
        /// <param name="frame">Frame number</param>
        /// <returns>Milliseconds for the frame</returns>
        public long FrameToMillis(int frame)
        {
            return (long)(frame / _frameRate.Ratio * 1000);
        }

        /// <summary>
        /// Get the frame closest to a Time value (Rounding Method)
        /// </summary>
        /// <param name="time">Time to get the frame at</param>
        /// <returns>Frame number</returns>
        public int TimeToFrame(Time time)
        {
            return MillisToFrame(time.TotalMilliseconds);
        }

        /// <summary>
        /// Get a Time for a frame
        /// </summary>
        /// <param name="frame">Frame number</param>
        /// <returns>Time object set to the time of the frame</returns>
        public Time FrameToTime(int frame)
        {
            return Time.FromMillis(FrameToMillis(frame));
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
            _playbackDestination = FrameCount - 1;
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
            var startFrame = Math.Min(Math.Max(0, TimeToFrame(startTime)), FrameCount - 1);
            var endFrame = Math.Min(Math.Max(0, TimeToFrame(endTime) - 1), FrameCount - 1);

            CurrentFrame = startFrame;
            _playbackDestination = endFrame;
            _playback.Start();
            IsPlaying = true;
            IsPaused = false;
        }

        /// <summary>
        /// Resume playing, without changing the end point
        /// </summary>
        public void ResumePlaying()
        {
            _playback.Start();
            IsPlaying = true;
            IsPaused = false;
        }

        /// <summary>
        /// Seek to the start of an event
        /// </summary>
        /// <param name="e">Event to seek to</param>
        public void SeekTo(Event e)
        {
            CurrentFrame = Math.Min(Math.Max(0, TimeToFrame(e.Start)), FrameCount - 1);
        }

        /// <summary>
        /// Move forward one frame, or if the end point has been reached, stop playing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tick(object sender, ElapsedEventArgs e)
        {
            if (CurrentFrame < _playbackDestination) CurrentFrame++;
            else StopPlaying();
        }


        // Extras for GUI support
        public int __FrameCountZeroIndex => _frameCount - 1;
        public int __FrameRateCeiling => (int)Math.Ceiling(_frameRate.Ratio);

        public VideoWrapper(int frameCount, Rational sar, Rational frameRate)
        {
            _currentFrame = 0;
            _frameCount = frameCount;
            _sar = sar;
            _frameRate = frameRate;
            _msPerFrame = 1 / _frameRate.Ratio * 1000;
            DisplayScale = ScalePercentage.VS_50;
            _playback = new Timer(_msPerFrame);
            _playback.Elapsed += Tick;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
