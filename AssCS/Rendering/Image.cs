using System.Runtime.InteropServices;
using System;

namespace AssCS.Rendering
{
    /// <summary>
    /// A linked list of images produced by an ass renderer
    /// </summary>
    /// <remarks>
    /// These images have to be rendered in-order for
    /// the correct screen composition.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Image
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
        public IntPtr Bitmap;
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
        public Image* Next;
        /// <summary>
        /// Type of image
        /// </summary>
        public ImageType Type;
    }
}
