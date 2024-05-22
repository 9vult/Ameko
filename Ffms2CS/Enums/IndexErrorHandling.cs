namespace Ffms2CS.Enums
{
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
}
