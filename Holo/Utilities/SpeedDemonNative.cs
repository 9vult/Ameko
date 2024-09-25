using System;
using System.Runtime.InteropServices;

namespace Holo.Utilities
{
    /// <summary>
    /// This class houses the interop functions
    /// for the native SpeedDemon C++ library
    /// </summary>
    /// <remarks>This class is not exposed</remarks>
    internal partial class SpeedDemonNative
    {
        [LibraryImport("SpeedDemon", EntryPoint = "render_subs")]
        public static unsafe partial void RenderSubs(IntPtr frameData, int width, int height, LibassCS.Structures.NativeImage* img);

        [LibraryImport("SpeedDemon", EntryPoint = "copy_frame")]
        public static unsafe partial void CopyFrame(IntPtr source, byte* destination, int size);

        [LibraryImport("SpeedDemon", EntryPoint = "copy_frame")]
        public static unsafe partial void CopyFrame(byte* source, IntPtr destination, int size);
    }
}
