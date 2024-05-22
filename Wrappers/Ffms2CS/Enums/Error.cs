namespace Ffms2CS.Enums
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
}
