using System.Runtime.InteropServices;

namespace LibassCS.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeStyle
    {
        public char* Name;
        public char* FontName;
        public double FontSize;
        public int PrimaryColor;
        public int SecondaryColor;
        public int OutlineColor;
        public int BackColor;
        public int Bold;
        public int Italic;
        public int Underline;
        public int StrikeOut;
        public double ScaleX;
        public double ScaleY;
        public double Spacing;
        public double Andle;
        public int BorderStyle;
        public double Outline;
        public double Shadow;
        public int Alignment;
        public int MarginL;
        public int MarginR;
        public int MarginV;
        public int Encoding;
        [Obsolete("Does nothing")]
        public int TreatFontNameAsPattern;
        public double Blur;
        public int Justify;
    }
}
