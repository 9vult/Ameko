// SPDX-License-Identifier: MPL-2.0

const std = @import("std");
const c = @import("c.zig").c;
const ffms = @import("ffms.zig");
const libass = @import("libass.zig");
const frames = @import("frames.zig");
const common = @import("common.zig");
const context = @import("context.zig");

/// Pre-initialize video buffers
pub fn Init(g_ctx: *context.GlobalContext, num_buffers: usize, max_cache_mb: c_int, width: usize, height: usize, pitch: usize) ffms.FfmsError!void {
    var ctx = &g_ctx.*.buffers;
    ctx.buffers = std.ArrayList(*frames.FrameGroup).init(common.allocator);
    ctx.max_size = max_cache_mb * (1024 ^ 2);

    // Pre-allocate buffers
    var i: usize = 0;
    while (i < num_buffers) : (i += 1) {
        const frame = try AllocateFrame(width, height, pitch);
        try ctx.buffers.append(frame);
    }
}

/// Initialize audio buffer
pub fn InitAudio(g_ctx: *context.GlobalContext) ffms.FfmsError!void {
    var ctx = &g_ctx.*.buffers;
    const ffms_ctx = &g_ctx.*.ffms;

    const total_samples: usize = @intCast(ffms_ctx.sample_count * ffms_ctx.channel_count);
    ctx.audio_buffer = try common.allocator.alloc(i16, total_samples);
    ctx.audio_frame = try common.allocator.create(frames.AudioFrame);
    ctx.audio_frame.?.* = .{
        .data = ctx.audio_buffer.?.ptr,
        .length = @intCast(ctx.audio_buffer.?.len),
        .channel_count = ffms_ctx.channel_count,
        .sample_count = ffms_ctx.sample_count,
        .sample_rate = ffms_ctx.sample_rate,
        .duration_ms = @divFloor((ffms_ctx.sample_count * 1000), ffms_ctx.sample_rate),
        .valid = 0,
    };
}

/// Get the audio buffer
pub fn GetAudio(g_ctx: *context.GlobalContext) ffms.FfmsError!*frames.AudioFrame {
    const ctx = &g_ctx.*.buffers;
    const ffms_ctx = &g_ctx.*.ffms;

    if (ctx.audio_frame == null) {
        return ffms.FfmsError.NoAudioTracks;
    }
    if (ctx.audio_frame.?.*.valid == 0) {
        try ffms.GetAudio(g_ctx, @ptrCast(ctx.audio_buffer.?.ptr), 0, ffms_ctx.sample_count);
        ctx.audio_frame.?.*.valid = 1;
    }
    return ctx.audio_frame.?;
}

/// Free the buffers
pub fn Deinit(g_ctx: *context.GlobalContext) void {
    var ctx = &g_ctx.*.buffers;
    for (ctx.buffers.items) |buffer| {
        common.allocator.destroy(buffer.*.video_frame);
        common.allocator.destroy(buffer.*.subtitle_frame);
        common.allocator.destroy(buffer);
    }
    ctx.buffers.deinit();
    common.allocator.destroy(ctx.audio_frame.?);
}

/// Get a frame
pub fn ProcFrame(g_ctx: *context.GlobalContext, frame_number: c_int, timestamp: c_longlong, raw: c_int) ffms.FfmsError!*frames.FrameGroup {
    var ctx = &g_ctx.*.buffers;
    var buffers = ctx.buffers;

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
            if (!libass.VerifyHash(g_ctx, result.?.*.subtitle_frame)) {
                try libass.GetFrame(g_ctx, timestamp, result.?.*.subtitle_frame);
            }

            return result.?;
        }
    }

    // The frame was not cached, so we need to find an invalidated buffer
    // (or create a new one if there's space in the cache)

    // If we're at the size limit, make space
    if (ctx.total_size >= ctx.max_size) {
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
        ctx.total_size += (result.?.*.video_frame.*.height * result.?.*.video_frame.*.pitch) * 2; // add size
    }

    try ffms.GetFrame(g_ctx, frame_number, result.?.*.video_frame);
    try libass.GetFrame(g_ctx, timestamp, result.?.*.subtitle_frame);

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
