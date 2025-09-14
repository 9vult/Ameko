// SPDX-License-Identifier: MPL-2.0

using System.Buffers.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Holo.IO;
using NLog;

namespace Holo.Media.Providers;

/// <summary>
/// Interop with the Mizuki source provider
/// </summary>
public unsafe class MizukiSourceProvider : ISourceProvider
{
    private static readonly Logger Logger = LogManager.GetLogger("Mizuki");
    private static readonly External.LogCallback LogDelegate = Log;

    private GlobalContext* _context;

    /// <summary>
    /// If this provider is initialized
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public int FrameCount { get; private set; }

    // <inheritdoc />
    public bool HasAudio { get; private set; }

    /// <inheritdoc />
    public Rational Sar { get; }

    public void Initialize()
    {
        External.SetLoggerCallback(LogDelegate);
        External.Initialize();

        _context = External.CreateContext();
        IsInitialized = true;
    }

    public int LoadVideo(string filename)
    {
        var status = External.LoadVideo(_context, filename, GetCachePath(filename), "bgra");
        if (status == 0)
        {
            FrameCount = External.GetFrameCount(_context);
        }

        return status;
    }

    public int LoadAudio(string filename, int audioTrackNumber = -1)
    {
        var status = External.LoadAudio(
            _context,
            filename,
            GetCachePath(filename),
            audioTrackNumber
        );
        if (status == 0)
        {
            HasAudio = true;
        }

        return status;
    }

    public int CloseVideo()
    {
        return 0;
    }

    public int CloseAudio()
    {
        return 0;
    }

    /// <inheritdoc />
    public int SetSubtitles(string data, string? codePage)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return External.SetSubtitles(_context, bytes, bytes.Length, codePage);
    }

    public int AllocateBuffers(int numBuffers, int maxCacheSize)
    {
        return External.AllocateBuffers(_context, numBuffers, maxCacheSize);
    }

    /// <inheritdoc />
    public int AllocateAudioBuffer()
    {
        return External.AllocateAudioBuffer(_context);
    }

    /// <inheritdoc />
    public int FreeBuffers()
    {
        return External.FreeBuffers(_context);
    }

    /// <inheritdoc />
    public FrameGroup* GetFrame(int frameNumber, long timestamp, bool raw)
    {
        return External.GetFrame(_context, frameNumber, timestamp, raw ? 1 : 0);
    }

    /// <inheritdoc />
    public AudioFrame* GetAudio()
    {
        return External.GetAudio(_context);
    }

    /// <inheritdoc />
    public int ReleaseFrame(FrameGroup* frame)
    {
        return External.ReleaseFrame(frame);
    }

    /// <inheritdoc />
    public int[] GetKeyframes()
    {
        var ptr = External.GetKeyframes(_context);
        return ptr.ToIntArray();
    }

    /// <inheritdoc />
    public long[] GetTimecodes()
    {
        var ptr = External.GetTimecodes(_context);
        return ptr.ToLongArray();
    }

    /// <inheritdoc />
    public long[] GetFrameIntervals()
    {
        var ptr = External.GetFrameIntervals(_context);
        return ptr.ToLongArray();
    }

    public AudioTrack[] GetAudioTracks(string filePath)
    {
        var resultCode = External.ListAudioTracks(filePath, out var nativeArray);

        if (resultCode != 0)
        {
            throw new Exception("Failed to list audio tracks");
        }

        try
        {
            var tracks = new AudioTrack[nativeArray.Length];
            var trackSize = Marshal.SizeOf<AudioTrackC>();

            for (var i = 0; i < (int)nativeArray.Length; i++)
            {
                var trackPtr = nativeArray.Pointer + (i * trackSize);
                var trackC = Marshal.PtrToStructure<AudioTrackC>(trackPtr);

                tracks[i] = new AudioTrack
                {
                    Index = (int)trackC.Index,
                    Language = trackC.Language,
                    Title = trackC.title,
                };
            }
            return tracks;
        }
        finally
        {
            External.FreeAudioTracks(ref nativeArray);
        }
    }

    /// <summary>
    /// Callback for handling logs emitted by Mizuki
    /// </summary>
    /// <param name="level">Log level</param>
    /// <param name="ptr">Pointer to the c-string</param>
    private static void Log(int level, nint ptr)
    {
        var msg = Marshal.PtrToStringAnsi(ptr);
        switch (level)
        {
            case 0:
                Logger.Trace(msg);
                break;
            case 1:
                Logger.Debug(msg);
                break;
            case 2:
                Logger.Info(msg);
                break;
            case 3:
                Logger.Warn(msg);
                break;
            case 4:
                Logger.Error(msg);
                break;
        }
    }

    /// <summary>
    /// Construct the cache path
    /// </summary>
    /// <param name="filePath">File being loaded</param>
    /// <returns>Filepath</returns>
    private string GetCachePath(string filePath)
    {
        // Try to get the last-modified time
        var modified = File.Exists(filePath)
            ? new FileInfo(filePath).LastWriteTimeUtc.ToString(CultureInfo.InvariantCulture)
            : string.Empty;

        var bytes = MD5.HashData(Encoding.UTF8.GetBytes($"{filePath}-{modified}"));
        var encoded = Base64Url.EncodeToString(bytes);

        return Path.Combine(Directories.CacheHome, $"{encoded}.ffindex");
    }
}

