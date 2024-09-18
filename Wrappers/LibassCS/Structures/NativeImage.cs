using LibassCS.Enums;
using System.Runtime.InteropServices;

namespace LibassCS.Structures
{
    /// <summary>
    /// A linked list of images produced by an ass renderer
    /// </summary>
    /// <remarks>
    /// These images have to be rendered in-order for the correct screen
    /// composition. The libass renderer clips these bitmaps to the frame size.
    /// Width and height can be zero; in this case the bitmap should not
    /// be rendered at all. The last bitmap row is not guaranteed to be padded
    /// up to stride size, e.g. in the worst case, a bitmap has the size
    /// stride * (h - 1) + 1.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeImage
    {
        /// <summary>
        /// Width of the bitmap
        /// </summary>
        public int Width;
        /// <summary>
        /// Height of the bitmap
        /// </summary>
        public int Height;
        /// <summary>
        /// Stride of the bitmap
        /// </summary>
        public int Stride;
        /// <summary>
        /// 1bpp stride * height alpha buffer
        /// </summary>
        /// <remarks>The last row may not be padded to bitmap stride!</remarks>
        public byte* Bitmap;
        /// <summary>
        /// Bitmap color and alpha, RGBA
        /// </summary>
        /// <remarks>
        /// For full compatibility with VSFilter, the
        /// value must be transformed according to <see cref="YCbCrMatrix"/>
        /// </remarks>
        public uint Color;
        /// <summary>
        /// Bitmap offset X in the video frame
        /// </summary>
        public int DistX;
        /// <summary>
        /// Bitmap offset Y in the video frame
        /// </summary>
        public int DistY;
        /// <summary>
        /// Next image or null
        /// </summary>
        public NativeImage* Next;
        /// <summary>
        /// Type of image
        /// </summary>
        public ImageType Type;
    }
}
