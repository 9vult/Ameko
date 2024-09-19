using LibassCS.Enums;
using LibassCS.Structures;
using System.Runtime.InteropServices;

namespace LibassCS
{
    internal static unsafe partial class External
    {
        /// <summary>
        /// Initialize the ass Library
        /// </summary>
        /// <returns>Library handle structure</returns>
        [LibraryImport("libass", EntryPoint = "ass_library_init")]
        public static partial NativeLibrary* InitLibrary();

        /// <summary>
        /// Uninitialize the ass library
        /// </summary>
        /// <param name="library">Library handle</param>
        [LibraryImport("libass", EntryPoint = "ass_library_done")]
        public static partial void UninitLibrary(NativeLibrary* library);

        /// <summary>
        /// Get the version of Libass in use
        /// </summary>
        /// <returns>Libass version (hexadecimal)</returns>
        [LibraryImport("libass", EntryPoint = "ass_library_version")]
        public static partial int GetVersion();

        /// <summary>
        /// Set the directory to load fonts from
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <param name="path">Path to the directory</param>
        [LibraryImport("libass", EntryPoint = "ass_set_fonts_dir")]
        public static partial void SetFontsDir(NativeLibrary* library, [MarshalAs(UnmanagedType.LPStr)] string path);

        /// <summary>
        /// Should fonts be extracted?
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <param name="extract">If fonts should be extracted</param>
        [LibraryImport("libass", EntryPoint = "ass_set_extract_fonts")]
        public static partial void SetExtractFonts(NativeLibrary* library, [MarshalAs(UnmanagedType.Bool)] bool extract);

        /// <summary>
        /// Set global style overrides
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <param name="list">List of style overrides</param>
        [LibraryImport("libass", EntryPoint = "ass_set_style_overrides")]
        public static partial void SetStyleOverrides(NativeLibrary* library, IntPtr list);

        /// <summary>
        /// Initializes the renderer
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <returns>Renderer handle structure</returns>
        /// <remarks>
        /// Before rendering starts, the renderer should be configured with at least
        /// <see cref="SetStorageSize(NativeRenderer*, int, int)"/>,
        /// <see cref="SetFrameSize(NativeRenderer*, int, int)"/>, and
        /// <see cref="SetFonts(NativeRenderer*, string, string, DefaultFontProvider, string, int)"/>.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_renderer_init")]
        public static partial NativeRenderer* InitRenderer(NativeLibrary* library);

        /// <summary>
        /// Unitialize the renderer
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        [LibraryImport("libass", EntryPoint = "ass_renderer_done")]
        public static partial void UninitRenderer(NativeRenderer* renderer);

        /// <summary>
        /// Set the frame size, in pixels, including margins
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="width">Width of the frame in pixels</param>
        /// <param name="height">Height of the frame in pixels</param>
        /// <remarks>
        /// The renderer will never return images that are outside of
        /// the frame area. The value set with this function can influence
        /// the pixel aspect ratio used for rendering.<br/>
        /// If after compensating for configured margins the frame size is
        /// not an isotropically scaled version of the video display size,
        /// you may have to use <see cref="SetPixelAspect(NativeRenderer*, double)"/>.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_set_frame_size")]
        public static partial void SetFrameSize(NativeRenderer* renderer, int width, int height);

        /// <summary>
        /// Set the source image size in pixels.
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <remarks>
        /// This affects some ass tags, like 3D transforms, and is used
        /// to calculate the source aspect ratio and blur scale. If subtitles
        /// specify valid LayoutRes headers, those will take precedence.<br/>
        /// The source image size can be reset to default by setting width and
        /// height to 0. The value set with this function can influence the pixel
        /// aspect ratio used for rendering.<br/>
        /// The values must be the actual storage size of the video stream,
        /// without any anamorphic de-squeeze applied.
        /// </remarks>
        /// <seealso cref="SetPixelAspect(NativeRenderer*, double)"/>
        [LibraryImport("libass", EntryPoint = "ass_set_storage_size")]
        public static partial void SetStorageSize(NativeRenderer* renderer, int width, int height);

