// SPDX-License-Identifier: MPL-2.0

const c = @import("c.zig").c;

const std = @import("std");
const ffms = @import("ffms.zig");
const common = @import("common.zig");

pub fn main() !void {
    const ffms_version = ffms.GetVersion();
    std.debug.print("FFMS2 Version: {x}.{x}.{x}\n", .{
        ffms_version.major,
        ffms_version.minor,
        ffms_version.patch,
    });
}
