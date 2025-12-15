// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using AssCS;
using AssCS.IO;
using Holo.Media;
using Holo.Media.Providers;
using Microsoft.Extensions.Logging;

namespace Holo;

public class MediaController : BindableBase
{
    private readonly ISourceProvider _provider;
    private readonly ILogger _logger;
    private readonly HighResolutionTimer _playback;

    private readonly Lock _frameLock = new();
    private readonly Lock _boundsLock = new();

    private unsafe FrameGroup* _lastFrame;
    private unsafe FrameGroup* _nextFrame;
    private unsafe AudioFrame* _audioFrame;
    private unsafe Bitmap* _lastVizFrame;
    private unsafe Bitmap* _nextVizFrame;
    private int _currentFrame;

    private Task? _fetchTask;
    private int _pendingFrame = -1;
    private bool _subtitlesChanged;

    private ScaleFactor _scaleFactor = ScaleFactor.Default;
    private double _screenScaleFactor = 1.0d;

    private bool _isPlaying;
    private bool _isPaused;

    private int _destinationFrame;

    private long[] _eventBounds = [];

    /// <summary>
    /// If media operations are to be enabled
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Information about the loaded video
    /// </summary>
    public VideoInfo? VideoInfo
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Information about the loaded audio
    /// </summary>
    public AudioInfo? AudioInfo
    {
        get;
        private set => SetProperty(ref field, value);
    }

    [MemberNotNullWhen(true, nameof(VideoInfo))]
    public bool IsVideoLoaded
    {
        get;
        private set => SetProperty(ref field, value);
    }