        /// <summary>
        /// Set frame margins. These values may be negative if pan-and-scan is used.<br/>
        /// Margins are in pixels. Each value specifies the distance from the video rectangle
        /// to the renderer frame. If a given margin value is positive, there will be free space
        /// between the renderer frame and the video area. If a given margin is negative,
        /// then the frame is inside the video (cropped).
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="top">Top margin</param>
        /// <param name="botton">Bottom margin</param>
        /// <param name="left">Left margin</param>
        /// <param name="right">Right margin</param>
        /// <remarks>
        /// <para>
        /// The renderer will try to keep subtitles inside the frame area.
        /// If possible, text is laid out so it is inside the cropped area. Subtitle events
        /// that can't be moved are cropped against the frame area.
        /// </para><para>
        /// This function can be used to render subtitles into the empty
        /// areas if margins are positive, i.e. the video area is smaller than
        /// the frame. (Traditionally, this has been used to show subtitles in the bottom
        /// "black bar" between the video bottom and the screen border when playing
        /// 16:9 widescreen video on a 4:3 standard monitor.)
        /// </para>
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_set_margins")]
        public static partial void SetMargins(NativeRenderer* renderer, int top, int botton, int left, int right);

        /// <summary>
        /// Set whether margins should be used for placing regular events
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="useMargins">If margins should be used</param>
        [LibraryImport("libass", EntryPoint = "ass_set_use_margins")]
        public static partial void SetUseMargins(NativeRenderer* renderer, [MarshalAs(UnmanagedType.Bool)] bool useMargins);

        /// <summary>
        /// Set aspect ratio parameters
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="dar">Display aspect ratio, prescaled for output PAR</param>
        /// <param name="sar">Storage aspect ratio</param>
        [Obsolete("Use SetPixelAspect instead")]
        [LibraryImport("libass", EntryPoint = "ass_set_aspect_ratio")]
        public static partial void SetAspectRatio(NativeRenderer* renderer, double dar, double sar);

        /// <summary>
        /// Set a fixed font scaling factor
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="scale">Scaling factor, default is 1.0</param>
        [LibraryImport("libass", EntryPoint = "ass_set_font_scale")]
        public static partial void SetFontScale(NativeRenderer* renderer, double scale);

        /// <summary>
        /// Set the font hinting method
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="hinting">Hinting method</param>
        [LibraryImport("libass", EntryPoint = "ass_set_hinting")]
        public static partial void SetHinting(NativeRenderer* renderer, Hinting hinting);

        /// <summary>
        /// Set line spacing (Not scaled with frame size)
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="spacing">Line spacing in pixels</param>
        [LibraryImport("libass", EntryPoint = "ass_set_line_spacing")]
        public static partial void SetLineSpacing(NativeRenderer* renderer, double spacing);

        /// <summary>
        /// Get the list of available font providers
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <param name="providers">List of default prividers</param>
        /// <param name="size">Number of providers</param>
        /// <remarks>
        /// The output array is allocated with malloc and can be released with free().
        /// If an allocation occurs, size is set to (size_t)-1.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_get_available_font_providers")]
        public static partial void GetAvailableFontProviders(NativeLibrary* library, out IntPtr providers, out UIntPtr size);

        /// <summary>
        /// Set font lookup defaults
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="defaultFont">Path to the default font to use.
        /// Must be supplied if all system FontProviders are disabled or unavailable</param>
        /// <param name="defaultFamily">Fallback font family, or null</param>
        /// <param name="dfp">Which font provider to use</param>
        /// <param name="config">Path to font configuration file, or null.
        /// Only relevant if fontconfig is used. Encoding must match fontconfig encoding</param>
        /// <param name="update">Whether FontConfig cache should be build or updated now</param>
        [LibraryImport("libass", EntryPoint = "ass_set_fonts")]
        public static partial void SetFonts(NativeRenderer* renderer, [MarshalAs(UnmanagedType.LPStr)] string? defaultFont, [MarshalAs(UnmanagedType.LPStr)] string? defaultFamily, DefaultFontProvider dfp, [MarshalAs(UnmanagedType.LPStr)] string? config, [MarshalAs(UnmanagedType.Bool)] bool update);

