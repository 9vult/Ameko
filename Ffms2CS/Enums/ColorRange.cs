namespace Ffms2CS.Enums
{
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
}