/// <summary>
/// External methods
/// </summary>
internal static unsafe partial class External
{
    [LibraryImport("mizuki")]
    internal static partial void Initialize();

    [LibraryImport("mizuki")]
    internal static unsafe partial GlobalContext* CreateContext();

    [LibraryImport("mizuki")]
    internal static partial void DestroyContext(GlobalContext* context);

    [LibraryImport("mizuki", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int LoadVideo(
        GlobalContext* context,
        string fileName,
        string cacheFileName,
        string colorMatrix
    );

    [LibraryImport("mizuki", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int LoadAudio(
        GlobalContext* context,
        string fileName,
        string cacheFileName,
        int audioTrackNumber
    );

    [LibraryImport("mizuki", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int SetSubtitles(
        GlobalContext* context,
        byte[] data,
        int dataLen,
        string? codePage
    );

    [LibraryImport("mizuki")]
    internal static partial int AllocateBuffers(
        GlobalContext* context,
        int numBuffers,
        int maxCacheSize
    );

    [LibraryImport("mizuki")]
    internal static partial int AllocateAudioBuffer(GlobalContext* context);

    [LibraryImport("mizuki")]
    internal static partial int FreeBuffers(GlobalContext* context);

    [LibraryImport("mizuki")]
    internal static unsafe partial FrameGroup* GetFrame(
        GlobalContext* context,
        int frameNumber,
        long timestamp,
        int raw
    );

    [LibraryImport("mizuki")]
    internal static unsafe partial AudioFrame* GetAudio(GlobalContext* context);

    [LibraryImport("mizuki")]
    internal static unsafe partial int ReleaseFrame(FrameGroup* frame);

    [LibraryImport("mizuki")]
    internal static partial int GetFrameCount(GlobalContext* context);

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetKeyframes(GlobalContext* context);

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetTimecodes(GlobalContext* context);

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetFrameIntervals(GlobalContext* context);

    [LibraryImport("mizuki")]
    internal static partial void SetLoggerCallback(LogCallback callback);

    [LibraryImport("mizuki", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int ListAudioTracks(string filename, out UnmanagedArray audiotracks);

    [LibraryImport("mizuki")]
    public static partial void FreeAudioTracks(ref UnmanagedArray audioTracks);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallback(int level, nint message);
}

/// <summary>
/// Used for contextualization
/// </summary>
internal struct GlobalContext;

/// <summary>
/// An unmanaged integer array
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct UnmanagedArray
{
    public nint Pointer;
    public nuint Length;
}

[StructLayout(LayoutKind.Sequential)]
public struct AudioTrackC
{
    public nuint Index;
    public string Language;
    public string title;
}

internal static class UnmanagedArrayExtensions
{
    /// <summary>
    /// Copy an unmanaged <see cref="UnmanagedArray"/> to a managed <c>int[]</c>
    /// </summary>
    /// <param name="array">Unmanaged array</param>
    /// <returns>Managed array</returns>
    public static int[] ToIntArray(this UnmanagedArray array)
    {
        var managed = new int[array.Length];
        Marshal.Copy(array.Pointer, managed, 0, managed.Length);
        return managed;
    }

    public static long[] ToLongArray(this UnmanagedArray array)
    {
        var managed = new long[array.Length];
        Marshal.Copy(array.Pointer, managed, 0, managed.Length);
        return managed;
    }
}
