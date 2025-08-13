// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Holo.IO;

namespace Holo.Media.Providers;

/// <summary>
/// Interop with the Mizuki source provider
/// </summary>
public class MizukiSourceProvider : ISourceProvider
{
    /// <summary>
    /// If this provider is initialized
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public int FrameCount { get; }

    /// <inheritdoc />
    public Rational Sar { get; }

    public void Initialize()
    {
        External.Initialize();
        IsInitialized = true;
    }

    public int LoadVideo(string filename)
    {
        return External.LoadVideo(filename, GetCachePath(filename), "bgra");
    }

    public int CloseVideo()
    {
        return 0;
    }

    public int AllocateBuffers(int numBuffers)
    {
        return External.AllocateBuffers(numBuffers);
    }

    /// <inheritdoc />
    public int FreeBuffers()
    {
        return External.FreeBuffers();
    }

    /// <inheritdoc />
    public int GetFrame(int frameNumber, out VideoFrame frame)
    {
        return External.GetFrame(frameNumber, out frame);
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
        [MarshalAs(UnmanagedType.LPStr)] string fileName,
        [MarshalAs(UnmanagedType.LPStr)] string cacheFileName,
        [MarshalAs(UnmanagedType.LPStr)] string colorMatrix
    );

    [LibraryImport("mizuki")]
    internal static partial int AllocateBuffers(int numBuffers);

    [LibraryImport("mizuki")]
    internal static partial int FreeBuffers();

    [LibraryImport("mizuki")]
    internal static partial int GetFrame(int frameNumber, out VideoFrame frame);

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetKeyframes();

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetTimecodes();

    [LibraryImport("mizuki")]
    internal static partial UnmanagedArray GetFrameIntervals();
}

[StructLayout(LayoutKind.Sequential)]
public struct VideoFrame
{
    public int FrameNumber;
    public long Timestamp;
    public int Width;
    public int Height;
    public int Pitch;
    public int Flipped;
    public nint Data;
    public int Valid;
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
