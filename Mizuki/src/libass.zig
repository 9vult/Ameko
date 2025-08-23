// SPDX-License-Identifier: MPL-2.0

//! Libass Subtitle Provider

const std = @import("std");
const c = @import("c.zig").c;
const fnv = @import("fnv.zig");
const frames = @import("frames.zig");
const common = @import("common.zig");
const logger = @import("logger.zig");

pub const LibassError = error{
    NotInitialized,
};

// Local variables
var library: ?*c.ASS_Library = null;
var renderer: ?*c.ass_renderer = null;
var track: ?*c.ass_track = null;
var current_hash: u32 = 0;

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
    // c.ass_set_message_cb(library, &AssLogCallback, null); // TODO: When va_args is supported
}

/// Deinitialize libass
pub fn Deinitialize() void {
    c.ass_renderer_done(renderer);
    c.ass_library_done(library);
    if (track != null) {
        c.ass_free_track(track);
        track = null;
    }
}

/// Load subtitles in from memory
pub fn SetSubtitles(data: [*c]u8, data_len: c_int, code_page: [*c]u8) void {
    if (track != null) {
        c.ass_free_track(track);
        track = null;
    }
    track = c.ass_read_memory(library, data, @intCast(data_len), code_page);
    current_hash = fnv.fnv1a_32(data, data_len);
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

/// Verify a frame's hash
pub fn VerifyHash(frame: ?*frames.SubtitleFrame) bool {
    return current_hash != 0 and current_hash == frame.?.*.hash;
}

/// Get the subtitles (On a transparent frame)
pub fn GetFrame(timestamp: c_longlong, out: *frames.SubtitleFrame) !void {
    const frame_w: i32 = @intCast(out.*.width);
    const frame_h: i32 = @intCast(out.*.height);
    const buffer_size: usize = @intCast(out.*.pitch * out.*.height);

    // Set the frame's hash
    out.*.hash = current_hash;

    // Reset buffer to transparent
    const buf = out.data[0..buffer_size];
    @memset(buf, 0);

    if (track == null) {
        return;
    }

    c.ass_set_frame_size(renderer, out.*.width, out.*.height);
    c.ass_set_storage_size(renderer, out.*.width, out.*.height);

    const detect_change = 0;
    var img: ?*c.ASS_Image = c.ass_render_frame(renderer, track, timestamp, detect_change);

    while (img) |i| {
        const o: u32 = (255 - (i.*.color & 0xFF)); // opacity
        const r: u32 = @intCast(i.*.color >> 24);
        const g: u32 = @intCast((i.*.color >> 16) & 0xFF);
        const b: u32 = @intCast((i.*.color >> 8) & 0xFF);

        var y: i32 = 0;
        while (y < i.*.h) : (y += 1) {
            var x: i32 = 0;
            while (x < i.*.w) : (x += 1) {
                const src_index: usize = @intCast(y * i.*.stride + x);
                const dest_x: i32 = x + i.*.dst_x;
                const dest_y: i32 = y + i.*.dst_y;

                if (dest_x >= 0 and dest_x < frame_w and dest_y >= 0 and dest_y < frame_h) {
                    const dest_index: usize = @intCast((dest_y * frame_w + dest_x) * 4);

                    const src_alpha: u8 = i.*.bitmap[src_index];
                    const k: u32 = src_alpha * o / 255;

                    buf[dest_index + 0] = @intCast((k * b + (255 - k) * buf[dest_index + 0]) / 255);
                    buf[dest_index + 1] = @intCast((k * g + (255 - k) * buf[dest_index + 1]) / 255);
                    buf[dest_index + 2] = @intCast((k * r + (255 - k) * buf[dest_index + 2]) / 255);
                    buf[dest_index + 3] = @intCast(k + (255 - k) * buf[dest_index + 3] / 255);
                }
            }
        }

        img = i.*.next;
    }
}

/// Callback for handling logs emitted by libass
///
/// Currently unused since Zig doesn't support va_list yet
pub export fn AssLogCallback(
    level: c_int,
    fmt: [*c]const u8,
    args: [*c]u8,
    data: ?*anyopaque,
) callconv(.c) void {
    _ = args;
    _ = data;
    if (level < 5) {
        // Note: Would need to work with the va_list
        // but it's not implemented yet(?)
        // https://github.com/ziglang/zig/blob/0d0f09fb0ee60b5fa42f51732bde2a4db43453a8/test/behavior/var_args.zig#L234
        const fmt_slice = std.mem.sliceTo(fmt, 0);
        logger.Info(fmt_slice);
    }
}
