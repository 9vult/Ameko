// SPDX-License-Identifier: MPL-2.0

//! FFMS2 Audio-Video Provider

const std = @import("std");
const c = @import("c.zig").c;
const frames = @import("frames.zig");
const common = @import("common.zig");
const logger = @import("logger.zig");
const context = @import("context.zig");

pub const FfmsError = error{
    FileNotFound,
    VideoNotSupported,
    NoAudioTracks,
    NoVideoTracks,
    AudioTrackLoadingFailed,
    VideoTrackLoadingFailed,
    DecodingFirstFrameFailed,
    GetTrackInfoFailed,
    GetTrackTimeBaseFailed,
    GetFrameInfoFailed,
    OutOfMemory,
    VideoDecodeError,
    SettingOutputFormatFailed,
    AudioDataNotFound,
    DecodingAudioFailed,
};

// Global
var err_buffer = std.mem.zeroes([1024]u8);
var err_info = c.FFMS_ErrorInfo{
    .BufferSize = err_buffer.len,
    .Buffer = &err_buffer[0],
};
var is_initialized = false;

/// Get the current FFMS version
///
/// Does not require FFMS to be initialized.
pub fn GetVersion() common.BackingVersion {
    const version = c.FFMS_GetVersion();
    return .{
        .major = (version >> 24) & 0xFF,
        .minor = (version >> 16) & 0xFF,
        .patch = version & 0xFFFF,
    };
}

/// Initialize FFMS
pub fn Initialize() void {
    if (!is_initialized) {
        c.FFMS_Init(0, 0);
        is_initialized = true;
    }
}

