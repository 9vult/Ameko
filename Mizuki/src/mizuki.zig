// SPDX-License-Identifier: MPL-2.0

const c = @import("c.zig").c;

const std = @import("std");
const ffms = @import("ffms.zig");
const libass = @import("libass.zig");
const frames = @import("frames.zig");
const common = @import("common.zig");
const errors = @import("errors.zig");
const buffers = @import("buffers.zig");

pub export fn Initialize() void {
    ffms.Initialize();
}

pub export fn LoadVideo(file_name: [*c]u8, cache_file_name: [*c]u8, color_matrix: [*c]u8) c_int {
    ffms.LoadVideo(file_name, cache_file_name, color_matrix) catch |err| {
        return errors.IntFromFfmsError(err);
    };
    return 0;
}

pub export fn CloseVideo() c_int {
    return 0; // TODO: implement
}

/// Allocate frame buffers
pub export fn AllocateBuffers(num_buffers: c_int, max_cache_mb: c_int) c_int {
    buffers.Init(@intCast(num_buffers), max_cache_mb, ffms.frame_width, ffms.frame_height, ffms.frame_pitch) catch |err| {
        return errors.IntFromFfmsError(err);
    };
    return 0;
}

/// Free frame buffers
pub export fn FreeBuffers() c_int {
    buffers.Deinit();
    return 0;
}

/// Get a frame
pub export fn GetFrame(frame_number: c_int) ?*frames.VideoFrame {
    return buffers.ProcFrame(frame_number, 0, 0) catch {
        return null;
    };
}

/// Release a frame
pub export fn ReleaseFrame(frame: *frames.VideoFrame) c_int {
    return buffers.ReleaseFrame(frame);
}

/// Get the number of frames in the video
pub export fn GetFrameCount() c_int {
    return ffms.frame_count;
}

/// Get array of keyframes
pub export fn GetKeyframes() common.IntArray {
    return .{
        .ptr = ffms.keyframes.ptr,
        .len = ffms.keyframes.len,
    };
}

/// Get array of timecodes
pub export fn GetTimecodes() common.LongArray {
    return .{
        .ptr = ffms.timecodes.ptr,
        .len = ffms.timecodes.len,
    };
}

// Get array of frame intervals
pub export fn GetFrameIntervals() common.LongArray {
    return .{
        .ptr = ffms.frame_intervals.ptr,
        .len = ffms.frame_intervals.len,
    };
}

/// Free an int array
///
/// Call this on Deinit
fn FreeIntArray(array: common.IntArray) void {
    const slice = @as([*]c_int, array.ptr)[0..array.len];
    common.allocator.free(slice);
}

/// Free a long array
///
/// Call this on Deinit
fn FreeLongArray(array: common.LongArray) void {
    const slice = @as([*]c_longlong, array.ptr)[0..array.len];
    common.allocator.free(slice);
}

pub fn main() !void {
    const ffms_version = ffms.GetVersion();
    std.debug.print("FFMS2 Version: {x}.{x}.{x}\n", .{
        ffms_version.major,
        ffms_version.minor,
        ffms_version.patch,
    });

    libass.Initialize();
    const libass_version = libass.GetVersion();
    std.debug.print("Libass Version: {x}.{x}.{x}\n", .{
        libass_version.major,
        libass_version.minor,
        libass_version.patch,
    });
}
