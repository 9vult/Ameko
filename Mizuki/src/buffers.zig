// SPDX-License-Identifier: MPL-2.0

const std = @import("std");
const c = @import("c.zig").c;
const ffms = @import("ffms.zig");
const frames = @import("frames.zig");
const common = @import("common.zig");

const max_size = 1024 ^ 3; // 1024 mb
var total_size: c_int = 0;
var buffers: std.ArrayList(*frames.VideoFrame) = undefined;

// TODO: FrameGroup instead of VideoFrame
pub fn Init(num_buffers: usize, width: usize, height: usize, pitch: usize) ffms.FfmsError!void {
    buffers = std.ArrayList(*frames.VideoFrame).init(common.allocator);

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
        common.allocator.destroy(buffer);
    }
    buffers.deinit();
}

/// Get a frame
pub fn ProcFrame(frame_number: c_int, timestamp: c_longlong, raw: c_int) ffms.FfmsError!*frames.VideoFrame {
    _ = raw; // For sub-less frame
    _ = timestamp;

    var frame: ?*frames.VideoFrame = null;
    // See if the frame is in the list of cached buffers already
    for (buffers.items, 0..) |buffer, idx| {
        if (buffer.*.valid == 1 and buffer.*.frame_number == frame_number) {
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
        if (buffer.*.valid == 0) {
            frame = buffer;
            break;
        }
    }

    // Allocate a new buffer
    if (frame == null) {
        // Clone geometry from the first buffer
        const reference = buffers.items[0];
        frame = try AllocateFrame(
            @intCast(reference.*.width),
            @intCast(reference.*.height),
            @intCast(reference.*.pitch),
        );
        try buffers.insert(0, frame.?);
        total_size = total_size + frame.?.*.height * frame.?.*.pitch; // add size
    }

    try ffms.GetFrame(frame_number, frame.?);

    frame.?.*.valid = 1;
    return frame.?;
    // TODO: subtitles
}

pub fn ReleaseFrame(frame: *frames.VideoFrame) c_int {
    frame.valid = 0;
    return 0;
}

/// Allocate a new frame buffer
fn AllocateFrame(width: usize, height: usize, pitch: usize) !*frames.VideoFrame {
    const total_bytes = height * pitch;
    const pixel_buffer = try common.allocator.alloc(u8, total_bytes);
    const frame = try common.allocator.create(frames.VideoFrame);

    frame.* = .{
        .frame_number = -1,
        .timestamp = 0,
        .width = @intCast(width),
        .height = @intCast(height),
        .pitch = @intCast(pitch),
        .flipped = 0,
        .data = pixel_buffer.ptr,
        .valid = 0,
    };
    return frame;
}
