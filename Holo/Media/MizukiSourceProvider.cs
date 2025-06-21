// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Holo.IO;

namespace Holo.Media;

/// <summary>
/// Interop with the Mizuki source provider
/// </summary>
public class MizukiSourceProvider : ISourceProvider
{
    public int GetFfmsVersion()
    {
        return External.TestGetFfmsVersion();
    }

    public void Initialize()
    {
        External.Initialize();
    }

    public int LoadVideo(string filename)
    {
        return External.LoadVideo(filename, GetCachePath(filename), string.Empty);
    }

    public int AllocateFrame()
    {
        return External.AllocateFrame();
    }

    public int GetFrame(int frameNumber, out VideoFrame frame)
    {
        return External.GetFrame(frameNumber, out frame);
    }

    public int[] GetTimecodes()
    {
        var ptr = External.GetTimecodes();
        var timecodes = ptr.ToArray(); // Frees
        return timecodes;
    }

    public int[] GetKeyframes()
    {
        var ptr = External.GetKeyframes();
        var keyframes = ptr.ToArray(); // Frees
        return keyframes;
    }

    private static string GetCachePath(string filePath)
    {
        var hash = Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(filePath)));
        return Path.Combine(DirectoryService.CacheHome, $"{hash}.ffindex");
    }
}

/// <summary>
/// External methods
/// </summary>
internal static unsafe partial class External
{
    [LibraryImport("mizuki")]
    internal static partial void Initialize();

    [LibraryImport("Mizuki")]
    internal static partial int TestGetFfmsVersion();

    [LibraryImport("mizuki", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int LoadVideo(
        [MarshalAs(UnmanagedType.LPStr)] string fileName,
        [MarshalAs(UnmanagedType.LPStr)] string cacheFileName,
        [MarshalAs(UnmanagedType.LPStr)] string colorMatrix
    );

    [LibraryImport("mizuki")]
    internal static partial int GetFrame(int frameNumber, out VideoFrame frame);

    [LibraryImport("mizuki")]
    internal static partial int AllocateFrame();

    [LibraryImport("mizuki")]
    internal static partial IntArray GetKeyframes();

    [LibraryImport("mizuki")]
    internal static partial IntArray GetTimecodes();

    [LibraryImport("mizuki")]
    internal static partial void FreeIntArray(IntArray array);
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
    public nint FrameData;
    public int Valid;
}

/// <summary>
/// An unmanaged integer array
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct IntArray
{
    public nint Pointer;
    public nuint Length;
}

internal static class IntArrayExtensions
{
    /// <summary>
    /// Copy an unmanaged <see cref="IntArray"/> to a managed <c>int[]</c>
    /// </summary>
    /// <param name="array">Unmanaged array</param>
    /// <returns>Managed array</returns>
    public static int[] ToArray(this IntArray array)
    {
        var managed = new int[array.Length];
        Marshal.Copy(array.Pointer, managed, 0, managed.Length);
        External.FreeIntArray(array);
        return managed;
    }
}
