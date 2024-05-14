using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ErrorInfo
    {
        public Error ErrorType;
        public Error SubType;
        public int BufferSize;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Buffer;
    }

    public enum Error
    {
        Success = 0,
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
