// SPDX-License-Identifier: MPL-2.0

const c = @import("c.zig").c;

const std = @import("std");
const ffms = @import("ffms.zig");
const common = @import("common.zig");

pub export fn initialize() void {
    ffms.Initialize();
}

pub export fn LoadVideo(file_name: [*c]u8, color_matrix: [*c]u8) c_int {
    ffms.LoadVideo(file_name, color_matrix) catch {
        return 0;
    };
    return 1;
}

pub fn main() !void {
    const ffms_version = ffms.GetVersion();
    std.debug.print("FFMS2 Version: {x}.{x}.{x}\n", .{
        ffms_version.major,
        ffms_version.minor,
        ffms_version.patch,
    });
}
