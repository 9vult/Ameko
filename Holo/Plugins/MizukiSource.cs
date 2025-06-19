// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;

namespace Holo.Plugins;

/// <summary>
/// Interop with the Mizuki source provider
/// </summary>
public class MizukiSource : ISourcePlugin
{
    public static int GetFfmsVersion()
    {
        return External.TestGetFfmsVersion();
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
    internal static partial void LoadVideo(
        [MarshalAs(UnmanagedType.LPStr)] string fileName,
        [MarshalAs(UnmanagedType.LPStr)] string colorMatrix
    );

    [LibraryImport("mizuki")]
    internal static partial IntArray GetKeyframes();

    [LibraryImport("mizuki")]
    internal static partial IntArray GetTimecodes();

    [LibraryImport("mizuki")]
    internal static partial void FreeIntArray(IntArray array);
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