        /// <summary>
        /// Render a frame, producing a list of <see cref="NativeImage"/>s
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="track">Subtitle track</param>
        /// <param name="now">Video timestamp in milliseconds</param>
        /// <param name="detectChange">Compare to the previous call and set
        /// to 1 if positions may have changed, or 2 if content may have changed</param>
        /// <returns>List of Images</returns>
        [LibraryImport("libass", EntryPoint = "ass_render_frame")]
        public static partial NativeImage* RenderFrame(NativeRenderer* renderer, NativeTrack* track, long now, int detectChange);

        /// <summary>
        /// Allocate a new empty <see cref="NativeTrack"/>
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <returns>Pointer to empty track, or null on failure</returns>
        [LibraryImport("libass", EntryPoint = "ass_new_track")]
        public static partial NativeTrack* NewTrack(NativeLibrary* library);

        /// <summary>
        /// Deallocate a track and all its child objects (styles, events)
        /// </summary>
        /// <param name="track">Track to deallocate</param>
        [LibraryImport("libass", EntryPoint = "ass_free_track")]
        public static partial void FreeTrack(NativeTrack* track);

        /// <summary>
        /// Allocate a new style
        /// </summary>
        /// <param name="track">Track</param>
        /// <returns>Newly allocated style id, >= 0, or a value < 0 on failure</returns>
        [LibraryImport("libass", EntryPoint = "ass_alloc_style")]
        public static partial int AllocStyle(NativeTrack* track);

        /// <summary>
        /// Allocate a new event
        /// </summary>
        /// <param name="track">Track</param>
        /// <returns>Newly allocated event id, >= 0, or a value < 0 on failure</returns>
        [LibraryImport("libass", EntryPoint = "ass_alloc_event")]
        public static partial int AllocEvent(NativeTrack* track);

        /// <summary>
        /// Delete a style
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="styleId">ID of the style</param>
        /// <remarks>
        /// Deallocates style data. Does not modify <see cref="NativeTrack.NumStyles"/>.<br/>
        /// Freeing a style without subsequently setting <see cref="NativeTrack.NumStyles"/>
        /// to a value less than or equal to the freed style id before calling any other libass
        /// api function is undefined behavior.<br/>
        /// Additionally, a freed style still being referenced by an event in <see cref="NativeTrack.Events"/>
        /// will also result in undefined behavior.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_free_style")]
        public static partial void FreeStyle(NativeTrack* track, int styleId);

        /// <summary>
        /// Delete an event
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="eventId">ID of the event</param>
        /// <remarks>
        /// Deallocates event data. Does not modify <see cref="NativeTrack.NumEvents"/>.<br/>
        /// Freeing an event without subsequently setting <see cref="NativeTrack.NumEvents"/>
        /// to a value less than or equal to the freed event id before calling any other libass
        /// api function is undefined behavior.<br/>
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_free_event")]
        public static partial void FreeEvent(NativeTrack* track, int eventId);

        /// <summary>
        /// Parse a chunk of subtitle stream data
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="data">String to parse</param>
        /// <param name="size">Length of data</param>
        [LibraryImport("libass", EntryPoint = "ass_process_data")]
        public static partial void ProcessData(NativeTrack* track, [MarshalAs(UnmanagedType.LPStr)] string data, int size);

        /// <summary>
        /// Parse the Codec Private section of the subtitle stream, in Matroska format
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="data">String to parse</param>
        /// <param name="size">Length of data</param>
        [LibraryImport("libass", EntryPoint = "ass_process_codec_private")]
        public static partial void ProcessCodecPrivate(NativeTrack* track, [MarshalAs(UnmanagedType.LPStr)] string data, int size);

