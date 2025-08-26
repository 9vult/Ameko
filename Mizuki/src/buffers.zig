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
var audio_buffer: ?[]f32 = null;
var audio_frame: ?*frames.AudioFrame = null;

/// Pre-initialize video buffers
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

/// Initialize audio buffer
pub fn InitAudio() ffms.FfmsError!void {
    const total_samples: usize = @intCast(ffms.sample_count * ffms.channel_count);
    audio_buffer = try common.allocator.alloc(f32, total_samples);
    audio_frame = try common.allocator.create(frames.AudioFrame);
    audio_frame.?.* = .{
        .data = audio_buffer.?.ptr,
        .length = @intCast(audio_buffer.?.len),
        .channel_count = ffms.sample_rate,
        .sample_count = ffms.sample_count,
        .sample_rate = ffms.sample_rate,
        .valid = 0,
    };
}

/// Get the audio buffer
pub fn GetAudio() ffms.FfmsError!*frames.AudioFrame {
    if (audio_frame == null) {
        return ffms.FfmsError.NoAudioTracks;
    }
    if (audio_frame.?.*.valid == 0) {
        try ffms.GetAudio(@ptrCast(audio_buffer.?.ptr), 0, ffms.sample_count);
        audio_frame.?.*.valid = 1;
    }
    return audio_frame.?;
}

/// Free the buffers
pub fn Deinit() void {
    for (buffers.items) |buffer| {
        common.allocator.destroy(buffer.*.video_frame);
        common.allocator.destroy(buffer.*.subtitle_frame);
        common.allocator.destroy(buffer);
    }
    buffers.deinit();
    common.allocator.destroy(audio_frame.?);
}

/// Get a frame
pub fn ProcFrame(frame_number: c_int, timestamp: c_longlong, raw: c_int) ffms.FfmsError!*frames.FrameGroup {
    _ = raw; // For sub-less frame
    var result: ?*frames.FrameGroup = null;

    // See if the frame is in the list of cached buffers already
    for (buffers.items, 0..) |buffer, idx| {
        if (buffer.*.video_frame.*.valid == 1 and buffer.*.video_frame.*.frame_number == frame_number) {
            result = buffer;

            // Move buffer to the front of the list
            if (idx != 0) {
                _ = buffers.swapRemove(idx);
                try buffers.insert(0, buffer);
            }

            // Check if we need to (re)render the subtitles
            if (!libass.VerifyHash(result.?.*.subtitle_frame)) {
                try libass.GetFrame(timestamp, result.?.*.subtitle_frame);
            }

            return result.?;
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
            result = buffer;
            break;
        }
    }

    // Allocate a new buffer
    if (result == null) {
        // Clone geometry from the first buffer
        const reference = buffers.items[0];
        result = try AllocateFrame(
            @intCast(reference.*.video_frame.*.width),
            @intCast(reference.*.video_frame.*.height),
            @intCast(reference.*.video_frame.*.pitch),
        );
        try buffers.insert(0, result.?);
        total_size = total_size + (result.?.*.video_frame.*.height * result.?.*.video_frame.*.pitch) * 2; // add size
    }

    try ffms.GetFrame(frame_number, result.?.*.video_frame);
    try libass.GetFrame(timestamp, result.?.*.subtitle_frame);

    result.?.*.video_frame.*.valid = 1;
    result.?.*.subtitle_frame.valid = 1;
    return result.?;
}

/// Mark a frame as invalid so it can be reused
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
