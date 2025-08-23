// SPDX-License-Identifier: MPL-2.0

const std = @import("std");
const c = @import("c.zig").c;
const ffms = @import("ffms.zig");
const libass = @import("libass.zig");
const frames = @import("frames.zig");
const common = @import("common.zig");

var max_size: c_int = 0;
var total_size: c_int = 0;
var buffers: std.ArrayList(*frames.FrameGroup) = undefined;

pub fn Init(num_buffers: usize, max_cache_mb: c_int, width: usize, height: usize, pitch: usize) ffms.FfmsError!void {
    buffers = std.ArrayList(*frames.FrameGroup).init(common.allocator);
    max_size = max_cache_mb * (1024 ^ 2);

    // Pre-allocate buffers
    var i: usize = 0;
    while (i < num_buffers) : (i += 1) {
        const frame = try AllocateFrame(width, height, pitch);
        try buffers.append(frame);
    }
}

/// Free the buffers
pub fn Deinit() void {
    for (buffers.items) |buffer| {
        common.allocator.destroy(buffer.*.video_frame);
        common.allocator.destroy(buffer.*.subtitle_frame);
        common.allocator.destroy(buffer);
    }
    buffers.deinit();
}

/// Get a frame
pub fn ProcFrame(frame_number: c_int, timestamp: c_longlong, raw: c_int) ffms.FfmsError!*frames.FrameGroup {
    _ = raw; // For sub-less frame

    // TODO: subtitle hash check
    var frame: ?*frames.FrameGroup = null;
    // See if the frame is in the list of cached buffers already
    for (buffers.items, 0..) |buffer, idx| {
        if (buffer.*.video_frame.*.valid == 1 and buffer.*.video_frame.*.frame_number == frame_number) {
            frame = buffer;

            // Move buffer to the front of the list
            if (idx != 0) {
                _ = buffers.swapRemove(idx);
                try buffers.insert(0, buffer);
            }

            return frame.?;
        }
    }

    // The frame was not cached, so we need to find an invalidated buffer
    // (or create a new one if there's space in the cache)

    // If we're at the size limit, make space
    if (total_size >= max_size) {
        const last = buffers.swapRemove(buffers.items.len - 1);
        _ = ReleaseFrame(last);
        try buffers.insert(0, last);
    }

    // Find an invalidated buffer
    for (buffers.items) |buffer| {
        if (buffer.*.video_frame.*.valid == 0) {
            frame = buffer;
            break;
        }
    }

    // Allocate a new buffer
    if (frame == null) {
        // Clone geometry from the first buffer
        const reference = buffers.items[0];
        frame = try AllocateFrame(
            @intCast(reference.*.video_frame.*.width),
            @intCast(reference.*.video_frame.*.height),
            @intCast(reference.*.video_frame.*.pitch),
        );
        try buffers.insert(0, frame.?);
        total_size = total_size + (frame.?.*.video_frame.*.height * frame.?.*.video_frame.*.pitch) * 2; // add size
    }

    try ffms.GetFrame(frame_number, frame.?.*.video_frame);
    try libass.GetFrame(timestamp, frame.?.*.subtitle_frame);

    frame.?.*.video_frame.*.valid = 1;
    frame.?.*.subtitle_frame.valid = 1;
    return frame.?;
}

pub fn ReleaseFrame(frame: *frames.FrameGroup) c_int {
    frame.video_frame.valid = 0;
    frame.subtitle_frame.valid = 0;
    return 0;
}

/// Allocate a new frame buffer
fn AllocateFrame(width: usize, height: usize, pitch: usize) !*frames.FrameGroup {
    const total_bytes = height * pitch;
    const v_pixel_buffer = try common.allocator.alloc(u8, total_bytes);
    const s_pixel_buffer = try common.allocator.alloc(u8, total_bytes);
    const v_frame = try common.allocator.create(frames.VideoFrame);
    const s_frame = try common.allocator.create(frames.SubtitleFrame);
    const group = try common.allocator.create(frames.FrameGroup);

    v_frame.* = .{
        .frame_number = -1,
        .timestamp = 0,
        .width = @intCast(width),
        .height = @intCast(height),
        .pitch = @intCast(pitch),
        .flipped = 0,
        .data = v_pixel_buffer.ptr,
        .valid = 0,
    };

    s_frame.* = .{
        .hash = 0,
        .timestamp = 0,
        .width = @intCast(width),
        .height = @intCast(height),
        .pitch = @intCast(pitch),
        .flipped = 0,
        .data = s_pixel_buffer.ptr,
        .valid = 0,
    };

    group.* = .{
        .video_frame = v_frame,
        .subtitle_frame = s_frame,
    };

    return group;
}
