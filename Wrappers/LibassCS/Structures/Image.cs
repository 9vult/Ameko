using LibassCS.Enums;
using System.Runtime.InteropServices;

namespace LibassCS.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Image
    {
        public int Width;
        public int Height;
        public int Stride;
        public IntPtr Bitmap;
        public uint Color;
        public int DistX;
        public int DistY;
        public IntPtr Next;
        public ImageType Type;
    }
}
