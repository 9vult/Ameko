// SPDX-License-Identifier: MPL-2.0

using AssCS;
using NLog;

namespace Holo.Media;

public class MediaController : BindableBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ISourceProvider _provider;
    private readonly HighResolutionTimer _playback;
    private VideoInfo _videoInfo;

    private bool _isVideoLoaded;

    private bool _isAutoSeekEnabled = true;
    private bool _isPlaying;
    private bool _isPaused;
    private int _currentFrame;
    private int _destinationFrame;

    /// <summary>
    /// Information about the loaded video
    /// </summary>
    public VideoInfo VideoInfo
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
        set => SetProperty(ref _currentFrame, value);
    }

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
        CurrentFrame = Math.Clamp(frameNumber, 0, _videoInfo.FrameCount - 1);
    }

    /// <summary>
    /// Seek to a frame by time
    /// </summary>
    /// <param name="time">Time to seek to</param>
    public void SeekTo(Time time)
    {
        CurrentFrame = _videoInfo.FrameFromTime(time);
    }

    /// <summary>
    /// Seek to a frame by event
    /// </summary>
    /// <param name="event">Event to seek to the start of</param>
    public void SeekTo(Event @event)
    {
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

        VideoInfo = new VideoInfo
        {
            FrameCount = _provider.FrameCount,
            Sar = new Rational { Numerator = 1, Denominator = 1 },
            FrameTimes = _provider.GetTimecodes(),
            FrameIntervals = _provider.GetFrameIntervals(),
            Keyframes = _provider.GetKeyframes(),
        };
        IsVideoLoaded = true;
        return true;
    }

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
    /// Controls playback
    /// </summary>
    /// <param name="provider">Source Provider to use</param>
    /// <param name="videoInfo">Information about the video</param>
    public MediaController(ISourceProvider provider)
    {
        _provider = provider;
    }
}
