using LibassCS.Enums;
using System.Runtime.InteropServices;

namespace LibassCS
{
    internal static partial class External
    {
        [LibraryImport("libass", EntryPoint = "ass_library_init")]
        public static partial IntPtr InitLibrary();

        [LibraryImport("libass", EntryPoint = "ass_library_done")]
        public static partial void UninitLibrary(IntPtr library);

        [LibraryImport("libass", EntryPoint = "ass_library_version")]
        public static partial int GetVersion();

        [LibraryImport("libass", EntryPoint = "ass_set_fonts_dir")]
        public static partial void SetFontsDir(IntPtr library, [MarshalAs(UnmanagedType.LPStr)] string path);

        [LibraryImport("libass", EntryPoint = "ass_set_extract_fonts")]
        public static partial void SetExtractFonts(IntPtr library, [MarshalAs(UnmanagedType.Bool)] bool extract);

        [LibraryImport("libass", EntryPoint = "ass_set_style_overrides")]
        public static partial void SetStyleOverrides(IntPtr library, IntPtr list);

        [LibraryImport("libass", EntryPoint = "ass_renderer_init")]
        public static partial IntPtr InitRenderer(IntPtr library);

        [LibraryImport("libass", EntryPoint = "ass_renderer_done")]
        public static partial IntPtr UninitRenderer(IntPtr renderer);

        [LibraryImport("libass", EntryPoint = "ass_set_frame_size")]
        public static partial void SetFrameSize(IntPtr renderer, int width, int height);

        [LibraryImport("libass", EntryPoint = "ass_set_storage_size")]
        public static partial void SetStorageSize(IntPtr renderer, int width, int height);

        [LibraryImport("libass", EntryPoint = "ass_set_margins")]
        public static partial void SetMargins(IntPtr renderer, int top, int botton, int left, int right);

        [LibraryImport("libass", EntryPoint = "ass_set_use_margins")]
        public static partial void SetUseMargins(IntPtr renderer, [MarshalAs(UnmanagedType.Bool)] bool useMargins);

        [Obsolete("Use SetPixelAspect instead")]
        [LibraryImport("libass", EntryPoint = "ass_set_aspect_ratio")]
        public static partial void SetAspectRatio(IntPtr renderer, double dar, double sar);

        [LibraryImport("libass", EntryPoint = "ass_set_font_scale")]
        public static partial void SetFontScale(IntPtr renderer, double scale);

        [LibraryImport("libass", EntryPoint = "ass_set_hinting")]
        public static partial void SetHinting(IntPtr renderer, Hinting hinting);

        [LibraryImport("libass", EntryPoint = "ass_set_line_spacing")]
        public static partial void SetLineSpacing(IntPtr renderer, double spacing);

        [LibraryImport("libass", EntryPoint = "ass_get_available_font_providers")]
        public static partial void GetAvailableFontProviders(IntPtr library, IntPtr providers, ref UIntPtr size);

        [LibraryImport("libass", EntryPoint = "ass_set_fonts")]
        public static partial void SetFonts(IntPtr renderer, [MarshalAs(UnmanagedType.LPStr)] string defaultFont, [MarshalAs(UnmanagedType.LPStr)] string defaultFamily, int dfp, [MarshalAs(UnmanagedType.LPStr)] string config, int update);

        [LibraryImport("libass", EntryPoint = "ass_render_frame")]
        public static partial IntPtr RenderFrame(IntPtr renderer, IntPtr track, long now, int detectChange);

        [LibraryImport("libass", EntryPoint = "ass_new_track")]
        public static partial IntPtr NewTrack(IntPtr library);

        [LibraryImport("libass", EntryPoint = "ass_free_track")]
        public static partial void FreeTrack(IntPtr track);

        [LibraryImport("libass", EntryPoint = "ass_alloc_style")]
        public static partial int AllocStyle(IntPtr track);

        [LibraryImport("libass", EntryPoint = "ass_alloc_event")]
        public static partial int AllocEvent(IntPtr track);

        [LibraryImport("libass", EntryPoint = "ass_free_style")]
        public static partial void FreeStyle(IntPtr track, int styleId);