    [MemberNotNullWhen(true, nameof(AudioInfo))]
    public bool IsAudioLoaded
    {
        get;
        private set => SetProperty(ref field, value);
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
            DisplayWidth = (VideoInfo?.Width ?? 1) * _scaleFactor.Multiplier / _screenScaleFactor;
            DisplayHeight = (VideoInfo?.Height ?? 1) * _scaleFactor.Multiplier / _screenScaleFactor;
            FrameReady?.Invoke();
        }
    }

    public RotationalFactor RotationalFactor
    {
        get;
        set
        {
            SetProperty(ref field, value);
            DisplayAngle = value.Angle;
        }
    } = RotationalFactor.Default;

    /// <summary>
    /// Screen scale factor (125% scale = 1.25)
    /// </summary>
    public double ScreenScaleFactor
    {
        get => _screenScaleFactor;
        set
        {
            SetProperty(ref _screenScaleFactor, value);
            DisplayWidth = (VideoInfo?.Width ?? 1) * _scaleFactor.Multiplier / _screenScaleFactor;
            DisplayHeight = (VideoInfo?.Height ?? 1) * _scaleFactor.Multiplier / _screenScaleFactor;
        }
    }

    public double DisplayWidth
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public double DisplayHeight
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public double DisplayAngle
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public int VisualizerWidth
    {
        get;
        set
        {
            SetProperty(ref field, value);
            if (IsVideoLoaded)
                RequestFrame(CurrentFrame);
        }
    }

    public int VisualizerHeight
    {
        get;
        set
        {
            SetProperty(ref field, value);
            if (IsVideoLoaded)
                RequestFrame(CurrentFrame);
        }
    }

    public double VisualizerHorizontalScale
    {
        get;
        set
        {
            SetProperty(ref field, value);
            if (IsVideoLoaded)
                RequestFrame(CurrentFrame);
        }
    } = 4d;

    public double VisualizerVerticalScale
    {
        get;
        set
        {
            SetProperty(ref field, value);
            if (IsVideoLoaded)
                RequestFrame(CurrentFrame);
        }
    } = 2d;

    public long VisualizerPositionMs
    {
        get;
        set
        {
            if (value <= 0)
                value = 0;
            if (value >= (AudioInfo?.Duration ?? 0))
                value = AudioInfo?.Duration ?? 0;

            SetProperty(ref field, value);
            if (IsVideoLoaded)
                RequestFrame(CurrentFrame);
        }
    } = 0;

    /// <summary>
    /// If we should automatically seek to the start of an event when the selection changes
    /// </summary>
    public bool IsAutoSeekEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

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
    public Time? CurrentTime => VideoInfo?.TimeFromFrame(CurrentFrame);

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
        _logger.LogDebug("Stopping playback");
        if (!IsPlaying)
            return;
        IsPlaying = false;
        IsPaused = false;
        _playback.Stop();
        OnPlaybackStop?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Pause playback
    /// </summary>
    public void Pause()
    {
        _logger.LogDebug("Pausing playback");
        if (!IsPlaying)
            return;
        IsPaused = true;
        IsPlaying = false;
        _playback.Stop();
        OnPlaybackStop?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Play from the current position to the end of the video
    /// </summary>
    public void PlayToEnd()
    {
        if (!IsVideoLoaded)
            throw new InvalidOperationException("Video is not loaded");

        Stop();
        _logger.LogDebug("Playing to end");
        _destinationFrame = VideoInfo.FrameCount - 1;
        _playback.IntervalIndex = _currentFrame;

        var e = new PlaybackStartEventArgs(
            VideoInfo.MillisecondsFromFrame(_currentFrame),
            VideoInfo.MillisecondsFromFrame(_destinationFrame)
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
        if (!IsVideoLoaded)
            throw new InvalidOperationException("Video is not loaded");

        var startTime = selection.Min(e => e.Start);
        var endTime = selection.Max(e => e.End);
        _logger.LogDebug("Playing selection [{StartTime}, {EndTime}]", startTime, endTime);

        if (startTime is null || endTime is null)
            return;

        var startFrame = VideoInfo.FrameFromTime(startTime);
        var endFrame = VideoInfo.FrameFromTime(endTime) - 1; // Stop on the last frame including the selection

        CurrentFrame = startFrame;
        _destinationFrame = endFrame;
        _playback.IntervalIndex = _currentFrame;

        var e = new PlaybackStartEventArgs(
            VideoInfo.MillisecondsFromFrame(_currentFrame),
            VideoInfo.MillisecondsFromFrame(_destinationFrame)
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
        if (!IsVideoLoaded)
            throw new InvalidOperationException("Video is not loaded");

        _logger.LogDebug("Resuming playback");
        _playback.IntervalIndex = _currentFrame;

        OnPlaybackStart?.Invoke(
            this,
            new PlaybackStartEventArgs(
                VideoInfo.MillisecondsFromFrame(_currentFrame),
                VideoInfo.MillisecondsFromFrame(_destinationFrame)
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
        if (!IsVideoLoaded)
            return;
        if (_isPlaying)
            Pause();
        CurrentFrame = Math.Clamp(frameNumber, 0, VideoInfo.FrameCount - 1);
        if (_isPaused)
            Resume();
    }

    /// <summary>
    /// Seek to a frame by time
    /// </summary>
    /// <param name="time">Time to seek to</param>
    public void SeekTo(Time time)
    {
        if (!IsVideoLoaded)
            return;
        if (_isPlaying)
            Pause();
        CurrentFrame = VideoInfo.FrameFromTime(time);
        if (_isPaused)
            Resume();
    }

    /// <summary>
    /// Seek to the first frame of an event
    /// </summary>
    /// <param name="event">Event to seek to the start of</param>
    public void SeekTo(Event @event)
    {
        if (!IsVideoLoaded)
            return;
        if (_isPlaying)
            Pause();
        CurrentFrame = VideoInfo.FrameFromTime(@event.Start);
        VisualizerPositionMs = @event.Start.TotalMilliseconds;
        if (_isPaused)
            Resume();
    }

    /// <summary>
    /// Seek to the last frame of an event
    /// </summary>
    /// <param name="event">Event to seek to the end of</param>
    public void SeekToEnd(Event @event)
    {
        if (!IsVideoLoaded)
            return;
        if (_isPlaying)
            Pause();
        CurrentFrame = VideoInfo.FrameFromTime(@event.End) - 1;
        if (_isPaused)
            Resume();
    }

    /// <summary>
    /// Seek to a frame by event if <see cref="IsAutoSeekEnabled"/>
    /// </summary>
    /// <param name="event">Event to seek to the start of</param>
    public void AutoSeekTo(Event @event)
    {
        if (!IsVideoLoaded || !IsAutoSeekEnabled)
            return;
        if (_isPlaying)
            Pause();
        CurrentFrame = VideoInfo.FrameFromTime(@event.Start);
        VisualizerPositionMs = @event.Start.TotalMilliseconds;
        if (_isPaused)
            Resume();
    }

    /// <summary>
    /// Open a video file
    /// </summary>
    /// <param name="filePath">Path to the video to open</param>
    /// <param name="progressCallback">Indexing progress callback (optional)</param>
    /// <returns><see langword="true"/> if successful</returns>
    /// <exception cref="InvalidOperationException">If the provider isn't initialized</exception>
    public async Task<bool> OpenVideoAsync(
        string filePath,
        ISourceProvider.IndexingProgressCallback? progressCallback = null
    )
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");

        _logger.LogInformation("Opening video {FilePath}", filePath);

        if (IsVideoLoaded)
            CloseVideo();

        return await Task.Run(() =>
        {
            if (_provider.LoadVideo(filePath, progressCallback) != 0)
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
                    path: filePath,
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

            DisplayWidth = VideoInfo.Width / _screenScaleFactor;
            DisplayHeight = VideoInfo.Height / _screenScaleFactor;

            IsVideoLoaded = true;

            // Re-fetch frame 0 with subtitles
            unsafe
            {
                _lastFrame = _provider.GetFrame(0, 0, false);
            }

            return true;
        });
    }

    /// <summary>
    /// Open an audio file
    /// </summary>
    /// <param name="filePath">Path to the audio to open</param>
    /// <param name="trackNumber">Track number to load</param>
    /// <param name="progressCallback">Indexing progress callback (optional)</param>
    /// <returns><see langword="true"/> if successful</returns>
    /// <exception cref="InvalidOperationException">If the provider isn't initialized</exception>
    public async Task<bool> OpenAudioAsync(
        string filePath,
        int trackNumber,
        ISourceProvider.IndexingProgressCallback? progressCallback = null
    )
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");

        _logger.LogInformation("Opening audio {FilePath}", filePath);
        return await Task.Run(() =>
        {
            if (_provider.LoadAudio(filePath, trackNumber) != 0)
            {
                // TODO: Handle error
                return false;
            }

            // Audio time
            if (_provider.AllocateAudioBuffer() != 0)
            {
                return false; // ??
            }

            unsafe
            {
                _audioFrame = _provider.GetAudio(progressCallback);
                if (_audioFrame->Valid != 1)
                {
                    return false; // ??
                }

                AudioInfo = new AudioInfo(
                    path: filePath,
                    channelCount: _provider.GetChannelCount(),
                    sampleRate: _provider.GetSampleRate(),
                    sampleCount: _provider.GetSampleCount()
                );

                IsAudioLoaded = true;

                _lastVizFrame = _provider.GetVisualization(
                    VisualizerWidth,
                    VisualizerHeight,
                    VisualizerHorizontalScale,
                    VisualizerVerticalScale,
                    0,
                    0,
                    null,
                    0
                );
            }

            return true;
        });
    }

    /// <summary>
    /// Get information about the audio tracks in a file
    /// </summary>
    /// <param name="filePath">Path to the file potentially containing audio tracks</param>
    /// <returns>Array of track information</returns>
    /// <exception cref="InvalidOperationException">If the provider is not initialized</exception>
    public async Task<TrackInfo[]> GetAudioTrackInfoAsync(string filePath)
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");
        _logger.LogInformation("Getting audio track information for {FilePath}", filePath);

        return await Task.Run(() => _provider.GetAudioTrackInfo(filePath));
    }

    /// <summary>
    /// Close the open video (includes audio)
    /// </summary>
    /// <returns><see langword="true"/> if successful</returns>
    /// <exception cref="InvalidOperationException">If the provider isn't initialized</exception>
    public bool CloseVideo()
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");

        if (!IsVideoLoaded)
            return true;

        Stop();
        IsVideoLoaded = false;
        IsAudioLoaded = false;

        // Close
        _logger.LogInformation("Closing video {FilePath}", VideoInfo?.Path);
        var result = _provider.CloseVideo() == 0;

        // Reset
        ScaleFactor = ScaleFactor.Default;
        RotationalFactor = RotationalFactor.Default;

        // Reset the slider without triggering frame fetch
        _currentFrame = 0;
        RaisePropertyChanged(nameof(CurrentFrame));
        RaisePropertyChanged(nameof(CurrentTime));

        return result;
    }

    /// <summary>
    /// Close the open audio
    /// </summary>
    /// <returns><see langword="true"/> if successful</returns>
    /// <exception cref="InvalidOperationException">If the provider isn't initialized</exception>
    public bool CloseAudio()
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");

        if (!IsAudioLoaded)
            return true;

        Stop();
        IsAudioLoaded = false;

        _logger.LogInformation("Closing audio {FilePath}", AudioInfo?.Path);
        return _provider.CloseAudio() == 0;
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
        if (!IsVideoLoaded)
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

    /// <summary>
    /// Get the current visualization frame
    /// </summary>
    /// <returns>Pointer to the frame</returns>
    /// <exception cref="InvalidOperationException">If there is no frame</exception>
    public unsafe Bitmap* GetCurrentVizFrame()
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");
        if (!IsAudioLoaded)
            throw new InvalidOperationException("Audio is not loaded");

        lock (_frameLock)
        {
            if (_nextVizFrame is not null)
            {
                _lastVizFrame = _nextVizFrame;
                _nextVizFrame = null;
            }
        }

        if (_lastVizFrame is not null)
            return _lastVizFrame;

        throw new InvalidOperationException("Frame is unavailable");
    }

    /// <summary>
    /// Get the audio frame
    /// </summary>
    /// <returns>Pointer to the frame</returns>
    /// <exception cref="InvalidOperationException">If there is no frame</exception>
    public unsafe AudioFrame* GetAudioFrame()
    {
        if (!_provider.IsInitialized)
            throw new InvalidOperationException("Provider is not initialized");
        if (!IsAudioLoaded)
            throw new InvalidOperationException("Audio is not loaded");
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
        if (!IsVideoLoaded)
            return;

        // TODO: preferably not create a new writer on each change
        var writer = new AssWriter(document, new ConsumerInfo("", "", ""));
        lock (_frameLock)
        {
            _provider.SetSubtitles(writer.Write(), null);
            _subtitlesChanged = true;
        }

        var events = document.EventManager.Events;

        lock (_boundsLock)
        {
            _eventBounds = new long[events.Count * 2];
            for (int i = 0, j = 0; i < events.Count; i++)
            {
                _eventBounds[j++] = events[i].Start.TotalMilliseconds;
                _eventBounds[j++] = events[i].End.TotalMilliseconds;
            }
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

        var time = VideoInfo?.MillisecondsFromFrame(frameToFetch) ?? 0;

        // Get video/subtitles
        var frame = _provider.GetFrame(frameToFetch, time, false);

        // Get audio visualization
        Bitmap* vizFrame = null;

        if (IsAudioLoaded)
        {
            lock (_boundsLock)
            {
                fixed (long* ptr = _eventBounds)
                {
                    vizFrame = _provider.GetVisualization(
                        VisualizerWidth,
                        VisualizerHeight,
                        VisualizerHorizontalScale,
                        VisualizerVerticalScale,
                        VisualizerPositionMs,
                        time,
                        ptr,
                        _eventBounds.Length
                    );
                }
            }
        }

        lock (_frameLock)
        {
            _nextFrame = frame;
            _nextVizFrame = vizFrame;

            FrameReady?.Invoke();

            if (_pendingFrame != -1 || _subtitlesChanged)
                _fetchTask = Task.Run(FetchFrame);
        }
    }

    /// <summary>
    /// Controls playback
    /// </summary>
    /// <param name="provider">Source Provider to use</param>
    /// <param name="logger">Logger to use</param>
    public MediaController(ISourceProvider provider, ILogger<MediaController> logger)
    {
        _provider = provider;
        _logger = logger;
        _playback = new HighResolutionTimer();
        _playback.Elapsed += AdvanceFrame;

        var initResult = _provider.Initialize();
        IsEnabled = initResult == 0;
        if (!IsEnabled)
        {
            _logger.LogWarning("Source provider initialization failed! Disabling media playback.");
        }
    }

    public event Action? FrameReady;
    public event EventHandler<PlaybackStartEventArgs>? OnPlaybackStart;
    public event EventHandler<EventArgs>? OnPlaybackStop;
}