        /// <summary>
        /// Parse a chunk of subtitle stream data
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="data">String to parse</param>
        /// <param name="size">Length of data</param>
        /// <param name="timecode">Starting time of the event in milliseconds</param>
        /// <param name="duration">Duration of the event in milliseconds</param>
        /// <remarks>
        /// See the Matroska specification for details.
        /// In later libass versions(since LIBASS_VERSION==0x01300001), using this
        /// function means you agree not to modify events manually, or using other
        /// functions manipulating the event list like <see cref="ProcessData(NativeTrack*, string, int)"/>. 
        /// If you do anyway, the internal duplicate checking might break.
        /// Calling <see cref="FlushEvents(NativeTrack*)"/> is still allowed.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_process_chunk")]
        public static partial void ProcessChunk(NativeTrack* track, [MarshalAs(UnmanagedType.LPStr)] string data, int size, long timecode, long duration);

        /// <summary>
        /// Read subtitles from a file
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <param name="fileName">File name</param>
        /// <param name="codePage">Codepage encoding (iconv format)</param>
        /// <returns>Newly allocated track or null on faulure</returns>
        /// <remarks>
        /// On Microsoft Windows, when using Win32 APIs, the frame must be in
        /// either UTF-8 mixed with lone or paired UTF-16 surrogates encoded
        /// like in CESU-8 or the encoding accepted by fopen with the former
        /// taking precedence if both versions are valid and exist.<br/>
        /// On all other systems there is no need for such consideration.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_read_file")]
        public static partial NativeTrack* ReadFile(NativeLibrary* library, [MarshalAs(UnmanagedType.LPStr)] string fileName, [MarshalAs(UnmanagedType.LPStr)] string? codePage);

        /// <summary>
        /// Read subtitles from memory
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <param name="buffer">Subtitle text</param>
        /// <param name="bufferSize">Size of the buffer</param>
        /// <param name="codePage">Codepage encoding (iconv format)</param>
        /// <returns>Newly allocated track or null on failure</returns>
        [LibraryImport("libass", EntryPoint = "ass_read_memory")]
        public static partial NativeTrack* ReadMemory(NativeLibrary* library, [MarshalAs(UnmanagedType.LPStr)] string buffer, UIntPtr bufferSize, [MarshalAs(UnmanagedType.LPStr)] string? codePage);

        /// <summary>
        /// Read styles from file into an already-initialized track
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="fileName">File name</param>
        /// <param name="codePage">Codepage encoding (iconv format)</param>
        /// <returns>0 on success</returns>
        /// <remarks>
        /// On Microsoft Windows, when using Win32 APIs, the frame must be in
        /// either UTF-8 mixed with lone or paired UTF-16 surrogates encoded
        /// like in CESU-8 or the encoding accepted by fopen with the former
        /// taking precedence if both versions are valid and exist.<br/>
        /// On all other systems there is no need for such consideration.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_read_styles")]
        public static partial int ReadStyles(NativeTrack* track, [MarshalAs (UnmanagedType.LPStr)] string fileName, [MarshalAs(UnmanagedType.LPStr)] string codePage);

        /// <summary>
        /// Add a memory font
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <param name="name">Name of the attachment</param>
        /// <param name="data">Binary font data</param>
        /// <param name="dataSize">Size of data</param>
        [LibraryImport("libass", EntryPoint = "ass_add_font")]
        public static partial void AddFont(NativeLibrary* library, [MarshalAs(UnmanagedType.LPStr)] string name, in byte[] data, int dataSize);

        /// <summary>
        /// Remove all fonts stores in a library object
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <remarks>
        /// This can only be called safely if all <see cref="NativeTrack"/> objects
        /// and Renderer instances associated with the library handle have been released first.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_clear_fonts")]
        public static partial void ClearFonts(NativeLibrary* library);

        /// <summary>
        /// Calculates timeshift from now to the start of some other subtitle event
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="now">Current time in milliseconds</param>
        /// <param name="movement">How many events to skip from the currently-displayed event</param>
        /// <returns>Timeshift in milliseconds</returns>
        /// <remarks>
        /// With regards to <paramref name="movement"/>, +2 means "the one after the next",
        /// -1 means "previous", etc.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_step_sub")]
        public static partial long StepSub(NativeTrack* track, long now, int movement);

        /// <summary>
        /// Explicitly process style overrides for a track
        /// </summary>
        /// <param name="track">Track handle</param>
        [LibraryImport("libass", EntryPoint = "ass_process_force_style")]
        public static partial void ProcessForceStyle(NativeTrack* track);