/// Load a video
pub fn LoadVideo(g_ctx: *context.GlobalContext, file_name: [*c]u8, cache_file_name: [*c]u8, color_matrix: [*c]u8) FfmsError!void {
    var ctx = &g_ctx.*.ffms;
    var index = ctx.index;

    const indexer = c.FFMS_CreateIndexer(file_name, &err_info);

    if (indexer == null) {
        if (err_info.SubType == c.FFMS_ERROR_FILE_READ) {
            return FfmsError.FileNotFound;
        } else {
            return FfmsError.VideoNotSupported;
        }
    }

    // Get tracks
    var video_tracks = GetTracksOfType(indexer, c.FFMS_TYPE_VIDEO) catch |err| {
        return err; // Theoretically this is an OutOfMemory error
    };
    var audio_tracks = GetTracksOfType(indexer, c.FFMS_TYPE_AUDIO) catch |err| {
        return err; // Theoretically this is an OutOfMemory error
    };

    if (video_tracks.count() <= 0) {
        return FfmsError.NoVideoTracks;
    }

    const has_audio = audio_tracks.count() > 0;

    var video_track_number: c_int = -1;
    if (video_tracks.count() > 1) {
        // TODO: Ask which track should be opened!
        video_track_number = 0;
    }

    var audio_track_number: c_int = -1;
    if (has_audio) {
        // TODO: Ask which track should be opened!
        // Get a random audio track lol
        var itr = audio_tracks.iterator();
        if (itr.next()) |entry| {
            audio_track_number = entry.key_ptr.*;
        }
    }

    // Check if there's a cached version of the index
    index = c.FFMS_ReadIndex(cache_file_name, &err_info);

    // IndexBelongsToFile returns 0 on success
    if (index != null and c.FFMS_IndexBelongsToFile(index, file_name, &err_info) != 0) {
        index = null;
    }

    // Make sure the track we want is indexed
    if (index != null and video_track_number >= 0) {
        const temp_video_track = c.FFMS_GetTrackFromIndex(index, video_track_number);
        if (c.FFMS_GetNumFrames(temp_video_track) <= 0) {
            index = null;
        }

        if (has_audio) {
            const temp_audio_track = c.FFMS_GetTrackFromIndex(index, audio_track_number);
            if (c.FFMS_GetNumFrames(temp_audio_track) <= 0) {
                index = null;
            }
        }
    }

    // If we still don't have an index, index now
    if (index == null) {
        c.FFMS_TrackTypeIndexSettings(indexer, c.FFMS_TYPE_VIDEO, 1, 0);
        if (has_audio) { // TODO: Option to index all audio tracks
            c.FFMS_TrackIndexSettings(indexer, audio_track_number, 1, 0);
        }

        index = c.FFMS_DoIndexing2(indexer, c.FFMS_IEH_ABORT, &err_info);

        // Write the index to the cache
        // We can ignore the status of this because it doesn't really affect anything
        const write_result = c.FFMS_WriteIndex(cache_file_name, index, &err_info);
        if (write_result != 0) {
            logger.Error(err_info.Buffer[0..err_buffer.len]);
        }
    } else {
        c.FFMS_CancelIndexing(indexer);
    }

    // TODO: Clean up the cache

    // If no track number has been selected, use the first one
    if (video_track_number < 0) {
        video_track_number = c.FFMS_GetFirstIndexedTrackOfType(index, c.FFMS_TYPE_VIDEO, &err_info);

        if (video_track_number < 0) {
            return FfmsError.NoVideoTracks;
        }
    }

    // TODO: Add an option for unsafe seeking
    ctx.video_source = c.FFMS_CreateVideoSource(file_name, video_track_number, index, -1, c.FFMS_SEEK_NORMAL, &err_info);

    if (ctx.video_source == null) {
        return FfmsError.VideoTrackLoadingFailed;
    }

    // Get video properties
    const video_info = c.FFMS_GetVideoProperties(ctx.video_source);
    const temp_frame = c.FFMS_GetFrame(ctx.video_source, 0, &err_info);

    if (temp_frame == null) {
        return FfmsError.DecodingFirstFrameFailed;
    }

    const width = temp_frame.*.EncodedWidth;
    const height = temp_frame.*.EncodedHeight;

    var dar: f64 = 0;

    // Calculate aspect ratio
    if (video_info.*.SARDen > 0 and video_info.*.SARNum > 0) {
        dar = (@as(f64, @floatFromInt(width)) * @as(f64, @floatFromInt(video_info.*.SARNum))) / (@as(f64, @floatFromInt(height)) * @as(f64, @floatFromInt(video_info.*.SARDen)));
    } else {
        dar = @as(f64, @floatFromInt(width)) / @as(f64, @floatFromInt(height));
    }

    ctx.video_color_space = temp_frame.*.ColorSpace;
    ctx.video_color_range = temp_frame.*.ColorRange;
    SetColorSpace(color_matrix);

    const target_formats = [2]c_int{ c.FFMS_GetPixFmt("bgra"), -1 };
    if (c.FFMS_SetOutputFormatV2(ctx.video_source, &target_formats[0], width, height, c.FFMS_RESIZER_BICUBIC, &err_info) != 0) {
        return FfmsError.SettingOutputFormatFailed;
    }

    // TODO: Clean up
    const pitch_frame = c.FFMS_GetFrame(ctx.video_source, 0, &err_info);
    ctx.frame_width = @intCast(pitch_frame.*.EncodedWidth);
    ctx.frame_height = @intCast(pitch_frame.*.EncodedHeight);
    ctx.frame_pitch = @intCast(pitch_frame.*.Linesize[0]);

    // Frame information
    const track = c.FFMS_GetTrackFromVideo(ctx.video_source);
    if (track == null) {
        return FfmsError.GetTrackInfoFailed;
    }
    const time_base = c.FFMS_GetTimeBase(track);
    if (time_base == null) {
        return FfmsError.GetTrackTimeBaseFailed;
    }

    // Build list of timecodes and keyframes
    ctx.frame_count = video_info.*.NumFrames;

    // Allocate ArrayLists
    var keyframes_list = std.ArrayList(c_int).init(common.allocator);
    var timecodes_list = std.ArrayList(c_longlong).init(common.allocator);
    var intervals_list = std.ArrayList(c_longlong).init(common.allocator);

    errdefer timecodes_list.deinit();
    errdefer keyframes_list.deinit();
    errdefer intervals_list.deinit();

    var frame_number: c_int = 0;
    while (frame_number < ctx.frame_count) : (frame_number += 1) {
        const frame_info = c.FFMS_GetFrameInfo(track, frame_number);
        if (frame_info == null) {
            return FfmsError.GetFrameInfoFailed;
        }

        if (frame_info.*.KeyFrame != 0) {
            try keyframes_list.append(frame_number);
        }

        const wc_num = @as(f64, @floatFromInt(frame_info.*.PTS)) * @as(f64, @floatFromInt(time_base.*.Num));
        const wc_den = @as(f64, @floatFromInt(time_base.*.Den));
        const wallclock_ms = @as(c_longlong, @intFromFloat(wc_num / wc_den));
        try timecodes_list.append(wallclock_ms);
    }

    // Get the slices (de-inits the ArrayLists)
    ctx.keyframes = keyframes_list.toOwnedSlice() catch unreachable;
    ctx.timecodes = timecodes_list.toOwnedSlice() catch unreachable;

    // Calculate frame intervals
    var i: usize = 0;
    while (i + 1 < ctx.frame_count) : (i += 1) {
        try intervals_list.append(ctx.timecodes[i + 1] - ctx.timecodes[i]);
    }

    // Last interval is 0
    try intervals_list.append(0);

    ctx.frame_intervals = intervals_list.toOwnedSlice() catch unreachable;

    if (has_audio) {
        ctx.audio_source = c.FFMS_CreateAudioSource(file_name, audio_track_number, index, c.FFMS_DELAY_FIRST_VIDEO_TRACK, &err_info);
        if (ctx.audio_source == null) {
            return FfmsError.AudioTrackLoadingFailed;
        }

        // Get video properties
        const audio_info = c.FFMS_GetAudioProperties(ctx.audio_source);

        ctx.channel_count = audio_info.*.Channels;
        ctx.sample_rate = audio_info.*.SampleRate;
        ctx.sample_count = audio_info.*.NumSamples;

        // Configure output
        const rs_options = c.FFMS_CreateResampleOptions(ctx.audio_source);
        defer c.FFMS_DestroyResampleOptions(rs_options);

        rs_options.*.SampleFormat = c.FFMS_FMT_S16; // Use i16
        if (ctx.channel_count > 2) { // Downmix to stereo
            rs_options.*.ChannelLayout = c.FFMS_CH_FRONT_LEFT | c.FFMS_CH_FRONT_RIGHT;
            ctx.channel_count = 2;
        }
        _ = c.FFMS_SetOutputFormatA(ctx.audio_source, rs_options, &err_info);
    }
    logger.Debug("[FFMS2] Successfully loaded video file");
}

