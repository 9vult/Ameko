// SPDX-License-Identifier: MPL-2.0

//! FFMS2 Audio-Video Provider

const std = @import("std");
const c = @import("c.zig").c;
const common = @import("common.zig");
const known_folders = @import("known-folders");

const FfmsError = error{
    FileNotFound,
    VideoNotSupported,
    NoVideoTracks,
    VideoTrackLoadingFailed,
    DecodingFirstFrameFailed,
    GetTrackInfoFailed,
    GetTrackTimeBaseFailed,
    GetFrameInfoFailed,
    OutOfMemory,
};

// Zero-init
var err_buffer = std.mem.zeroes([1024]u8);
var err_info = c.FFMS_ErrorInfo{
    .BufferSize = err_buffer.len,
    .Buffer = &err_buffer[0],
};

var gpa: std.heap.GeneralPurposeAllocator(.{}) = .init;
const allocator = gpa.allocator();

// Local variables
var index: ?*c.FFMS_Index = null;
var video_source: ?*c.FFMS_VideoSource = null;

var color_space_buffer = std.mem.zeroes([128]u8);
var color_space = &color_space_buffer[0];
var video_color_space: c_int = -1;
var video_color_range: c_int = -1;

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
pub fn LoadVideo(file_name: [*c]u8, color_matrix: [*c]u8) FfmsError!void {
    const indexer = c.FFMS_CreateIndexer(file_name, &err_info);

    if (indexer == null) {
        if (err_info.SubType == c.FFMS_ERROR_FILE_READ) {
            return FfmsError.FileNotFound;
        } else {
            return FfmsError.VideoNotSupported;
        }
    }

    // Get video tracks
    var tracks = GetTracksOfType(indexer, c.FFMS_TYPE_VIDEO) catch |err| {
        return err; // Theoretically this is an OutOfMemory error
    };

    if (tracks.count() <= 0) {
        return FfmsError.NoVideoTracks;
    }

    var track_number: c_int = -1;
    if (tracks.count() > 1) {
        // TODO: Ask which track should be opened!
        track_number = 0;
    }

    // TODO: Cache stuff

    // Index the file
    index = c.FFMS_DoIndexing2(indexer, c.FFMS_IEH_ABORT, &err_info);

    // If no track number has been selected, use the first one
    if (track_number < 0) {
        track_number = c.FFMS_GetFirstIndexedTrackOfType(index, c.FFMS_TYPE_VIDEO, &err_info);

        if (track_number < 0) {
            return FfmsError.NoVideoTracks;
        }
    }

    // TODO: Audio
    // const has_audio = c.FFMS_GetFirstTrackOfType(index, c.FFMS_TYPE_AUDIO, &err_info) != -1;

    // TODO: Add an option for unsafe seeking
    video_source = c.FFMS_CreateVideoSource(file_name, track_number, index, 1, c.FFMS_SEEK_NORMAL, &err_info);

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

    // Frame information
    const track = c.FFMS_GetTrackFromVideo(video_source);
    if (track == null) {
        return FfmsError.GetTrackInfoFailed;
    }
    const time_base = c.FFMS_GetTimeBase(track);
    if (time_base == null) {
        return FfmsError.GetTrackTimeBaseFailed;
    }

    // Get frame times and keyframes
    var time_codes = std.ArrayList(c_int).init(allocator);
    var keyframes = std.ArrayList(c_int).init(allocator);

    var frame_number: c_int = 0;
    while (frame_number < video_info.*.NumFrames) : (frame_number += 1) {
        const frame_info = c.FFMS_GetFrameInfo(track, frame_number);
        if (frame_info == null) {
            return FfmsError.GetFrameInfoFailed;
        }

        if (frame_info.*.KeyFrame != 0) {
            try keyframes.append(frame_number);
        }

        const timestamp = @as(c_int, @intCast(@divTrunc(frame_info.*.PTS * time_base.*.Num, time_base.*.Den)));
        try time_codes.append(timestamp);
    }

    // if (time_codes.items.len < 2) {}
}

// TODO: This thing
fn SetColorSpace(color_matrix: [*c]u8) void {
    _ = color_matrix;
    return;
}

/// Get tracks of the given type
pub fn GetTracksOfType(indexer: ?*c.FFMS_Indexer, track_type: c.FFMS_TrackType) !std.AutoHashMap(c_int, [*c]const u8) {
    var track_list = std.AutoHashMap(c_int, [*c]const u8).init(allocator);
    const track_count = c.FFMS_GetNumTracksI(indexer);

    var i: c_int = 0;
    while (i < track_count) : (i += 1) {
        if (c.FFMS_GetTrackTypeI(indexer, i) == track_type) {
            try track_list.put(i, c.FFMS_GetCodecNameI(indexer, i));
        }
    }
    return track_list;
}
