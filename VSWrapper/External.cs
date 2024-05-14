using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Ffms2CS.Ffms2;

namespace Ffms2CS
{
    public static class External
    {
        [DllImport("ffms2", EntryPoint = "FFMS_Init")]
        public static extern void Init(int zero1, int zero2);

        [DllImport("ffms2", EntryPoint = "FFMS_Deinit")]
        public static extern void Uninit();

        [DllImport("ffms2", EntryPoint = "FFMS_GetVersion")]
        public static extern int GetVersion();

        [DllImport("ffms2", EntryPoint = "FFMS_GetLogLevel")]
        public static extern int GetLogLevel();

        [DllImport("ffms2", EntryPoint = "FFMS_SetLogLevel")]
        public static extern void SetLogLevel(int level);

        [DllImport("ffms2", EntryPoint = "FFMS_CreateVideoSource")]
        public static extern IntPtr CreateVideoSource(byte[] sourceFile, int track, Index index, int threads, int seekMode, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_CreateAudioSource")]
        public static extern IntPtr CreateAudioSource(byte[] sourceFile, int track, Index index, int delayMode, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_CreateAudioSource2")]
        public static extern IntPtr CreateAudioSource2(byte[] sourceFile, int track, Index index, int delayMode, int fillGaps, double drcScale, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_DestroyVideoSource")]
        public static extern void DestroyVideoSource(IntPtr videoSource);

        [DllImport("ffms2", EntryPoint = "FFMS_DestroyAudioSource")]
        public static extern void DestroyAudioSource(IntPtr audioSource);

        [DllImport("ffms2", EntryPoint = "FFMS_GetVideoProperties")]
        public static extern IntPtr GetVideoProperties(IntPtr videoSource);

        [DllImport("ffms2", EntryPoint = "FFMS_GetAudioProperties")]
        public static extern IntPtr GetAudioProperties(IntPtr audioSource);

        [DllImport("ffms2", EntryPoint = "FFMS_GetFrame")]
        public static extern IntPtr GetFrame(IntPtr videoSource, int num, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_GetFrameByTime")]
        public static extern IntPtr GetFrameByTime(IntPtr videoSource, double time, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_GetAudio")]
        public static extern int GetAudio(IntPtr audioSource, byte[] buf, long start, long count, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_SetOutputFormatV2")]
        public static extern int SetOutputFormatV2(IntPtr videoSource, int[] targetFormats, int width, int height, int resizer, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_ResetOutputFormatV")]
        public static extern void ResetOutputFormatV(IntPtr videoSource);

        [DllImport("ffms2", EntryPoint = "FFMS_SetInputFormatV")]
        public static extern int SetInputFormatV(IntPtr videoSource, int colorSpace, int colorRange, int format, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_ResetInputFormatV")]
        public static extern void ResetInputFormatV(IntPtr videoSource);

        [DllImport("ffms2", EntryPoint = "FFMS_CreateResampleOptions")]
        public static extern IntPtr CreateResampleOptions(IntPtr audioSource); // TODO: Check return type

        [DllImport("ffms2", EntryPoint = "FFMS_SetOutputFormatA")]
        public static extern int SetOutputFormatA(IntPtr audioSource, IntPtr options, ref ErrorInfo errorInfo); // TODO: check input

        [DllImport("ffms2", EntryPoint = "FFMS_DestroyResampleOptions")]
        public static extern void DestroyResampleOptions(IntPtr options); // TODO: check input

        [DllImport("ffms2", EntryPoint = "FFMS_DestroyIndex")]
        public static extern void DestroyIndex(IntPtr index);

        [DllImport("ffms2", EntryPoint = "FFMS_GetFirstTrackOfType")]
        public static extern int GetFirstTrackOfType(Index index, int trackType, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_GetFirstIndexedTrackOfType")]
        public static extern int GetFirstIndexedTrackOfType(Index index, int trackType, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_GetNumTracks")]
        public static extern int GetNumTracks(Index index);

        [DllImport("ffms2", EntryPoint = "FFMS_GetNumTracksI")]
        public static extern int GetNumTracksI(Indexer indexer);

        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackType")]
        public static extern int GetTrackType(IntPtr track);

        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackTypeI")]
        public static extern int GetTrackTypeI(Indexer indexer, int track);

        [DllImport("ffms2", EntryPoint = "FFMS_GetErrorHandling")]
        public static extern int GetErrorHandling(Index index);

        [DllImport("ffms2", EntryPoint = "FFMS_GetCodecNameI")]
        public static extern IntPtr GetCodecNameI(Indexer indexer, int track);

        [DllImport("ffms2", EntryPoint = "FFMS_GetFormatNameI")]
        public static extern IntPtr GetFormatNameI(Indexer indexer);

        [DllImport("ffms2", EntryPoint = "FFMS_GetNumFrames")]
        public static extern int GetNumFrames(IntPtr track);

        [DllImport("ffms2", EntryPoint = "FFMS_GetFrameInfo")]
        public static extern IntPtr GetFrameInfo(IntPtr track, int frame);

        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackFromIndex")]
        public static extern IntPtr GetTrackFromIndex(Index index, int track);

        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackFromVideo")]
        public static extern IntPtr GetTrackFromVideo(IntPtr videoSource);

        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackFromAudio")]
        public static extern IntPtr GetTrackFromAudio(IntPtr audioSource);

        [DllImport("ffms2", EntryPoint = "FFMS_GetTimeBase")]
        public static extern IntPtr GetTimeBase(IntPtr track);

        [DllImport("ffms2", EntryPoint = "FFMS_WriteTimecodes")]
        public static extern int WriteTimecodes(IntPtr track, byte[] timecodeFile, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_CreateIndexer")]
        public static extern Indexer CreateIndexer(byte[] sourceFile, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_CreateIndexer2")]
        public static extern Indexer CreateIndexer2(byte[] sourceFile, ref KeyValuePair[] demuxerOptions, int numOptions, ref ErrorInfo errorInfo); // TODO: Check KVP

        [DllImport("ffms2", EntryPoint = "FFMS_TrackIndexSettings")]
        public static extern void TrackIndexSettings(Indexer indexer, int track, int index, int zero);

        [DllImport("ffms2", EntryPoint = "FFMS_TrackTypeIndexSettings")]
        public static extern void TrackTypeIndexSettings(Indexer indexer, int trackKype, int index, int zero);

        [DllImport("ffms2", EntryPoint = "FFMS_SetProgressCallback")]
        public static extern void SetProgressCallback(Indexer indexer, TIndexCallback callback, IntPtr icPrivate);

        [DllImport("ffms2", EntryPoint = "FFMS_DoIndexing2")]
        public static extern Index DoIndexing2(Indexer indexer, int errorHandling, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_CancelIndexing")]
        public static extern void CancelIndexing(IntPtr indexer);

        [DllImport("ffms2", EntryPoint = "FFMS_ReadIndex")]
        public static extern Index ReadIndex(byte[] indexFile, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_ReadIndexFromBuffer")]
        public static extern Index ReadIndexFromBuffer(IntPtr bufferPointer, int size, Index index, ref ErrorInfo info); // TODO: check

        [DllImport("ffms2", EntryPoint = "FFMS_IndexBelongsToFile")]
        public static extern int IndexBelongsToFile(Index index, byte[] sourceFile, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_WriteIndex")]
        public static extern int WriteIndex(byte[] indexFile, Index index, ref ErrorInfo errorInfo);

        [DllImport("ffms2", EntryPoint = "FFMS_WriteIndexToBuffer")]
        public static extern int WriteIndexToBuffer(IntPtr bufferPointer, int size, Index index, ref ErrorInfo errorInfo); // TODO: check

        [DllImport("ffms2", EntryPoint = "FFMS_FreeIndexBuffer")]
        public static extern void FreeIndexBuffer(IntPtr bufferPointer);

        [DllImport("ffms2", EntryPoint = "FFMS_GetPixFmt")]
        public static extern int GetPixFmt([MarshalAs(UnmanagedType.LPStr)] string name);
    }
}
