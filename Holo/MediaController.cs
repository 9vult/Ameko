// SPDX-License-Identifier: MPL-2.0

using AssCS;
using Holo.Media;
using Holo.Media.Providers;
using NLog;

namespace Holo;

public class MediaController : BindableBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ISourceProvider _provider;
    private readonly HighResolutionTimer _playback;
    private VideoInfo? _videoInfo;

    private bool _isVideoLoaded;

    private ScaleFactor _scaleFactor = ScaleFactor.Default;
    private double _displayWidth;
    private double _displayHeight;

    private bool _isAutoSeekEnabled = true;
    private bool _isPlaying;
    private bool _isPaused;

    private int _destinationFrame;

    /// <summary>
    /// Information about the loaded video
    /// </summary>
    public VideoInfo? VideoInfo
    {
        get => _videoInfo;
        private set => SetProperty(ref _videoInfo, value);
    }

    public bool IsVideoLoaded
    {
        get => _isVideoLoaded;
        private set => SetProperty(ref _isVideoLoaded, value);
    }

    /// <summary>
    /// Scale factor of the viewport
    /// </summary>
    public ScaleFactor ScaleFactor
    {
        get => _scaleFactor;
        set
        {
            SetProperty(ref _scaleFactor, value);
            DisplayWidth = VideoInfo?.Width ?? 1 * _scaleFactor.Multiplier;
            DisplayHeight = VideoInfo?.Height ?? 1 * _scaleFactor.Multiplier;
        }
    }

    public double DisplayWidth
    {
        get => _displayWidth;
        private set => SetProperty(ref _displayWidth, value);
    }

    public double DisplayHeight
    {
        get => _displayHeight;
        private set => SetProperty(ref _displayHeight, value);
    }

    /// <summary>
    /// If we should automatically seek to the start of an event when the selection changes
    /// </summary>
    public bool IsAutoSeekEnabled
    {
        get => _isAutoSeekEnabled;
        set => SetProperty(ref _isAutoSeekEnabled, value);
    }

    /// <summary>
    /// The current frame
    /// </summary>
    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            SetProperty(ref _currentFrame, value);
            RaisePropertyChanged(nameof(CurrentTime));
            OnCurrentFrameChanged(value);
        }
    }

    /// <summary>
    /// The current frame time
    /// </summary>
    public Time? CurrentTime => _videoInfo?.TimeFromFrame(CurrentFrame);

    /// <summary>
    /// If video is currently playing
    /// </summary>
    public bool IsPlaying
    {
        get => _isPlaying;
        private set => SetProperty(ref _isPlaying, value);
    }

    /// <summary>
    /// If playback is paused
    /// </summary>
    public bool IsPaused
    {
        get => _isPaused;
        set => SetProperty(ref _isPaused, value);
    }

    /// <summary>
    /// Stop playback
    /// </summary>
    public void Stop()
    {
        Logger.Trace("Stopping playback");
        if (!IsPlaying)
            return;
        IsPlaying = false;
        _playback.Stop();
    }

    /// <summary>
    /// Pause playback
    /// </summary>
    public void Pause()
    {
        IsPaused = true;
        Stop();
    }

    /// <summary>
    /// Play from the current position to the end of the video
    /// </summary>
    public void PlayToEnd()
    {
        if (_videoInfo is null)
            throw new InvalidOperationException("Video is not loaded");

        Stop();
        Logger.Trace("Playing to end");
        _destinationFrame = _videoInfo.FrameCount - 1;
        _playback.IntervalIndex = _currentFrame;
        _playback.Start();
        IsPlaying = true;
        IsPaused = false;
    }

    /// <summary>
    /// Play a selection
    /// </summary>
    /// <param name="selection"></param>
    public void PlaySelection(IList<Event> selection)
    {
        if (_videoInfo is null)
            throw new InvalidOperationException("Video is not loaded");

        var startTime = selection.Min(e => e.Start);
        var endTime = selection.Max(e => e.End);
        Logger.Trace($"Playing selection [{startTime}, {endTime}]");

        if (startTime is null || endTime is null)
            return;

        var startFrame = _videoInfo.FrameFromTime(startTime);
        var endFrame = _videoInfo.FrameFromTime(endTime);

        CurrentFrame = startFrame;
        _destinationFrame = endFrame;
        _playback.IntervalIndex = _currentFrame;
        _playback.Start();
        IsPlaying = true;
        IsPaused = false;
    }

    /// <summary>
    /// Resume playback
    /// </summary>
    public void Resume()
    {
        Logger.Trace("Resuming playback");
        _playback.IntervalIndex = _currentFrame;
        _playback.Start();
        IsPlaying = true;
        IsPaused = false;
    }

    /// <summary>
    /// Seek to a frame by number
    /// </summary>
    /// <param name="frameNumber">Frame number to seek to</param>
    public void SeekTo(int frameNumber)
    {
        if (_videoInfo is null)
            throw new InvalidOperationException("Video is not loaded");
        CurrentFrame = Math.Clamp(frameNumber, 0, _videoInfo.FrameCount - 1);
    }

    /// <summary>
    /// Seek to a frame by time
    /// </summary>
    /// <param name="time">Time to seek to</param>
    public void SeekTo(Time time)
    {
        if (_videoInfo is not null)
            CurrentFrame = _videoInfo.FrameFromTime(time);
    }

    /// <summary>
    /// Seek to a frame by event
    /// </summary>
    /// <param name="event">Event to seek to the start of</param>
    public void SeekTo(Event @event)
    {
        if (_videoInfo is not null)
            CurrentFrame = _videoInfo.FrameFromTime(@event.Start);
    }

    /// <summary>
    /// Seek to a frame by event if <see cref="IsAutoSeekEnabled"/>
    /// </summary>
    /// <param name="event">Event to seek to the start of</param>
    public void AutoSeekTo(Event @event)
    {
        if (_videoInfo is not null && IsAutoSeekEnabled)
            CurrentFrame = _videoInfo.FrameFromTime(@event.Start);
    }

    public bool OpenVideo(string filePath)
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");

        Logger.Info($"Opening video {filePath}");

        if (IsVideoLoaded)
            _provider.CloseVideo();

        if (_provider.LoadVideo(filePath) != 0)
        {
            // TODO: Handle error
            return false;
        }

        if (_provider.AllocateBuffers(2) != 0)
        {
            return false;
        }

        unsafe
        {
            var testFrame = _provider.GetFrame(0);
            if (testFrame is null)
            {
                // TODO: Handle error
            }

            VideoInfo = new VideoInfo(
                frameCount: _provider.FrameCount,
                sar: new Rational { Numerator = 1, Denominator = 1 },
                frameTimes: _provider.GetTimecodes(),
                frameIntervals: _provider.GetFrameIntervals(),
                keyframes: _provider.GetKeyframes(),
                testFrame->Width,
                testFrame->Height
            );

            _playback.Intervals = VideoInfo.FrameIntervals;
            _lastFrame = testFrame;
        }

        DisplayWidth = VideoInfo.Width;
        DisplayHeight = VideoInfo.Height;
        IsVideoLoaded = true;
        return true;
    }

    private unsafe VideoFrame* _lastFrame;
    private unsafe VideoFrame* _nextFrame;
    private int _currentFrame;
    private readonly object _frameLock = new();
    private Task? _fetchTask;

    /// <summary>
    /// Advance the current frame, or stop if the <see cref="_destinationFrame"/> has been reached
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AdvanceFrame(object? sender, HighResolutionTimerElapsedEventArgs e)
    {
        if (_currentFrame < _destinationFrame)
            CurrentFrame++;
        else
            Stop();
    }

    /// <summary>
    /// Get the current frame
    /// </summary>
    /// <returns>Pointer to the frame</returns>
    /// <exception cref="InvalidOperationException">If there is no frame</exception>
    public unsafe VideoFrame* GetVideoFrame()
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");
        if (_videoInfo is null)
            throw new InvalidOperationException("Video is not loaded");

        lock (_frameLock)
        {
            if (_nextFrame is not null)
            {
                if (_lastFrame is not null)
                    _provider.ReleaseFrame(_lastFrame);

                _lastFrame = _nextFrame;
                _nextFrame = null;
            }
        }

        if (_lastFrame is not null)
            return _lastFrame;

        throw new InvalidOperationException("Frame is unavailable");
    }

    /// <summary>
    /// Asynchronously fetch the next frame
    /// </summary>
    /// <param name="fetchingFrame">Frame number to fetch</param>
    private unsafe void OnCurrentFrameChanged(int fetchingFrame)
    {
        lock (_frameLock)
        {
            if (_fetchTask is null || _fetchTask.IsCompleted)
            {
                _fetchTask = Task.Run(() =>
                {
                    var frame = _provider.GetFrame(fetchingFrame);
                    lock (_frameLock)
                    {
                        if (fetchingFrame == _currentFrame)
                            _nextFrame = frame;
                        else
                            _provider.ReleaseFrame(frame);
                    }
                });
            }
        }
    }

    /// <summary>
    /// Controls playback
    /// </summary>
    /// <param name="provider">Source Provider to use</param>
    public MediaController(ISourceProvider provider)
    {
        _provider = provider;
        _playback = new HighResolutionTimer();
        _playback.Elapsed += AdvanceFrame;
    }
}
