using Ffms2CS.Enums;
using Ffms2CS.Structures;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Ffms2CS
{
    public unsafe class Frame
    {
        private readonly NativeFrame* _struct;
        private readonly IntPtr _data;
        internal bool Invalid = false;

        /// <summary>
        /// Pointer to pixel data
        /// </summary>
        public IntPtr Data => (!Invalid) ? _data : throw new ObjectDisposedException(nameof(Frame));

        public byte[] Bytes
        {
            get
            {
                ObjectDisposedException.ThrowIf(Invalid, nameof(Frame));
                byte[] _bytes;
                IntPtr plane0 = _struct->Data0;
                int plane0size = _struct->LineSize0;
                if (plane0 != IntPtr.Zero && plane0size > 0 && _struct->ScaledHeight > 0)
                {
                    int bgraBitmapSize = plane0size * _struct->ScaledHeight;
                    _bytes = new byte[bgraBitmapSize];
                    Marshal.Copy(plane0, _bytes, 0, bgraBitmapSize);
                    return _bytes;
                }
                return [];
            }
        }

        /// <summary>
        /// List of integers for the length of each scan line
        /// </summary>
        public ReadOnlyCollection<int> LineSize => (!Invalid) ? new ReadOnlyCollection<int>(new List<int>
        {
            _struct->LineSize0,
            _struct->LineSize1,
            _struct->LineSize2,
            _struct->LineSize3
        }) : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Frame size in pixels
        /// </summary>
        public Size EncodedResolution => (!Invalid) ? new Size(_struct->EncodedWidth, _struct->EncodedHeight) : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Frame pixel format
        /// </summary>
        public int EncodedPixelFormat => (!Invalid) ? _struct->EncodedPixelFormat : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// The scaled output resolution of the frame
        /// </summary>
        public Size OutputResolution => (!Invalid) ? new Size(_struct->ScaledWidth, _struct->ScaledHeight) : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Output pixel format
        /// </summary>
        public int OutputPixelFormat => (!Invalid) ? _struct->ConvertedPixelFormat : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// If this frame is a keyframe
        /// </summary>
        public bool IsKeyframe => (!Invalid) ? _struct->Keyframe != 0 : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// RFF flag for this frame
        /// </summary>
        public int RepeatPicture => (!Invalid) ? _struct->RepeatPict : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// If this frame is interlaced
        /// </summary>
        public bool IsInterlaced => (!Invalid) ? _struct->InterlacedFrame != 0 : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// For interlaced frames, if the top field is first
        /// </summary>
        public bool IsTopFieldFirst => (!Invalid) ? _struct->TopFieldFirst != 0 : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// The coding type of the frame
        /// </summary>
        public char FrameType => (!Invalid) ? _struct->PictType : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Colorspace used in this frame
        /// </summary>
        public int ColorSpace => (!Invalid) ? _struct->ColorSpace : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Luma range of the frame
        /// </summary>
        public ColorRange ColorRange => (!Invalid) ? (ColorRange)_struct->ColorRange : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Color primaries of the frame
        /// </summary>
        public int ColorPrimaries => (!Invalid) ? _struct->ColorPrimaries : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Transfer characteristics of the frame
        /// </summary>
        public int TransferCharacteristics => (!Invalid) ? _struct->TransferCharacteristics : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Chroma location in the frame
        /// </summary>
        public ChromaLocation ChromaLocation => (!Invalid) ? (ChromaLocation)_struct->ChromaLocation : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// If the frame has mastering display primaries
        /// </summary>
        public bool HasMasteringDisplayPrimaries => (!Invalid) ? _struct->HasMasteringDisplayPrimaries != 0 : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// X-axis mastering display primaries
        /// </summary>
        public ReadOnlyCollection<double> MasteringDisplayPrimariesX => (!Invalid) ? new ReadOnlyCollection<double>(new List<double>
        {
            _struct->MasteringDisplayPrimariesX0,
            _struct->MasteringDisplayPrimariesX1,
            _struct->MasteringDisplayPrimariesX2
        }) : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Y-axis mastering display primaries
        /// </summary>
        public ReadOnlyCollection<double> MasteringDisplayPrimariesY => (!Invalid) ? new ReadOnlyCollection<double>(new List<double>
        {
            _struct->MasteringDisplayPrimariesY0,
            _struct->MasteringDisplayPrimariesY1,
            _struct->MasteringDisplayPrimariesY2
        }) : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// X-axis white point for mastering displays
        /// </summary>
        public double MasteringDisplayWhitePointX => (!Invalid) ? _struct->MasteringDisplayWhitePointX : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Y-axis white point for mastering displays
        /// </summary>
        public double MasteringDisplayWhitePointY => (!Invalid) ? _struct->MasteringDisplayWhitePointY : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// If the frame has content light levels
        /// </summary>
        public bool HasContentLightLevel => (!Invalid) ? _struct->HasContentLightLevel != 0 : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Maximum content light level
        /// </summary>
        public uint ContentLightLevelMax => (!Invalid) ? _struct->ContentLightLevelMax : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Average content light level
        /// </summary>
        public uint ContentLightLevelAverage => (!Invalid) ? _struct->ContentLightLevelAverage : throw new ObjectDisposedException(nameof(Frame));

        /*

        /// <summary>
        /// Dolby Vision RPU
        /// </summary>
        public ReadOnlyCollection<byte> DolbyVisionRPU => (!Invalid) ? new ReadOnlyCollection<byte>(_struct.DolbyVisionRPU) : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Size of the Dolby Vision RPU
        /// </summary>
        public int DolbyVisionRPUSize => (!Invalid) ? _struct.DolbyVisionRPUSize : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// HDR10+
        /// </summary>
        public ReadOnlyCollection<byte> HDR10Plus => (!Invalid) ? new ReadOnlyCollection<byte>(_struct.HDR10Plus) : throw new ObjectDisposedException(nameof(Frame));

        /// <summary>
        /// Size of the HDR10+
        /// </summary>
        public int HDR10PlusSize => (!Invalid) ? _struct.HDR10PlusSize : throw new ObjectDisposedException(nameof(Frame));

        */

        [Obsolete("Don't use this")]
        public SKBitmap SKBitmap
        {
            get
            {
                ObjectDisposedException.ThrowIf(Invalid, nameof(Frame));

                if (_struct->ConvertedPixelFormat != Ffms2.GetPixelFormat("bgra")) throw new InvalidOperationException("Invalid pixel format");

                var bitmap = new SKBitmap(
                    _struct->ScaledWidth,
                    _struct->ScaledHeight,
                    false
                );
                bitmap.SetPixels(_struct->Data0);
                return bitmap;
            }
        }

        /// <summary>
        /// Construct the frame
        /// </summary>
        /// <param name="frame"></param>
        internal Frame(NativeFrame* frame)
        {
            _struct = frame;
            _data = _struct->Data0;
        }
    }

    /// <summary>
    /// Information about a frame
    /// </summary>
    public class FrameInfo
    {
        private readonly Structures.FrameInfo _struct;

        /// <summary>
        /// Frame timestamp
        /// </summary>
        public long PTS => _struct.PTS;

        /// <summary>
        /// RFF flag for the frame
        /// </summary>
        public int RepeatPicture => _struct.RepeatPict;

        /// <summary>
        /// If the frame is a keyframe
        /// </summary>
        public bool IsKeyframe => _struct.KeyFrame != 0;

        /// <summary>
        /// Create a frameinfo
        /// </summary>
        /// <param name="struct">FrameInfoStruct</param>
        internal FrameInfo(Structures.FrameInfo @struct)
        {
            _struct = @struct;
        }
    }
}
