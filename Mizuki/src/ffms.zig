// SPDX-License-Identifier: MPL-2.0

//! FFMS2 Audio-Video Provider

const std = @import("std");
const c = @import("c.zig").c;
const frames = @import("frames.zig");
const common = @import("common.zig");
const logger = @import("logger.zig");

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

// Zero-init
var err_buffer = std.mem.zeroes([1024]u8);
var err_info = c.FFMS_ErrorInfo{
    .BufferSize = err_buffer.len,
    .Buffer = &err_buffer[0],
};

// Local variables
var index: ?*c.FFMS_Index = null;
var video_source: ?*c.FFMS_VideoSource = null;
var audio_source: ?*c.FFMS_AudioSource = null;

var color_space_buffer = std.mem.zeroes([128]u8);
var color_space = &color_space_buffer[0];
var video_color_space: c_int = -1;
var video_color_range: c_int = -1;

// Public variables

pub var keyframes: []c_int = undefined;
pub var timecodes: []c_longlong = undefined;
pub var frame_intervals: []c_longlong = undefined;
pub var frame_count: c_int = undefined;

pub var frame_width: usize = 0;
pub var frame_height: usize = 0;
pub var frame_pitch: usize = 0;

pub var channel_count: c_int = -1;
pub var sample_rate: c_int = -1;
pub var sample_count: i64 = -1;

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
    c.FFMS_Init(0, 0);
}

/// Load a video
pub fn LoadVideo(file_name: [*c]u8, cache_file_name: [*c]u8, color_matrix: [*c]u8) FfmsError!void {
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
        _ = c.FFMS_WriteIndex(cache_file_name, index, &err_info);
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
    video_source = c.FFMS_CreateVideoSource(file_name, video_track_number, index, -1, c.FFMS_SEEK_NORMAL, &err_info);

    if (video_source == null) {
        return FfmsError.VideoTrackLoadingFailed;
    }

    // Get video properties
    const video_info = c.FFMS_GetVideoProperties(video_source);
    const temp_frame = c.FFMS_GetFrame(video_source, 0, &err_info);

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

    video_color_space = temp_frame.*.ColorSpace;
    video_color_range = temp_frame.*.ColorRange;
    SetColorSpace(color_matrix);

    const target_formats = [2]c_int{ c.FFMS_GetPixFmt("bgra"), -1 };
    if (c.FFMS_SetOutputFormatV2(video_source, &target_formats[0], width, height, c.FFMS_RESIZER_BICUBIC, &err_info) != 0) {
        return FfmsError.SettingOutputFormatFailed;
    }

    // TODO: Clean up
    const pitch_frame = c.FFMS_GetFrame(video_source, 0, &err_info);
    frame_width = @intCast(pitch_frame.*.EncodedWidth);
    frame_height = @intCast(pitch_frame.*.EncodedHeight);
    frame_pitch = @intCast(pitch_frame.*.Linesize[0]);

    // Frame information
    const track = c.FFMS_GetTrackFromVideo(video_source);
    if (track == null) {
        return FfmsError.GetTrackInfoFailed;
    }
    const time_base = c.FFMS_GetTimeBase(track);
    if (time_base == null) {
        return FfmsError.GetTrackTimeBaseFailed;
    }

    // Build list of timecodes and keyframes
    frame_count = video_info.*.NumFrames;

    // Allocate ArrayLists
    var keyframes_list = std.ArrayList(c_int).init(common.allocator);
    var timecodes_list = std.ArrayList(c_longlong).init(common.allocator);
    var intervals_list = std.ArrayList(c_longlong).init(common.allocator);

    errdefer timecodes_list.deinit();
    errdefer keyframes_list.deinit();
    errdefer intervals_list.deinit();

    var frame_number: c_int = 0;
    while (frame_number < frame_count) : (frame_number += 1) {
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
    keyframes = keyframes_list.toOwnedSlice() catch unreachable;
    timecodes = timecodes_list.toOwnedSlice() catch unreachable;

    // Calculate frame intervals
    var i: usize = 0;
    while (i + 1 < frame_count) : (i += 1) {
        try intervals_list.append(timecodes[i + 1] - timecodes[i]);
    }

    // Last interval is 0
    try intervals_list.append(0);

    frame_intervals = intervals_list.toOwnedSlice() catch unreachable;

    if (has_audio) {
        audio_source = c.FFMS_CreateAudioSource(file_name, audio_track_number, index, c.FFMS_DELAY_FIRST_VIDEO_TRACK, &err_info);
        if (audio_source == null) {
            return FfmsError.AudioTrackLoadingFailed;
        }

        // Force usage of floats
        const rs_options = c.FFMS_CreateResampleOptions(audio_source);
        defer c.FFMS_DestroyResampleOptions(rs_options);
        rs_options.*.SampleFormat = c.FFMS_FMT_FLT;
        _ = c.FFMS_SetOutputFormatA(audio_source, rs_options, &err_info);

        // Get video properties
        const audio_info = c.FFMS_GetAudioProperties(audio_source);

        channel_count = audio_info.*.Channels;
        sample_rate = audio_info.*.SampleRate;
        sample_count = audio_info.*.NumSamples;
    }
    logger.Debug("[FFMS2] Successfully loaded video file");
}

pub fn GetFrame(frame_number: c_int, out: *frames.VideoFrame) FfmsError!void {
    const frame = c.FFMS_GetFrame(video_source, frame_number, &err_info);
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

pub fn GetAudio(buffer: *f32, start: i64, count: i64) FfmsError!void {
    const result = c.FFMS_GetAudio(audio_source, buffer, start, count, &err_info);
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
