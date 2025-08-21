// SPDX-License-Identifier: MPL-2.0

//! Libass Subtitle Provider

const std = @import("std");
const c = @import("c.zig").c;
const frames = @import("frames.zig");
const common = @import("common.zig");

pub const LibassError = error{
    NotInitialized,
};

// Local variables
var library: ?*c.ASS_Library = null;
var renderer: ?*c.ass_renderer = null;
var track: ?*c.ass_track = null;

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

/// Deinitialize libass
pub fn Deinitialize() void {
    c.ass_renderer_done(renderer);
    c.ass_library_done(library);
    if (track != null) {
        c.ass_free_track(track);
    }
}

/// Set up renderer
pub fn LoadVideo(frame_width: c_int, frame_height: c_int) void {
    if (renderer != null) {
        c.ass_renderer_done(renderer);
        renderer = null;
    }
    
    renderer = c.ass_renderer_init(library);
    c.ass_set_frame_size(renderer, frame_width, frame_height);
    c.ass_set_storage_size(renderer, frame_width, frame_height);
    c.ass_set_font_scale(renderer, 1.0);

    c.ass_set_fonts(renderer, null, "Sans", 1, null, 0);
}
