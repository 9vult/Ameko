// SPDX-License-Identifier: MPL-2.0

const c = @import("c.zig").c;

const std = @import("std");
const ffms = @import("ffms.zig");
const frames = @import("frames.zig");
const common = @import("common.zig");
const errors = @import("errors.zig");
const buffers = @import("buffers.zig");

pub export fn Initialize() void {
    ffms.Initialize();
}

pub export fn TestGetFfmsVersion() c_int {
    return c.FFMS_GetVersion();
}

pub export fn LoadVideo(file_name: [*c]u8, cache_file_name: [*c]u8, color_matrix: [*c]u8) c_int {
    ffms.LoadVideo(file_name, cache_file_name, color_matrix) catch |err| {
        return errors.IntFromFfmsError(err);
    };
    return 0;
}

/// Get array of keyframes
pub export fn GetKeyframes() common.IntArray {
    const buffer = ffms.keyframes.toOwnedSlice() catch unreachable;
    return .{
        .ptr = buffer.ptr,
        .len = buffer.len,
    };
}

/// Free an int array
///
/// Should be called after retrieving an int array
pub export fn FreeIntArray(array: common.IntArray) void {
    const slice = @as([*]c_int, array.ptr)[0..array.len];
    common.allocator.free(slice);
}

/// Get array of timecodes
pub export fn GetTimecodes() common.IntArray {
    const buffer = ffms.time_codes.toOwnedSlice() catch unreachable;
    return .{
        .ptr = buffer.ptr,
        .len = buffer.len,
    };
}

/// Allocate a frame
pub export fn AllocateBuffers(num_buffers: c_int) c_int {
    buffers.Init(@intCast(num_buffers), ffms.frame_width, ffms.frame_height, ffms.frame_pitch) catch |err| {
        return errors.IntFromFfmsError(err);
    };
    return 0;
}

/// Get a frame
pub export fn GetFrame(frame_number: c_int, out: *frames.VideoFrame) c_int {
    ffms.GetFrame(frame_number, out) catch |err| {
        return errors.IntFromFfmsError(err);
    };
    return 0;
}

pub fn main() !void {
    const ffms_version = ffms.GetVersion();
    std.debug.print("FFMS2 Version: {x}.{x}.{x}\n", .{
        ffms_version.major,
        ffms_version.minor,
        ffms_version.patch,
    });
}
