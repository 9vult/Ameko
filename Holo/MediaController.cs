// SPDX-License-Identifier: MPL-2.0

using AssCS;
using AssCS.IO;
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

    private unsafe FrameGroup* _lastFrame;
    private unsafe FrameGroup* _nextFrame;
    private unsafe AudioFrame* _audioFrame;
    private int _currentFrame;
    private readonly Lock _frameLock = new();
    private Task? _fetchTask;
    private int _pendingFrame = -1;
    private bool _subtitlesChanged = false;

    private ScaleFactor _scaleFactor = ScaleFactor.Default;
    private double _displayWidth;
    private double _displayHeight;
    private int _displayAngle;

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
            DisplayWidth = (VideoInfo?.Width ?? 1) * _scaleFactor.Multiplier;
            DisplayHeight = (VideoInfo?.Height ?? 1) * _scaleFactor.Multiplier;
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

    public int DisplayAngle
    {
        get => _displayAngle;
        set
        {
            value = (value % 360 + 360) % 360; // Normalize to [0-359]
            SetProperty(ref _displayAngle, value);
        }
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
            RequestFrame(value);
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
        Logger.Debug("Stopping playback");
        if (!IsPlaying)
            return;
        IsPlaying = false;
        _playback.Stop();
        OnPlaybackStop?.Invoke(this, EventArgs.Empty);
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
        Logger.Debug("Playing to end");
        _destinationFrame = _videoInfo.FrameCount - 1;
        _playback.IntervalIndex = _currentFrame;

        var e = new PlaybackStartEventArgs(
            _videoInfo.MillisecondsFromFrame(_currentFrame),
            _videoInfo.MillisecondsFromFrame(_destinationFrame)
        );
        OnPlaybackStart?.Invoke(this, e);

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
        Logger.Debug($"Playing selection [{startTime}, {endTime}]");

        if (startTime is null || endTime is null)
            return;

        var startFrame = _videoInfo.FrameFromTime(startTime);
        var endFrame = _videoInfo.FrameFromTime(endTime) - 1; // Stop on the last frame including the selection

        CurrentFrame = startFrame;
        _destinationFrame = endFrame;
        _playback.IntervalIndex = _currentFrame;

        var e = new PlaybackStartEventArgs(
            _videoInfo.MillisecondsFromFrame(_currentFrame),
            _videoInfo.MillisecondsFromFrame(_destinationFrame)
        );
        OnPlaybackStart?.Invoke(this, e);

        _playback.Start();
        IsPlaying = true;
        IsPaused = false;
    }

    /// <summary>
    /// Resume playback
    /// </summary>
    public void Resume()
    {
        if (_videoInfo is null)
            throw new InvalidOperationException("Video is not loaded");

        Logger.Debug("Resuming playback");
        _playback.IntervalIndex = _currentFrame;

        OnPlaybackStart?.Invoke(
            this,
            new PlaybackStartEventArgs(
                _videoInfo.MillisecondsFromFrame(_currentFrame),
                _videoInfo.MillisecondsFromFrame(_destinationFrame)
            )
        );

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
    /// Seek to the first frame of an event
    /// </summary>
    /// <param name="event">Event to seek to the start of</param>
    public void SeekTo(Event @event)
    {
        if (_videoInfo is not null)
            CurrentFrame = _videoInfo.FrameFromTime(@event.Start);
    }

    /// <summary>
    /// Seek to the last frame of an event
    /// </summary>
    /// <param name="event">Event to seek to the end of</param>
    public void SeekToEnd(Event @event)
    {
        if (_videoInfo is not null)
            CurrentFrame = _videoInfo.FrameFromTime(@event.End) - 1;
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

    /// <summary>
    /// Open a video
    /// </summary>
    /// <param name="filePath">Path to the video to open</param>
    /// <returns><see langword="true"/> if successful</returns>
    /// <exception cref="InvalidOperationException">If the provider isn't initialized</exception>
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

        if (_provider.AllocateBuffers(64, 32) != 0)
        {
            return false;
        }

        unsafe
        {
            var testFrame = _provider.GetFrame(0, 0, true);
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
                testFrame->VideoFrame->Width,
                testFrame->VideoFrame->Height
            );

            _playback.Intervals = VideoInfo.FrameIntervals;
        }

        DisplayWidth = VideoInfo.Width;
        DisplayHeight = VideoInfo.Height;

        // Audio time
        if (_provider.AllocateAudioBuffer() != 0)
        {
            return false; // ??
        }

        unsafe
        {
            _audioFrame = _provider.GetAudio();
            if (_audioFrame->Valid != 1)
            {
                return false; // ??
            }
        }

        IsVideoLoaded = true;

        // Re-fetch frame 0 with subtitles
        unsafe
        {
            _lastFrame = _provider.GetFrame(0, 0, false);
        }
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
    /// Get the current frame
    /// </summary>
    /// <returns>Pointer to the frame</returns>
    /// <exception cref="InvalidOperationException">If there is no frame</exception>
    public unsafe FrameGroup* GetCurrentFrame()
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");
        if (_videoInfo is null)
            throw new InvalidOperationException("Video is not loaded");

        lock (_frameLock)
        {
            if (_nextFrame is not null)
            {
                _lastFrame = _nextFrame;
                _nextFrame = null;
            }
        }

        if (_lastFrame is not null)
            return _lastFrame;

        throw new InvalidOperationException("Frame is unavailable");
    }

    public unsafe AudioFrame* GetAudioFrame()
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");
        if (_videoInfo is null)
            throw new InvalidOperationException("Video is not loaded");
        return _audioFrame;
    }

    /// <summary>
    /// Set the subtitles to be displayed
    /// </summary>
    /// <param name="document">Document being displayed</param>
    /// <exception cref="InvalidOperationException">If the provider isn't initialized</exception>
    public void SetSubtitles(Document document)
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");
        if (_videoInfo is null)
            return;

        // TODO: preferably not create a new writer on each change
        var writer = new AssWriter(document, new ConsumerInfo("", "", ""));
        lock (_frameLock)
        {
            _provider.SetSubtitles(writer.Write(false), null);
            _subtitlesChanged = true;
        }

        RequestFrame(CurrentFrame);
    }

    /// <summary>
    /// Queue a request for a frame
    /// </summary>
    /// <param name="fetchingFrame">Frame number to fetch</param>
    private void RequestFrame(int fetchingFrame)
    {
        lock (_frameLock)
        {
            _pendingFrame = fetchingFrame;
            if (_fetchTask is null || _fetchTask.IsCompleted)
            {
                _fetchTask = Task.Run(FetchFrame);
            }
        }
    }

    /// <summary>
    /// Fetch a frame
    /// </summary>
    private unsafe void FetchFrame()
    {
        int frameToFetch;
        lock (_frameLock)
        {
            frameToFetch = _pendingFrame;
            _pendingFrame = -1;
            _subtitlesChanged = false;
        }

        var time = _videoInfo?.MillisecondsFromFrame(frameToFetch) ?? 0;

        var frame = _provider.GetFrame(frameToFetch, time, false);
        lock (_frameLock)
        {
            // if (frameToFetch == _currentFrame || subtitlesChanged) // Do we want this gate?
            _nextFrame = frame;

            if (_pendingFrame != -1 || _subtitlesChanged)
                _fetchTask = Task.Run(FetchFrame);
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

    public event EventHandler<PlaybackStartEventArgs>? OnPlaybackStart;
    public event EventHandler<EventArgs>? OnPlaybackStop;
}
