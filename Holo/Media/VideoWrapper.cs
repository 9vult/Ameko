using AssCS;
using Holo.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Holo.Media
{
    public class VideoWrapper : INotifyPropertyChanged
    {
        private int _frameCount;
        private Rational _sar;
        private Rational _frameRate;

        private long[] _frametimes;
        private float[] _frameIntervals;
        private double _msPerFrame;

        /// <summary>
        /// Number of frames in the video
        /// </summary>
        public int FrameCount
        {
            get => _frameCount;
            private set
            {
                _frameCount = value;
                OnPropertyChanged(nameof(FrameCount));
                OnPropertyChanged(nameof(FrameCountZeroIndex));
            }
        }

        /// <summary>
        /// Rational aspect ratio
        /// </summary>
        public Rational SAR
        {
            get => _sar;
            private set
            {
                _sar = value;
                OnPropertyChanged(nameof(SAR));
            }
        }

        /// <summary>
        /// Rational frame rate. For reference use only.
        /// </summary>
        public Rational FrameRate
        {
            get => _frameRate;
            private set
            {
                _frameRate = value;
                OnPropertyChanged(nameof(FrameRate));
                OnPropertyChanged(nameof(FrameRateCeiling));
            }
        }

        /// <summary>
        /// How long each frame is to be displayed for
        /// </summary>
        public float[] FrameIntervals => _frameIntervals;

        /// <summary>
        /// Milliseconds per frame (estimated)
        /// </summary>
        public double MsPerFrame => _msPerFrame;

        // Methods

        /// <summary>
        /// Get the frame closest to the milliseconds provided
        /// </summary>
        /// <remarks>
        /// With X milliseconds per frame, this should return 0 for:
        /// <list type="bullet">
        /// <item>Exact: [0, X - 1]</item>
        /// <item>Start: [1 - X, 0]</item>
        /// <item>End: [1, X]</item>
        /// </list>
        /// There are two properties we take advantage of here:
        /// <list type="number">
        /// <item>Start and End's ranges are adjacent, meaning doing
        /// the calculations for End and adding one gives us Start.</item>
        /// <item>End is Exact plus one millisecond, meaning we can subtract one millisecond to get Exact</item>
        /// </list>
        /// Combining these allows us to easily calculate Start and End in terms of Exact.
        /// </remarks>
        /// <param name="milliseconds"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int MillisToFrame(long milliseconds, FrameTimeType type = FrameTimeType.Exact)
        {
            if (type == FrameTimeType.Start) return MillisToFrame(milliseconds - 1) + 1;
            if (type == FrameTimeType.End) return MillisToFrame(milliseconds - 1);

            if (milliseconds < _frametimes[0])
                return (int)((milliseconds * FrameRate.Numerator / FrameRate.Denominator - 999) / 1000);

            if (milliseconds > _frametimes.Last())
                return (int)((milliseconds * FrameRate.Numerator - FrameRate.Numerator / 2 - _frametimes.Last() + FrameRate.Numerator - 1) / FrameRate.Denominator / 1000) + _frametimes.Length - 1;

            var bs = Array.BinarySearch(_frametimes, milliseconds);
            if (bs >= 0) return bs;
            if (~bs == _frametimes.Length + 1) return _frametimes.Length;
            return ~bs - 1; // ~bs → index of first greater element. -1 because we want the smaller element
        }

        /// <summary>
        /// Get the milliseconds for a frame
        /// </summary>
        /// <param name="frame">Frame number</param>
        /// <returns>Milliseconds for the frame</returns>
        public long FrameToMillis(int frame, FrameTimeType type = FrameTimeType.Exact)
        {
            if (type == FrameTimeType.Start)
            {
                var prev = FrameToMillis(frame - 1);
                var cur = FrameToMillis(frame);
                return prev + (cur - prev + 1) / 2;
            }

            if (type == FrameTimeType.End)
            {
                var cur = FrameToMillis(frame);
                var next = FrameToMillis(frame + 1);
                return cur + (next - cur + 1) / 2;
            }

            if (frame < 0) return frame * FrameRate.Denominator * 1000 / FrameRate.Numerator;

            if (frame >= _frametimes.Length)
            {
                long framesPastEnd = frame - _frametimes.Length + 1;
                return (framesPastEnd * 1000 * FrameRate.Denominator + _frametimes.Last() + FrameRate.Numerator / 2) / FrameRate.Numerator;
            }

            return _frametimes[frame];
        }

        /// <summary>
        /// Get the frame closest to a Time value (Rounding Method)
        /// </summary>
        /// <param name="time">Time to get the frame at</param>
        /// <returns>Frame number</returns>
        public int TimeToFrame(Time time, FrameTimeType type = FrameTimeType.Exact)
        {
            return MillisToFrame(time.TotalMilliseconds, type);
        }

        /// <summary>
        /// Get a Time for a frame
        /// </summary>
        /// <param name="frame">Frame number</param>
        /// <returns>Time object set to the time of the frame</returns>
        public Time FrameToTime(int frame, FrameTimeType type = FrameTimeType.Exact)
        {
            return Time.FromMillis(FrameToMillis(frame, type));
        }

        // Extras for GUI support
        internal int FrameCountZeroIndex => _frameCount - 1;
        internal int FrameRateCeiling => (int)Math.Ceiling(_frameRate.Ratio);

        public VideoWrapper(int frameCount, Rational sar, Rational frameRate, long[] frametimes, float[] frameIntervals)
        {
            _frameCount = frameCount;
            _sar = sar;
            _frameRate = frameRate;
            _frametimes = frametimes;
            _frameIntervals = frameIntervals;
            _msPerFrame = 1 / _frameRate.Ratio * 1000;
        }

        internal void Scaffold(int frameCount, Rational sar, Rational frameRate, long[] frametimes, float[] frameIntervals)
        {
            FrameCount = frameCount;
            SAR = sar;
            FrameRate = frameRate;
            _frametimes = frametimes;
            _frameIntervals = frameIntervals;
            _msPerFrame = 1 / _frameRate.Ratio * 1000;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
