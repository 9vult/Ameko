// SPDX-License-Identifier: MPL-2.0

/// C bindings
pub const c = @cImport({
    // FFmpegSource 2
    @cInclude("ffms.h");

    // libass
    @cInclude("ass.h");
});

const mem = @import("std").mem;

pub const av = extern struct {
    pub const struct_AVRational = extern struct {
        num: c_int = mem.zeroes(c_int),
        den: c_int = mem.zeroes(c_int),
    };
    pub const AVRational = struct_AVRational;

    pub const struct_AVCodecTag = opaque {};
    pub const struct_AVOption_3 = opaque {};
    pub const AVClassCategory = c_uint;
    pub const struct_AVOptionRanges = opaque {};
    pub const enum_AVCodecID = c_uint;
    pub const enum_AVIODataMarkerType = c_uint;
    pub const AVMEDIA_TYPE_UNKNOWN: c_int = -1;
    pub const AVMEDIA_TYPE_VIDEO: c_int = 0;
    pub const AVMEDIA_TYPE_AUDIO: c_int = 1;
    pub const AVMEDIA_TYPE_DATA: c_int = 2;
    pub const AVMEDIA_TYPE_SUBTITLE: c_int = 3;
    pub const AVMEDIA_TYPE_ATTACHMENT: c_int = 4;
    pub const AVMEDIA_TYPE_NB: c_int = 5;
    pub const enum_AVMediaType = c_int;
    pub const enum_AVDiscard = c_int;
    pub const enum_AVPacketSideDataType = c_uint;
    pub const enum_AVFieldOrder = c_uint;
    pub const enum_AVColorRange = c_uint;
    pub const enum_AVColorPrimaries = c_uint;
    pub const enum_AVColorTransferCharacteristic = c_uint;
    pub const enum_AVStreamGroupParamsType = c_uint;
    pub const enum_AVColorSpace = c_uint;
    pub const enum_AVChromaLocation = c_uint;
    pub const enum_AVChannelOrder = c_uint;
    pub const enum_AVChannel = c_int;
    pub const enum_AVDurationEstimationMethod = c_uint;
    pub const enum_AVPixelFormat = c_int;
    pub const enum_AVSampleFormat = c_int;

    pub const struct_AVDictionary = opaque {};
    pub const AVDictionary = struct_AVDictionary;

    pub const struct_AVDictionaryEntry = extern struct {
        key: [*c]u8 = mem.zeroes([*c]u8),
        value: [*c]u8 = mem.zeroes([*c]u8),
    };
    pub const AVDictionaryEntry = struct_AVDictionaryEntry;

    pub const struct_AVInputFormat = extern struct {
        name: [*c]const u8 = mem.zeroes([*c]const u8),
        long_name: [*c]const u8 = mem.zeroes([*c]const u8),
        flags: c_int = mem.zeroes(c_int),
        extensions: [*c]const u8 = mem.zeroes([*c]const u8),
        codec_tag: [*c]const ?*const struct_AVCodecTag = mem.zeroes([*c]const ?*const struct_AVCodecTag),
        priv_class: [*c]const AVClass = mem.zeroes([*c]const AVClass),
        mime_type: [*c]const u8 = mem.zeroes([*c]const u8),
    };

    pub const AVInputFormat = struct_AVInputFormat;

    pub const struct_AVOutputFormat = extern struct {
        name: [*c]const u8 = mem.zeroes([*c]const u8),
        long_name: [*c]const u8 = mem.zeroes([*c]const u8),
        mime_type: [*c]const u8 = mem.zeroes([*c]const u8),
        extensions: [*c]const u8 = mem.zeroes([*c]const u8),
        audio_codec: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        video_codec: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        subtitle_codec: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        flags: c_int = mem.zeroes(c_int),
        codec_tag: [*c]const ?*const struct_AVCodecTag = mem.zeroes([*c]const ?*const struct_AVCodecTag),
        priv_class: [*c]const AVClass = mem.zeroes([*c]const AVClass),
    };

    pub const struct_AVIOContext = extern struct {
        av_class: [*c]const AVClass = mem.zeroes([*c]const AVClass),
        buffer: [*c]u8 = mem.zeroes([*c]u8),
        buffer_size: c_int = mem.zeroes(c_int),
        buf_ptr: [*c]u8 = mem.zeroes([*c]u8),
        buf_end: [*c]u8 = mem.zeroes([*c]u8),
        @"opaque": ?*anyopaque = mem.zeroes(?*anyopaque),
        read_packet: ?*const fn (?*anyopaque, [*c]u8, c_int) callconv(.c) c_int = mem.zeroes(?*const fn (?*anyopaque, [*c]u8, c_int) callconv(.c) c_int),
        write_packet: ?*const fn (?*anyopaque, [*c]const u8, c_int) callconv(.c) c_int = mem.zeroes(?*const fn (?*anyopaque, [*c]const u8, c_int) callconv(.c) c_int),
        seek: ?*const fn (?*anyopaque, i64, c_int) callconv(.c) i64 = mem.zeroes(?*const fn (?*anyopaque, i64, c_int) callconv(.c) i64),
        pos: i64 = mem.zeroes(i64),
        eof_reached: c_int = mem.zeroes(c_int),
        @"error": c_int = mem.zeroes(c_int),
        write_flag: c_int = mem.zeroes(c_int),
        max_packet_size: c_int = mem.zeroes(c_int),
        min_packet_size: c_int = mem.zeroes(c_int),
        checksum: c_ulong = mem.zeroes(c_ulong),
        checksum_ptr: [*c]u8 = mem.zeroes([*c]u8),
        update_checksum: ?*const fn (c_ulong, [*c]const u8, c_uint) callconv(.c) c_ulong = mem.zeroes(?*const fn (c_ulong, [*c]const u8, c_uint) callconv(.c) c_ulong),
        read_pause: ?*const fn (?*anyopaque, c_int) callconv(.c) c_int = mem.zeroes(?*const fn (?*anyopaque, c_int) callconv(.c) c_int),
        read_seek: ?*const fn (?*anyopaque, c_int, i64, c_int) callconv(.c) i64 = mem.zeroes(?*const fn (?*anyopaque, c_int, i64, c_int) callconv(.c) i64),
        seekable: c_int = mem.zeroes(c_int),
        direct: c_int = mem.zeroes(c_int),
        protocol_whitelist: [*c]const u8 = mem.zeroes([*c]const u8),
        protocol_blacklist: [*c]const u8 = mem.zeroes([*c]const u8),
        write_data_type: ?*const fn (?*anyopaque, [*c]const u8, c_int, enum_AVIODataMarkerType, i64) callconv(.c) c_int = mem.zeroes(?*const fn (?*anyopaque, [*c]const u8, c_int, enum_AVIODataMarkerType, i64) callconv(.c) c_int),
        ignore_boundary_point: c_int = mem.zeroes(c_int),
        buf_ptr_max: [*c]u8 = mem.zeroes([*c]u8),
        bytes_read: i64 = mem.zeroes(i64),
        bytes_written: i64 = mem.zeroes(i64),
    };
    pub const AVIOContext = struct_AVIOContext;

    pub const struct_AVClass = extern struct {
        class_name: [*c]const u8 = mem.zeroes([*c]const u8),
        item_name: ?*const fn (?*anyopaque) callconv(.c) [*c]const u8 = mem.zeroes(?*const fn (?*anyopaque) callconv(.c) [*c]const u8),
        option: ?*const struct_AVOption_3 = mem.zeroes(?*const struct_AVOption_3),
        version: c_int = mem.zeroes(c_int),
        log_level_offset_offset: c_int = mem.zeroes(c_int),
        parent_log_context_offset: c_int = mem.zeroes(c_int),
        category: AVClassCategory = mem.zeroes(AVClassCategory),
        get_category: ?*const fn (?*anyopaque) callconv(.c) AVClassCategory = mem.zeroes(?*const fn (?*anyopaque) callconv(.c) AVClassCategory),
        query_ranges: ?*const fn ([*c]?*struct_AVOptionRanges, ?*anyopaque, [*c]const u8, c_int) callconv(.c) c_int = mem.zeroes(?*const fn ([*c]?*struct_AVOptionRanges, ?*anyopaque, [*c]const u8, c_int) callconv(.c) c_int),
        child_next: ?*const fn (?*anyopaque, ?*anyopaque) callconv(.c) ?*anyopaque = mem.zeroes(?*const fn (?*anyopaque, ?*anyopaque) callconv(.c) ?*anyopaque),
        child_class_iterate: ?*const fn ([*c]?*anyopaque) callconv(.c) [*c]const struct_AVClass = mem.zeroes(?*const fn ([*c]?*anyopaque) callconv(.c) [*c]const struct_AVClass),
    };
    pub const AVClass = struct_AVClass;

    pub const struct_AVBuffer = opaque {};
    pub const AVBuffer = struct_AVBuffer;

    pub const struct_AVBufferRef = extern struct {
        buffer: ?*AVBuffer = mem.zeroes(?*AVBuffer),
        data: [*c]u8 = mem.zeroes([*c]u8),
        size: usize = mem.zeroes(usize),
    };
    pub const AVBufferRef = struct_AVBufferRef;

    pub const struct_AVPacketSideData = extern struct {
        data: [*c]u8 = mem.zeroes([*c]u8),
        size: usize = mem.zeroes(usize),
        type: enum_AVPacketSideDataType = mem.zeroes(enum_AVPacketSideDataType),
    };
    pub const AVPacketSideData = struct_AVPacketSideData;

    pub const struct_AVPacket = extern struct {
        buf: [*c]AVBufferRef = mem.zeroes([*c]AVBufferRef),
        pts: i64 = mem.zeroes(i64),
        dts: i64 = mem.zeroes(i64),
        data: [*c]u8 = mem.zeroes([*c]u8),
        size: c_int = mem.zeroes(c_int),
        stream_index: c_int = mem.zeroes(c_int),
        flags: c_int = mem.zeroes(c_int),
        side_data: [*c]AVPacketSideData = mem.zeroes([*c]AVPacketSideData),
        side_data_elems: c_int = mem.zeroes(c_int),
        duration: i64 = mem.zeroes(i64),
        pos: i64 = mem.zeroes(i64),
        @"opaque": ?*anyopaque = mem.zeroes(?*anyopaque),
        opaque_ref: [*c]AVBufferRef = mem.zeroes([*c]AVBufferRef),
        time_base: AVRational = mem.zeroes(AVRational),
    };
    pub const AVPacket = struct_AVPacket;

    pub const struct_AVStream = extern struct {
        av_class: [*c]const AVClass = mem.zeroes([*c]const AVClass),
        index: c_int = mem.zeroes(c_int),
        id: c_int = mem.zeroes(c_int),
        codecpar: [*c]AVCodecParameters = mem.zeroes([*c]AVCodecParameters),
        priv_data: ?*anyopaque = mem.zeroes(?*anyopaque),
        time_base: AVRational = mem.zeroes(AVRational),
        start_time: i64 = mem.zeroes(i64),
        duration: i64 = mem.zeroes(i64),
        nb_frames: i64 = mem.zeroes(i64),
        disposition: c_int = mem.zeroes(c_int),
        discard: enum_AVDiscard = mem.zeroes(enum_AVDiscard),
        sample_aspect_ratio: AVRational = mem.zeroes(AVRational),
        metadata: ?*AVDictionary = mem.zeroes(?*AVDictionary),
        avg_frame_rate: AVRational = mem.zeroes(AVRational),
        attached_pic: AVPacket = mem.zeroes(AVPacket),
        side_data: [*c]AVPacketSideData = mem.zeroes([*c]AVPacketSideData),
        nb_side_data: c_int = mem.zeroes(c_int),
        event_flags: c_int = mem.zeroes(c_int),
        r_frame_rate: AVRational = mem.zeroes(AVRational),
        pts_wrap_bits: c_int = mem.zeroes(c_int),
    };
    pub const AVStream = struct_AVStream;

    pub const struct_AVChannelCustom = extern struct {
        id: enum_AVChannel = mem.zeroes(enum_AVChannel),
        name: [16]u8 = mem.zeroes([16]u8),
        @"opaque": ?*anyopaque = mem.zeroes(?*anyopaque),
    };
    pub const AVChannelCustom = struct_AVChannelCustom;

    const union_unnamed_4 = extern union {
        mask: u64,
        map: [*c]AVChannelCustom,
    };

    pub const struct_AVChannelLayout = extern struct {
        order: enum_AVChannelOrder = mem.zeroes(enum_AVChannelOrder),
        nb_channels: c_int = mem.zeroes(c_int),
        u: union_unnamed_4 = mem.zeroes(union_unnamed_4),
        @"opaque": ?*anyopaque = mem.zeroes(?*anyopaque),
    };
    pub const AVChannelLayout = struct_AVChannelLayout;

    pub const struct_AVCodecParameters = extern struct {
        codec_type: enum_AVMediaType = mem.zeroes(enum_AVMediaType),
        codec_id: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        codec_tag: u32 = mem.zeroes(u32),
        extradata: [*c]u8 = mem.zeroes([*c]u8),
        extradata_size: c_int = mem.zeroes(c_int),
        coded_side_data: [*c]AVPacketSideData = mem.zeroes([*c]AVPacketSideData),
        nb_coded_side_data: c_int = mem.zeroes(c_int),
        format: c_int = mem.zeroes(c_int),
        bit_rate: i64 = mem.zeroes(i64),
        bits_per_coded_sample: c_int = mem.zeroes(c_int),
        bits_per_raw_sample: c_int = mem.zeroes(c_int),
        profile: c_int = mem.zeroes(c_int),
        level: c_int = mem.zeroes(c_int),
        width: c_int = mem.zeroes(c_int),
        height: c_int = mem.zeroes(c_int),
        sample_aspect_ratio: AVRational = mem.zeroes(AVRational),
        framerate: AVRational = mem.zeroes(AVRational),
        field_order: enum_AVFieldOrder = mem.zeroes(enum_AVFieldOrder),
        color_range: enum_AVColorRange = mem.zeroes(enum_AVColorRange),
        color_primaries: enum_AVColorPrimaries = mem.zeroes(enum_AVColorPrimaries),
        color_trc: enum_AVColorTransferCharacteristic = mem.zeroes(enum_AVColorTransferCharacteristic),
        color_space: enum_AVColorSpace = mem.zeroes(enum_AVColorSpace),
        chroma_location: enum_AVChromaLocation = mem.zeroes(enum_AVChromaLocation),
        video_delay: c_int = mem.zeroes(c_int),
        ch_layout: AVChannelLayout = mem.zeroes(AVChannelLayout),
        sample_rate: c_int = mem.zeroes(c_int),
        block_align: c_int = mem.zeroes(c_int),
        frame_size: c_int = mem.zeroes(c_int),
        initial_padding: c_int = mem.zeroes(c_int),
        trailing_padding: c_int = mem.zeroes(c_int),
        seek_preroll: c_int = mem.zeroes(c_int),
    };
    pub const AVCodecParameters = struct_AVCodecParameters;

    pub const struct_AVIAMFAudioElement = opaque {};
    pub const struct_AVIAMFMixPresentation = opaque {};

    const struct_unnamed_10 = extern struct {
        idx: c_uint = mem.zeroes(c_uint),
        horizontal: c_int = mem.zeroes(c_int),
        vertical: c_int = mem.zeroes(c_int),
    };

    pub const struct_AVStreamGroupTileGrid = extern struct {
        av_class: [*c]const AVClass = mem.zeroes([*c]const AVClass),
        nb_tiles: c_uint = mem.zeroes(c_uint),
        coded_width: c_int = mem.zeroes(c_int),
        coded_height: c_int = mem.zeroes(c_int),
        offsets: [*c]struct_unnamed_10 = mem.zeroes([*c]struct_unnamed_10),
        background: [4]u8 = mem.zeroes([4]u8),
        horizontal_offset: c_int = mem.zeroes(c_int),
        vertical_offset: c_int = mem.zeroes(c_int),
        width: c_int = mem.zeroes(c_int),
        height: c_int = mem.zeroes(c_int),
    };

    const union_unnamed_9 = extern union {
        iamf_audio_element: ?*struct_AVIAMFAudioElement,
        iamf_mix_presentation: ?*struct_AVIAMFMixPresentation,
        tile_grid: [*c]struct_AVStreamGroupTileGrid,
    };

    pub const struct_AVStreamGroup = extern struct {
        av_class: [*c]const AVClass = mem.zeroes([*c]const AVClass),
        priv_data: ?*anyopaque = mem.zeroes(?*anyopaque),
        index: c_uint = mem.zeroes(c_uint),
        id: i64 = mem.zeroes(i64),
        type: enum_AVStreamGroupParamsType = mem.zeroes(enum_AVStreamGroupParamsType),
        params: union_unnamed_9 = mem.zeroes(union_unnamed_9),
        metadata: ?*AVDictionary = mem.zeroes(?*AVDictionary),
        nb_streams: c_uint = mem.zeroes(c_uint),
        streams: [*c][*c]AVStream = mem.zeroes([*c][*c]AVStream),
        disposition: c_int = mem.zeroes(c_int),
    };
    pub const AVStreamGroup = struct_AVStreamGroup;

    pub const struct_AVChapter = extern struct {
        id: i64 = mem.zeroes(i64),
        time_base: AVRational = mem.zeroes(AVRational),
        start: i64 = mem.zeroes(i64),
        end: i64 = mem.zeroes(i64),
        metadata: ?*AVDictionary = mem.zeroes(?*AVDictionary),
    };
    pub const AVChapter = struct_AVChapter;

    pub const struct_AVProgram = extern struct {
        id: c_int = mem.zeroes(c_int),
        flags: c_int = mem.zeroes(c_int),
        discard: enum_AVDiscard = mem.zeroes(enum_AVDiscard),
        stream_index: [*c]c_uint = mem.zeroes([*c]c_uint),
        nb_stream_indexes: c_uint = mem.zeroes(c_uint),
        metadata: ?*AVDictionary = mem.zeroes(?*AVDictionary),
        program_num: c_int = mem.zeroes(c_int),
        pmt_pid: c_int = mem.zeroes(c_int),
        pcr_pid: c_int = mem.zeroes(c_int),
        pmt_version: c_int = mem.zeroes(c_int),
        start_time: i64 = mem.zeroes(i64),
        end_time: i64 = mem.zeroes(i64),
        pts_wrap_reference: i64 = mem.zeroes(i64),
        pts_wrap_behavior: c_int = mem.zeroes(c_int),
    };
    pub const AVProgram = struct_AVProgram;

    pub const struct_AVIOInterruptCB = extern struct {
        callback: ?*const fn (?*anyopaque) callconv(.c) c_int = mem.zeroes(?*const fn (?*anyopaque) callconv(.c) c_int),
        @"opaque": ?*anyopaque = mem.zeroes(?*anyopaque),
    };
    pub const AVIOInterruptCB = struct_AVIOInterruptCB;

    pub const struct_AVProfile = extern struct {
        profile: c_int = mem.zeroes(c_int),
        name: [*c]const u8 = mem.zeroes([*c]const u8),
    };
    pub const AVProfile = struct_AVProfile;

    pub const struct_AVCodec = extern struct {
        name: [*c]const u8 = mem.zeroes([*c]const u8),
        long_name: [*c]const u8 = mem.zeroes([*c]const u8),
        type: enum_AVMediaType = mem.zeroes(enum_AVMediaType),
        id: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        capabilities: c_int = mem.zeroes(c_int),
        max_lowres: u8 = mem.zeroes(u8),
        supported_framerates: [*c]const AVRational = mem.zeroes([*c]const AVRational),
        pix_fmts: [*c]const enum_AVPixelFormat = mem.zeroes([*c]const enum_AVPixelFormat),
        supported_samplerates: [*c]const c_int = mem.zeroes([*c]const c_int),
        sample_fmts: [*c]const enum_AVSampleFormat = mem.zeroes([*c]const enum_AVSampleFormat),
        priv_class: [*c]const AVClass = mem.zeroes([*c]const AVClass),
        profiles: [*c]const AVProfile = mem.zeroes([*c]const AVProfile),
        wrapper_name: [*c]const u8 = mem.zeroes([*c]const u8),
        ch_layouts: [*c]const AVChannelLayout = mem.zeroes([*c]const AVChannelLayout),
    };
    pub const AVCodec = struct_AVCodec;

    pub const av_format_control_message = ?*const fn ([*c]struct_AVFormatContext, c_int, ?*anyopaque, usize) callconv(.c) c_int;

    pub const struct_AVFormatContext = extern struct {
        av_class: [*c]const AVClass = mem.zeroes([*c]const AVClass),
        iformat: [*c]const struct_AVInputFormat = mem.zeroes([*c]const struct_AVInputFormat),
        oformat: [*c]const struct_AVOutputFormat = mem.zeroes([*c]const struct_AVOutputFormat),
        priv_data: ?*anyopaque = mem.zeroes(?*anyopaque),
        pb: [*c]AVIOContext = mem.zeroes([*c]AVIOContext),
        ctx_flags: c_int = mem.zeroes(c_int),
        nb_streams: c_uint = mem.zeroes(c_uint),
        streams: [*c][*c]AVStream = mem.zeroes([*c][*c]AVStream),
        nb_stream_groups: c_uint = mem.zeroes(c_uint),
        stream_groups: [*c][*c]AVStreamGroup = mem.zeroes([*c][*c]AVStreamGroup),
        nb_chapters: c_uint = mem.zeroes(c_uint),
        chapters: [*c][*c]AVChapter = mem.zeroes([*c][*c]AVChapter),
        url: [*c]u8 = mem.zeroes([*c]u8),
        start_time: i64 = mem.zeroes(i64),
        duration: i64 = mem.zeroes(i64),
        bit_rate: i64 = mem.zeroes(i64),
        packet_size: c_uint = mem.zeroes(c_uint),
        max_delay: c_int = mem.zeroes(c_int),
        flags: c_int = mem.zeroes(c_int),
        probesize: i64 = mem.zeroes(i64),
        max_analyze_duration: i64 = mem.zeroes(i64),
        key: [*c]const u8 = mem.zeroes([*c]const u8),
        keylen: c_int = mem.zeroes(c_int),
        nb_programs: c_uint = mem.zeroes(c_uint),
        programs: [*c][*c]AVProgram = mem.zeroes([*c][*c]AVProgram),
        video_codec_id: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        audio_codec_id: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        subtitle_codec_id: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        data_codec_id: enum_AVCodecID = mem.zeroes(enum_AVCodecID),
        metadata: ?*AVDictionary = mem.zeroes(?*AVDictionary),
        start_time_realtime: i64 = mem.zeroes(i64),
        fps_probe_size: c_int = mem.zeroes(c_int),
        error_recognition: c_int = mem.zeroes(c_int),
        interrupt_callback: AVIOInterruptCB = mem.zeroes(AVIOInterruptCB),
        debug: c_int = mem.zeroes(c_int),
        max_streams: c_int = mem.zeroes(c_int),
        max_index_size: c_uint = mem.zeroes(c_uint),
        max_picture_buffer: c_uint = mem.zeroes(c_uint),
        max_interleave_delta: i64 = mem.zeroes(i64),
        max_ts_probe: c_int = mem.zeroes(c_int),
        max_chunk_duration: c_int = mem.zeroes(c_int),
        max_chunk_size: c_int = mem.zeroes(c_int),
        max_probe_packets: c_int = mem.zeroes(c_int),
        strict_std_compliance: c_int = mem.zeroes(c_int),
        event_flags: c_int = mem.zeroes(c_int),
        avoid_negative_ts: c_int = mem.zeroes(c_int),
        audio_preload: c_int = mem.zeroes(c_int),
        use_wallclock_as_timestamps: c_int = mem.zeroes(c_int),
        skip_estimate_duration_from_pts: c_int = mem.zeroes(c_int),
        avio_flags: c_int = mem.zeroes(c_int),
        duration_estimation_method: enum_AVDurationEstimationMethod = mem.zeroes(enum_AVDurationEstimationMethod),
        skip_initial_bytes: i64 = mem.zeroes(i64),
        correct_ts_overflow: c_uint = mem.zeroes(c_uint),
        seek2any: c_int = mem.zeroes(c_int),
        flush_packets: c_int = mem.zeroes(c_int),
        probe_score: c_int = mem.zeroes(c_int),
        format_probesize: c_int = mem.zeroes(c_int),
        codec_whitelist: [*c]u8 = mem.zeroes([*c]u8),
        format_whitelist: [*c]u8 = mem.zeroes([*c]u8),
        protocol_whitelist: [*c]u8 = mem.zeroes([*c]u8),
        protocol_blacklist: [*c]u8 = mem.zeroes([*c]u8),
        io_repositioned: c_int = mem.zeroes(c_int),
        video_codec: [*c]const struct_AVCodec = mem.zeroes([*c]const struct_AVCodec),
        audio_codec: [*c]const struct_AVCodec = mem.zeroes([*c]const struct_AVCodec),
        subtitle_codec: [*c]const struct_AVCodec = mem.zeroes([*c]const struct_AVCodec),
        data_codec: [*c]const struct_AVCodec = mem.zeroes([*c]const struct_AVCodec),
        metadata_header_padding: c_int = mem.zeroes(c_int),
        @"opaque": ?*anyopaque = mem.zeroes(?*anyopaque),
        control_message_cb: av_format_control_message = mem.zeroes(av_format_control_message),
        output_ts_offset: i64 = mem.zeroes(i64),
        dump_separator: [*c]u8 = mem.zeroes([*c]u8),
        io_open: ?*const fn ([*c]struct_AVFormatContext, [*c][*c]AVIOContext, [*c]const u8, c_int, [*c]?*AVDictionary) callconv(.c) c_int = mem.zeroes(?*const fn ([*c]struct_AVFormatContext, [*c][*c]AVIOContext, [*c]const u8, c_int, [*c]?*AVDictionary) callconv(.c) c_int),
        io_close2: ?*const fn ([*c]struct_AVFormatContext, [*c]AVIOContext) callconv(.c) c_int = mem.zeroes(?*const fn ([*c]struct_AVFormatContext, [*c]AVIOContext) callconv(.c) c_int),
    };

    pub const AVFormatContext = struct_AVFormatContext;

    pub extern fn avformat_open_input(ps: [*c][*c]AVFormatContext, url: [*c]const u8, fmt: [*c]const AVInputFormat, options: [*c]?*AVDictionary) c_int;

    pub extern fn avformat_find_stream_info(ic: [*c]AVFormatContext, options: [*c]?*AVDictionary) c_int;

    pub extern fn avformat_close_input(s: [*c][*c]AVFormatContext) void;

    pub extern fn av_dict_get(m: ?*const AVDictionary, key: [*c]const u8, prev: [*c]const AVDictionaryEntry, flags: c_int) [*c]AVDictionaryEntry;

    pub extern fn avformat_version() c_uint;
};
