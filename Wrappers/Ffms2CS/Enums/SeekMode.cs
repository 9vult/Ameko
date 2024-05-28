namespace Ffms2CS.Enums
{
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
}
