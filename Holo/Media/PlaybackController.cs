// SPDX-License-Identifier: MPL-2.0

using AssCS;
using NLog;

namespace Holo.Media;

public class PlaybackController : BindableBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ISourceProvider _provider;
    private readonly HighResolutionTimer _playback;
    private VideoInfo _videoInfo;
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
        set => SetProperty(ref _videoInfo, value);
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
    /// If we should automatically seek to the start of an event when the selection changes
    /// </summary>
    public bool IsAutoSeekEnabled
    {
        get => _isAutoSeekEnabled;
        set => SetProperty(ref _isAutoSeekEnabled, value);
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
    public PlaybackController(ISourceProvider provider, VideoInfo videoInfo)
    {
        _provider = provider;
        _videoInfo = videoInfo;
        _playback = new HighResolutionTimer(videoInfo.FrameIntervals);
    }
}
