using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// Types of errors
    /// </summary>
    public enum Error
    {
        // Success
        Success = 0,
        // Where the error occured
        Index = 1,
        Indexing,
        Postprocessing,
        Scaling,
        Decoding,
        Seeking,
        Parser,
        WaveWriter,
        Cancelled,
        Resampling,

        // What caused the error
        Unknown = 20,
        Unsupported,
        FileRead,
        FileWrite,
        NoFile,
        Version,
        AllocationFailed,
        InvalidArgument,
        Codec,
        NotAvailable,
        FileMismatch,
        User
    }

    /// <summary>
    /// Control the way seeking is handled
    /// </summary>
    public enum SeekMode
    {
        /// <summary>
        /// Linear access without rewind
        /// </summary>
        LinearNoRW = -1,
        /// <summary>
        /// Linear access
        /// </summary>
        Linear = 0,
        /// <summary>
        /// Safe normal. Bases seeking decisions on the keyframe positions reported by libavformat.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Unsafe normal. Same as FFMS_SEEK_NORMAL but no error will be thrown if the exact destination has to be guessed.
        /// </summary>
        Unsafe = 2,
        /// <summary>
        /// Aggressive. Seeks in the forward direction even if no closer keyframe is known to exist.
        /// </summary>
        Aggressive = 3
    }

    /// <summary>
    /// Used by the indexing functions to control behavior when a decoding error is encountered.
    /// </summary>
    public enum IndexErrorHandling
    {
        /// <summary>
        /// Abort indexing and raise an error
        /// </summary>
        Abort = 0,
        /// <summary>
        /// Clear all indexing entries for the track (i.e. return a blank track)
        /// </summary>
        ClearTrack = 1,
        /// <summary>
        /// Stop indexing but keep previous indexing entries (i.e. return a track that stops where the error occurred)
        /// </summary>
        StopTrack = 2,
        /// <summary>
        /// Ignore the error and pretend it's raining
        /// </summary>
        Ignore = 3
    }

    /// <summary>
    /// Used for determining the type of a given track.
    /// </summary>
    public enum TrackType
    {
        Unknown = -1,
        Video,
        Audio,
        Data,
        Subtitle,
        Attachment
    }

    /// <summary>
    /// Audio sample formats
    /// </summary>
    public enum SampleFormat
    {
        /// <summary>
        /// One 8-bit unsigned integer (uint8_t) per sample.
        /// </summary>
        U8 = 0,
        /// <summary>
        /// One 16-bit signed integer (int16_t) per sample.
        /// </summary>
        S16,
        /// <summary>
        /// One 32-bit signed integer (int32_t) per sample.
        /// </summary>
        S32,
        /// <summary>
        /// One 32-bit (single precision) floating point value (float_t) per sample.
        /// </summary>
        FLT,
        /// <summary>
        /// One 64-bit (double precision) floating point value (double_t) per sample.
        /// </summary>
        DBL
    }

    /// <summary>
    /// Describes the audio channel layout of an audio stream.
    /// </summary>
    public enum AudioChannel
    {
        FrontLeft = 0x00000001,
        FrontRight = 0x00000002,
        FrontCenter = 0x00000004,
        LowFrequency = 0x00000008,
        BackLeft = 0x00000010,
        BackRight = 0x00000020,
        FrontLeftOfCenter = 0x00000040,
        FrontRightOfCenter = 0x00000080,
        BackCenter = 0x00000100,
        SideLeft = 0x00000200,
        SideRight = 0x00000400,
        TopCenter = 0x00000800,
        TopFrontLeft = 0x00001000,
        TopFrontCenter = 0x00002000,
        TopFrontRight = 0x00004000,
        TopBackLeft = 0x00008000,
        TopBackCenter = 0x00010000,
        TopBackRight = 0x00020000,
        StereoLeft = 0x20000000,
        StereoRight = 0x40000000
    }

    /// <summary>
    /// Resizing algorithms
    /// </summary>
    public enum Resizer
    {
        FastBilinear = 0x01,
        Bilinear = 0x02,
        Bicubic = 0x04,
        X = 0x08,
        Point = 0x10,
        Area = 0x20,
        Bicublin = 0x40,
        Gauss = 0x80,
        Sinc = 0x100,
        Lanczos = 0x200,
        Spline = 0x400
    }

    /// <summary>
    /// Controls how audio with a non-zero first PTS is handled;
    /// in other words what FFMS does about audio delay.
    /// </summary>
    public enum AudioDelayMode
    {
        /// <summary>
        /// No adjustment is made; the first decodable audio sample
        /// becomes the first sample in the output. May lead to audio/video desync.
        /// </summary>
        DelayNoShift = -3,
        /// <summary>
        /// Samples are created (with silence) or discarded
        /// so that sample 0 in the decoded audio starts at time zero.
        /// </summary>
        DelayTimeZero = -2,
        /// <summary>
        /// Samples are created (with silence) or discarded so that sample 0
        /// in the decoded audio starts at the same time as frame 0 of the first video track.
        /// This is what most users want and is a sane default.
        /// </summary>
        DelayFirstVideoTrack = -1
    }

    /// <summary>
    /// Identifies the location of chroma samples in a frame.
    /// </summary>
    public enum ChromaLocation
    {
        Unspecified = 0,
        Left = 1,
        Center = 2,
        TopLeft = 3,
        Top = 4,
        BottomLeft = 5,
        Bottom = 6
    }

    /// <summary>
    /// Identifies the valid range of luma values in a YUV stream.
    /// </summary>
    public enum ColorRange
    {
        Unspecified = 0,
        /// <summary>
        /// "TV Range"
        /// </summary>
        MPEG = 1,
        /// <summary>
        /// Full range
        /// </summary>
        JPEG = 2
    }

    /// <summary>
    /// Identifies the type of stereo 3D the video is.
    /// </summary>
    public enum Stereo3DType
    {
        TwoD = 0,
        SideBySide,
        TopBottom,
        FrameSequence,
        Checkerboard,
        SideBySide_QUINCUNX,
        Lines,
        Columns
    }

    /// <summary>
    /// Flags for Stereo 3D videos
    /// </summary>
    public enum Stereo3DFlags
    {
        Invert = 1
    }

    /// <summary>
    /// FFmpeg log level
    /// </summary>
    public enum LogLevel
    {
        Quiet = -8,
        Panic = 0,
        Fatal = 8,
        Error = 16,
        Warning = 24,
        Info = 32,
        Verbose = 40,
        Debug = 48,
        Trace = 56
    }

    /// <summary>
    /// Information about a video
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VideoProperties
    {
        public int FPSDenominator;
        public int FPSNumerator;
        public int RFFDenominator;
        public int RFFNumerator;
        public int NumFrames;
        public int SARNum;
        public int SARDen;
        public int CropTop;
        public int CropBottom;
        public int CropLeft;
        public int CropRight;
        public int TopFieldFirst;
        public int ColorSpace;
        public int ColorRange;
        public double FirstTime;
        public double LastTime;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct FrameStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public IntPtr[] Data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] LineSize;
        public int EncodedWidth;
        public int EncodedHeight;
        public int EncodedPixelFormat;
        public int ScaledWidth;
        public int ScaledHeight;
        public int ConvertedPixelFormat;
        public int Keyframe;
        public int RepeatPict;
        public int InterlacedFrame;
        public int TopFieldFirst;
        public char PictType;
        public int ColorSpace;
        public int ColorRange;
        public int ColorPrimaries;
        public int TransferCharacteristics;
        public int ChromaLocation;
        public int HasMasteringDisplayPrimaries;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] MasteringDisplayPrimariesX;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] MasteringDisplayPrimariesY;
        public double MasteringDisplayWhitePointX;
        public double MasteringDisplayWhitePointY;
        public int HasMasteringDisplayLuminance;
        public double MasteringDisplayMinLuminance;
        public double MasteringDisplayMaxLuminance;
        public int HasContentLightLevel;
        public uint ContentLightLevelMax;
        public uint ContentLightLevelAverage;
        public byte[] DolbyVisionRPU;
        public int DolbyVisionRPUSize;
        public byte[] HDR10Plus;
        public int HDR10PlusSize;
    }

    /// <summary>
    /// Track time base
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TrackTimeBase
    {
        public long Num;
        public long Den;
    }

    /// <summary>
    /// Information about a frame
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FrameInfoStruct
    {
        public long PTS;
        public int RepeatPict;
        public int KeyFrame;
        public long OriginalPTS;
    }

    /// <summary>
    /// A rectangle representing how much to crop in on a frame
    /// </summary>
    public class Crop
    {
        /// <summary>
        /// How much to crop in from the top
        /// </summary>
        public int Top { get; private set; }
        /// <summary>
        /// How much to crop in from the left
        /// </summary>
        public int Left { get; private set; }
        /// <summary>
        /// How much to crop in from the right
        /// </summary>
        public int Right { get; private set; }
        /// <summary>
        /// How much to crop in from the bottom
        /// </summary>
        public int Bottom { get; private set;}

        /// <summary>
        /// Create a crop
        /// </summary>
        /// <param name="top">How much to crop in from the top</param>
        /// <param name="left">How much to crop in from the left</param>
        /// <param name="right">How much to crop in from the right</param>
        /// <param name="bottom">How much to crop in from the bottom</param>
        internal Crop(int top, int left, int right, int bottom)
        {
            Top = top;
            Left = left;
            Right = right;
            Bottom = bottom;
        }
    }
}
