// SPDX-License-Identifier: MPL-2.0

const std = @import("std");
const c = @import("c.zig").c;
const ffms = @import("ffms.zig");
const frames = @import("frames.zig");
const common = @import("common.zig");

var buffers: std.ArrayList(*frames.VideoFrame) = undefined;

// TODO: FrameGroup instead of VideoFrame
pub fn Init(num_buffers: usize, width: usize, height: usize, pitch: usize) ffms.FfmsError!void {
    buffers = std.ArrayList(*frames.VideoFrame).init(common.allocator);

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
pub fn ProcFrame(frame_number: c_int, timestamp: c_longlong, raw: c_int, out: *frames.VideoFrame) ffms.FfmsError!void {
    _ = raw; // For sub-less frame
    _ = timestamp;

    // Find an invalid buffer or create one if needed
    var frame: *frames.VideoFrame = undefined;
    for (buffers.items) |buffer| {
        if (buffer.*.valid == 0) {
            frame = buffer;
            break;
        }
    }

    if (frame == undefined) {
        const reference = buffers.items[0];
        frame = AllocateFrame(reference.*.width, reference.*.height, reference.*.pitch);
        try buffers.append(frame);
    }

    try ffms.GetFrame(frame_number, frame);

    out.* = frame.*;
    // TODO: subtitles
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
