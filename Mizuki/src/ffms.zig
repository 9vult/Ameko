// SPDX-License-Identifier: MPL-2.0

//! FFMS2 Interop

const std = @import("std");
const c = @import("c.zig").c;
const common = @import("common.zig");

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
