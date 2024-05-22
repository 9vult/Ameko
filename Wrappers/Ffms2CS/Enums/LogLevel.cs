namespace Ffms2CS.Enums
{
    /// <summary>
    /// FFmpeg log level
    /// </summary>
    public enum LogLevel
    {
        Quiet = -8,
        Panic = 0,
        Fatal = 8,
        Error = 16,
        Warning = 24,
        Info = 32,
        Verbose = 40,
        Debug = 48,
        Trace = 56
    }
}
