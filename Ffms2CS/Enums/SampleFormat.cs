namespace Ffms2CS.Enums
{
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
}