pub fn GetFrame(g_ctx: *context.GlobalContext, frame_number: c_int, out: *frames.VideoFrame) FfmsError!void {
    const ctx = &g_ctx.*.ffms;
    const frame = c.FFMS_GetFrame(ctx.video_source, frame_number, &err_info);
    if (frame == null) {
        return FfmsError.VideoDecodeError;
    }

    const src_ptr = frame.*.Data[0];

    const pitch: usize = @intCast(frame.*.Linesize[0]);
    const height: usize = @intCast(frame.*.EncodedHeight);
    const total_bytes = pitch * height;

    // Copy
    const dst = out.*.data[0..total_bytes];
    @memcpy(dst, src_ptr[0..total_bytes]);

    out.*.frame_number = frame_number;
    out.*.valid = 1;
}

pub fn GetAudio(g_ctx: *context.GlobalContext, buffer: *i16, start: i64, count: i64) FfmsError!void {
    const ctx = &g_ctx.*.ffms;
    const result = c.FFMS_GetAudio(ctx.audio_source, buffer, start, count, &err_info);
    if (result != 0) {
        return FfmsError.DecodingAudioFailed;
    }
}

// TODO: This thing
fn SetColorSpace(color_matrix: [*c]u8) void {
    _ = color_matrix;
    return;
}

/// Get tracks of the given type
pub fn GetTracksOfType(indexer: ?*c.FFMS_Indexer, track_type: c.FFMS_TrackType) !std.AutoHashMap(c_int, [*c]const u8) {
    var track_list = std.AutoHashMap(c_int, [*c]const u8).init(common.allocator);
    const track_count = c.FFMS_GetNumTracksI(indexer);

    var i: c_int = 0;
    while (i < track_count) : (i += 1) {
        if (c.FFMS_GetTrackTypeI(indexer, i) == track_type) {
            try track_list.put(i, c.FFMS_GetCodecNameI(indexer, i));
        }
    }
    return track_list;
}
