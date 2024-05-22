namespace Ffms2CS.Enums
{
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
}
