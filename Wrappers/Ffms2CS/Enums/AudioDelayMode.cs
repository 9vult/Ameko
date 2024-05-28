namespace Ffms2CS.Enums
{
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
}
