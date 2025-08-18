// SPDX-License-Identifier: MPL-2.0

//! Libass Subtitle Provider

const std = @import("std");
const c = @import("c.zig").c;
const frames = @import("frames.zig");
const common = @import("common.zig");

// Local variables
var library: ?*c.ASS_Library = null;

/// Get the current libass version
///
/// Requires libass to be initialized.
pub fn GetVersion() common.BackingVersion {
    const version = c.ass_library_version(); // hex
    return .{
        .major = version >> 28,
        .minor = (version >> 20) & 0xFF,
        .patch = (version >> 12) & 0xFF,
    };
}

/// Initialize libass
pub fn Initialize() void {
    library = c.ass_library_init();
}
