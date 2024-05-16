using AssCS;
using Holo.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Holo
{
    public class VideoProperties : INotifyPropertyChanged
    {
        private int _currentFrame;
        private readonly int _frameCount;
        private readonly Rational _sar;
        private readonly Rational _frameRate;

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

        public Time CurrentTimeEstimated
        {
            get
            {
                long milliseconds = (long)((_currentFrame / _frameRate.Ratio) * 1000);
                return Time.FromMillis(milliseconds);
            }
        }

        // Primaries
        public int FrameCount => _frameCount;
        public Rational SAR => _sar;
        public Rational FrameRate => _frameRate;

        // Extras for GUI support
        public int __FrameCountZeroIndex => _frameCount - 1;
        public int __FrameRateCeiling => (int)Math.Ceiling(_frameRate.Ratio);

        public VideoProperties(int frameCount, Rational sar, Rational frameRate)
        {
            _currentFrame = 0;
            _frameCount = frameCount;
            _sar = sar;
            _frameRate = frameRate;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