        /// <summary>
        /// Register a callback for debug/info messages
        /// </summary>
        /// <param name="library">Library handle</param>
        /// <param name="callback">Callback function</param>
        /// <remarks>
        /// If a callback is registered, it is called for every message emitted
        /// by libass. The callback receives a format string and a list of arguments
        /// to be used for the printf family of functions.<br/>
        /// Additionally, a log level from 0 (FATAL) to 7 (DEBUG) is passed.
        /// Usually, level 5 should be used by applications.<br/>
        /// If no callback is set, all messages level < 5 are printed to stderr,
        /// prefixed with [ass].
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_set_message_cb")]
        public static partial void SetMessageCallback(NativeLibrary* library, MessageCallback callback);

        [Obsolete("Does nothing")]
        [LibraryImport("libass", EntryPoint = "ass_fonts_update")]
        public static partial int FontsUpdate(NativeRenderer* renderer);

        /// <summary>
        /// Set cache hard limits. Do not set, or set to zero, for reasonable defaults.
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="glyphMax">Maximum number of cached glyphs</param>
        /// <param name="bitmapMaxSize">Maximum bitmap cache size in megabytes</param>
        [LibraryImport("libass", EntryPoint = "ass_set_cache_limits")]
        public static partial void SetCacheLimits(NativeRenderer* renderer, int glyphMax, int bitmapMaxSize);

        /// <summary>
        /// Flush buffered events
        /// </summary>
        /// <param name="track">Track</param>
        [LibraryImport("libass", EntryPoint = "ass_flush_events")]
        public static partial void FlushEvents(NativeTrack* track);

        /// <summary>
        /// Set the shaping level
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="level">Shaping level</param>
        /// <remarks>
        /// This is merely a hint, the renderer will use
        /// whatever is available if the request cannot be fulfilled.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_set_shaper")]
        public static partial void SetShaper(NativeRenderer* renderer, ShapingLevel level);

        /// <summary>
        /// Set the vertical line position
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="position">Line spacing in pixels</param>
        [LibraryImport("libass", EntryPoint = "ass_set_line_position")]
        public static partial void SetLinePosition(NativeRenderer* renderer, double position);

        /// <summary>
        /// Set the pixel aspect ratio correction. This is the ratio of pixel width to pixel height.
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="par">Pixel aspect ratio (1.0 = square, 0 = default)</param>
        /// <remarks>
        /// <para>
        /// Generally, this is <c>(d_w / d_w) / (s_w / s_h)</c>, where <c>s_w</c> and <c>s_h</c>
        /// is the video storage size, and <c>d_w</c> and <c>d_h</c> is the video display size.
        /// (Display and storage size can be different for anamorphic video, such as DVDs).
        /// </para><para>
        /// If the pixel aspect ratio is 0, or if the aspect ratio has never been set by
        /// calling this function, libass will calculate a default pixel aspect ratio
        /// out of values set with <see cref="SetFrameSize(NativeRenderer*, int, int)"/> and
        /// <see cref="SetStorageSize(NativeRenderer*, int, int)"/>. Note that this corresponds to
        /// an isotropically scaled version of the video display size. If the storage size
        /// has not been set, a pixel aspect ratio of 1 is assumed.
        /// </para><para>
        /// If subtitles specify valid LayoutRes headers, the API-configured pixel aspect value
        /// is discarded in favor of one calculated out of the headers and values set
        /// with <see cref="SetFrameSize(NativeRenderer*, int, int)"/>.
        /// </para>
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_set_pixel_aspect")]
        public static partial void SetPixelAspect(NativeRenderer* renderer, double par);