        [LibraryImport("libass", EntryPoint = "ass_free_event")]
        public static partial void FreeEvent(IntPtr track, int eventId);

        [LibraryImport("libass", EntryPoint = "ass_process_data")]
        public static partial void ProcessData(IntPtr track, [MarshalAs(UnmanagedType.LPStr)] string data, int size);

        [LibraryImport("libass", EntryPoint = "ass_process_codec_private")]
        public static partial void ProcessCodecPrivate(IntPtr track, [MarshalAs(UnmanagedType.LPStr)] string data, int size);

        [LibraryImport("libass", EntryPoint = "ass_process_chunk")]
        public static partial void ProcessChunk(IntPtr track, [MarshalAs(UnmanagedType.LPStr)] string data, int size, long timecode, long duration);

        [LibraryImport("libass", EntryPoint = "ass_read_file")]
        public static partial IntPtr ReadFile(IntPtr library, [MarshalAs(UnmanagedType.LPStr)] string fileName, [MarshalAs(UnmanagedType.LPStr)] string codePage);

        [LibraryImport("libass", EntryPoint = "ass_read_memory")]
        public static partial IntPtr ReadMemory(IntPtr library, in byte[] buffer, UIntPtr bufferSize, [MarshalAs(UnmanagedType.LPStr)] string codePage);

        [LibraryImport("libass", EntryPoint = "ass_read_styles")]
        public static partial int ReadStyles(IntPtr track, [MarshalAs (UnmanagedType.LPStr)] string fileName, [MarshalAs(UnmanagedType.LPStr)] string codePage);

        [LibraryImport("libass", EntryPoint = "ass_add_font")]
        public static partial void AddFont(IntPtr library, [MarshalAs(UnmanagedType.LPStr)] string name, in byte[] data, int dataSize);

        [LibraryImport("libass", EntryPoint = "ass_clear_fonts")]
        public static partial void ClearFonts(IntPtr library);

        [LibraryImport("libass", EntryPoint = "ass_step_sub")]
        public static partial long StepSub(IntPtr track, long now, int movement);

        [LibraryImport("libass", EntryPoint = "ass_process_force_style")]
        public static partial void ProcessForceStyle(IntPtr track);

        [LibraryImport("libass", EntryPoint = "ass_set_message_cb")]
        public static partial void SetMessageCB(IntPtr library, MessageCBCallback callback);

        [Obsolete("Does nothing")]
        [LibraryImport("libass", EntryPoint = "ass_fonts_update")]
        public static partial int FontsUpdate(IntPtr renderer);

        [LibraryImport("libass", EntryPoint = "ass_set_cache_limits")]
        public static partial void SetCacheLimits(IntPtr renderer, int glyphMax, int bitmapMaxSize);

        [LibraryImport("libass", EntryPoint = "ass_flush_events")]
        public static partial void FlushEvents(IntPtr track);

        [LibraryImport("libass", EntryPoint = "ass_set_shaper")]
        public static partial void SetShaper(IntPtr renderer, ShapingLevel level);

        [LibraryImport("libass", EntryPoint = "ass_set_line_position")]
        public static partial void SetLinePosition(IntPtr renderer, double position);

        [LibraryImport("libass", EntryPoint = "ass_set_pixel_aspect")]
        public static partial void SetPixelAspect(IntPtr renderer, double par);

        [LibraryImport("libass", EntryPoint = "ass_set_selective_style_override_enabled")]
        public static partial void SetSelectiveStyleOverrideEnabled(IntPtr renderer, OverrideBits bits);

        [LibraryImport("libass", EntryPoint = "ass_set_selective_style_override")]
        public static partial void SetSelectiveStyleOverride(IntPtr renderer, IntPtr style);

        [LibraryImport("libass", EntryPoint = "ass_set_check_readorder")]
        public static partial void SetCheckReadOrder(IntPtr track, int checkReadOrder);

        [LibraryImport("libass", EntryPoint = "ass_malloc")]
        public static partial IntPtr Malloc(UIntPtr size);

        [LibraryImport("libass", EntryPoint = "ass_free")]
        public static partial void Free(IntPtr pointer);


        public delegate void MessageCBCallback(int level, [MarshalAs(UnmanagedType.LPStr)] string format, params object[] args);
    }
}
