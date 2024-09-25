using System;
using System.Runtime.InteropServices;

namespace Ffms2CS.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeFrame
    {
        public IntPtr Data0;
        public IntPtr Data1;
        public IntPtr Data2;
        public IntPtr Data3;

        public int LineSize0;
        public int LineSize1;
        public int LineSize2;
        public int LineSize3;

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

        public double MasteringDisplayPrimariesX0;
        public double MasteringDisplayPrimariesX1;
        public double MasteringDisplayPrimariesX2;
        public double MasteringDisplayPrimariesY0;
        public double MasteringDisplayPrimariesY1;
        public double MasteringDisplayPrimariesY2;

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