        /// <summary>
        /// Set selective style override mode
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="bits">Bit mask comprised of <see cref="OverrideBits"/> values</param>
        /// <remarks>
        /// <para>
        /// If enabled, the renderer attempts to override the ASS script's styling of
        /// normal subtitles, without affecting explicitly positioned text.If an event
        /// looks like a normal subtitle, parts of the font style are copied from the
        /// user style set with <see cref="SetSelectiveStyleOverride(NativeRenderer*, void*)"/>.
        /// </para><para>
        /// <b>WARNING</b>: the heuristic used for deciding when to override the style is rather
        /// rough, and enabling this option can lead to incorrectly rendered
        /// subtitles. Since the ASS format doesn't have any support for
        /// allowing end-users to customize subtitle styling, this feature can
        /// only be implemented on "best effort" basis, and has to rely on
        /// heuristics that can easily break.
        /// </para>
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_set_selective_style_override_enabled")]
        public static partial void SetSelectiveStyleOverrideEnabled(NativeRenderer* renderer, OverrideBits bits);

        /// <summary>
        /// Set style for selective style override
        /// </summary>
        /// <param name="renderer">Renderer handle</param>
        /// <param name="style">Settings to use if override is enabled</param>
        /// <remarks>
        /// Applications should initialize <paramref name="style"/> with {0} 
        /// before setting fields. Strings will be copied by the function.
        /// </remarks>
        /// <seealso cref="SetSelectiveStyleOverrideEnabled(NativeRenderer*, OverrideBits)"/>
        [LibraryImport("libass", EntryPoint = "ass_set_selective_style_override")]
        public static partial void SetSelectiveStyleOverride(NativeRenderer* renderer, NativeStyle* style);

        /// <summary>
        /// Set whether the ReadOrder field when processing a packet with <see cref="ProcessChunk(NativeTrack*, string, int, long, long)"/>
        /// should be used for eliminating duplicates
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="checkReadOrder">0 = no, 1 = yes, other = undefined</param>
        [LibraryImport("libass", EntryPoint = "ass_set_check_readorder")]
        public static partial void SetCheckReadOrder(NativeTrack* track, int checkReadOrder);

        /// <summary>
        /// Enable or disable certain features
        /// </summary>
        /// <param name="track">Track</param>
        /// <param name="feature">Feature to enable or disable</param>
        /// <param name="enable">Whether to enable the feature</param>
        /// <returns>0 if the feature was set, -1 if feature is unknown</returns>
        /// <remarks>
        /// <para>
        /// This manages flags that control the behavior of the renderer and
        /// how certain tags, etc within the track are interpreted. The defaults
        /// on a newly created <see cref="NativeTrack"/> are such that rendering is
        /// compatible with traditional renderers like VSFilter, and/or old versions of libass.
        /// </para><para>
        /// Calling <see cref="ProcessData(NativeTrack*, string, int)"/> or
        /// <see cref="ProcessCodecPrivate(NativeTrack*, string, int)"/> may change
        /// some of these flags according to file headers.
        /// (<see cref="ProcessChunk(NativeTrack*, string, int, long, long)"/>
        /// will not change any of the flags).
        /// </para><para>
        /// Additions to <see cref="Feature"/> are backwards-compatible to old libass
        /// releases (ABI compativility).
        /// </para>
        /// After calling <see cref="RenderFrame(NativeRenderer*, NativeTrack*, long, int)"/>,
        /// changing features is no longer allowed.
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_track_set_feature")]
        public static partial int SetTrackFeature(NativeTrack* track, Feature feature, [MarshalAs(UnmanagedType.Bool)] bool enable);

        /// <summary>
        /// Allocates memory that can be safely freed by libass later
        /// </summary>
        /// <param name="size">Number of bytes to allocate</param>
        /// <returns>Pointer to the allocated buffer, or null on failure</returns>
        /// <remarks>
        /// Use this to allocate buffers you'll use to manually modify <see cref="NativeTrack"/> events
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_malloc")]
        public static partial void* Malloc(UIntPtr size);

        /// <summary>
        /// Releases memory previously allocated within libass
        /// </summary>
        /// <param name="pointer">Pointer to the buffer to release</param>
        /// <remarks>
        /// Use this to free memory allocated using <see cref="Malloc(nuint)"/>
        /// </remarks>
        [LibraryImport("libass", EntryPoint = "ass_free")]
        public static partial void Free(void* pointer);
    }

    public delegate void MessageCallback(int level, [MarshalAs(UnmanagedType.LPStr)] string format, params object[] args);
    public unsafe struct NativeLibrary;
    public unsafe struct NativeRenderer;
}
