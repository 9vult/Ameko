// SPDX-License-Identifier: MPL-2.0

const c = @import("c.zig").c;

const std = @import("std");
const ffms = @import("ffms.zig");
const common = @import("common.zig");

pub export fn Initialize() void {
    ffms.Initialize();
}

pub export fn TestGetFfmsVersion() c_int {
    return c.FFMS_GetVersion();
}

pub export fn LoadVideo(file_name: [*c]u8, color_matrix: [*c]u8) c_int {
    ffms.LoadVideo(file_name, color_matrix) catch {
        return 0;
    };
    return 1;
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

pub fn main() !void {
    const ffms_version = ffms.GetVersion();
    std.debug.print("FFMS2 Version: {x}.{x}.{x}\n", .{
        ffms_version.major,
        ffms_version.minor,
        ffms_version.patch,
    });
}
