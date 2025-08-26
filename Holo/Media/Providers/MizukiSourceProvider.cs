// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Holo.IO;
using NLog;

namespace Holo.Media.Providers;

/// <summary>
/// Interop with the Mizuki source provider
/// </summary>
public class MizukiSourceProvider : ISourceProvider
{
    private static readonly Logger Logger = LogManager.GetLogger("Mizuki");
    private static readonly External.LogCallback LogDelegate = Log;

    /// <summary>
    /// If this provider is initialized
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public int FrameCount { get; private set; }

    /// <inheritdoc />
    public Rational Sar { get; }

    public void Initialize()
    {
        External.SetLoggerCallback(LogDelegate);
        External.Initialize();
        IsInitialized = true;
    }

    public int LoadVideo(string filename)
    {
        var status = External.LoadVideo(filename, GetCachePath(filename), "bgra");
        if (status == 0)
        {
            FrameCount = External.GetFrameCount();
        }

        return status;
    }

    public int CloseVideo()
    {
        return 0;
    }

    /// <inheritdoc />
    public int SetSubtitles(string data, string? codePage)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return External.SetSubtitles(bytes, bytes.Length, codePage);
    }

    public int AllocateBuffers(int numBuffers, int maxCacheSize)
    {
        return External.AllocateBuffers(numBuffers, maxCacheSize);
    }

    /// <inheritdoc />
    public int AllocateAudioBuffer()
    {
        return External.AllocateAudioBuffer();
    }

    /// <inheritdoc />
    public int FreeBuffers()
    {
        return External.FreeBuffers();
    }

    /// <inheritdoc />
    public unsafe FrameGroup* GetFrame(int frameNumber, long timestamp, bool raw)
    {
        return External.GetFrame(frameNumber, timestamp, raw ? 1 : 0);
    }

    /// <inheritdoc />
    public unsafe AudioFrame* GetAudio()
    {
        return External.GetAudio();
    }

    /// <inheritdoc />
    public unsafe int ReleaseFrame(FrameGroup* frame)
    {
        return External.ReleaseFrame(frame);
    }

    /// <inheritdoc />
    public int[] GetKeyframes()
    {
        var ptr = External.GetKeyframes();
        return ptr.ToIntArray();
    }

    /// <inheritdoc />
    public long[] GetTimecodes()
    {
        var ptr = External.GetTimecodes();
        return ptr.ToLongArray();
    }

    /// <inheritdoc />
    public long[] GetFrameIntervals()
    {
        var ptr = External.GetFrameIntervals();
        return ptr.ToLongArray();
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

    private static string GetCachePath(string filePath)
    {
        var hash = Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(filePath)));
        return Path.Combine(Directories.CacheHome, $"{hash}.ffindex");
    }
}

/// <summary>
/// External methods
/// </summary>
internal static unsafe partial class External
{
    [LibraryImport("mizuki")]
    internal static partial void Initialize();

    [LibraryImport("mizuki", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int LoadVideo(
        string fileName,
        string cacheFileName,
        string colorMatrix
    );

    [LibraryImport("mizuki", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int SetSubtitles(byte[] data, int dataLen, string? codePage);

    [LibraryImport("mizuki")]
    internal static partial int AllocateBuffers(int numBuffers, int maxCacheSize);

    [LibraryImport("mizuki")]
    internal static partial int AllocateAudioBuffer();

    [LibraryImport("mizuki")]
    internal static partial int FreeBuffers();

    [LibraryImport("mizuki")]
    internal static unsafe partial FrameGroup* GetFrame(int frameNumber, long timestamp, int raw);

    [LibraryImport("mizuki")]
    internal static unsafe partial AudioFrame* GetAudio();

    [LibraryImport("mizuki")]
    internal static unsafe partial int ReleaseFrame(FrameGroup* frame);

    [LibraryImport("mizuki")]
    internal static partial int GetFrameCount();

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetKeyframes();

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetTimecodes();

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetFrameIntervals();

    [LibraryImport("mizuki")]
    internal static partial void SetLoggerCallback(LogCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogCallback(int level, nint message);
}

/// <summary>
/// An unmanaged integer array
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct UnmanagedArray
{
    public nint Pointer;
    public nuint Length;
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
