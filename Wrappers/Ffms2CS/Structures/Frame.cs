using System;
using System.Runtime.InteropServices;

namespace Ffms2CS.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Frame
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
        //public byte[] DolbyVisionRPU;
        //public int DolbyVisionRPUSize;
        //public byte[] HDR10Plus;
        //public int HDR10PlusSize;
    }
}
