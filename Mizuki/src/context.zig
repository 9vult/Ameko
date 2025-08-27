// SPDX-License-Identifier: MPL-2.0

//! Contextualization

const std = @import("std");
const c = @import("c.zig").c;
const frames = @import("frames.zig");
const common = @import("common.zig");
const logger = @import("logger.zig");

pub const FFMSContext = struct {
    // Allegedly Private
    index: ?*c.FFMS_Index,
    video_source: ?*c.FFMS_VideoSource,
    audio_source: ?*c.FFMS_AudioSource,
    color_space_buffer: [128]u8,
    color_space: *const u8,
    video_color_space: c_int,
    video_color_range: c_int,

    // Public
    keyframes: []c_int,
    timecodes: []c_longlong,
    frame_intervals: []c_longlong,
    frame_count: c_int,

    frame_width: usize,
    frame_height: usize,
    frame_pitch: usize,

    channel_count: c_int,
    sample_rate: c_int,
    sample_count: i64,

    pub fn Init() FFMSContext {
        const csb: [128]u8 = std.mem.zeroes([128]u8);
        return FFMSContext{
            .index = null,
            .video_source = null,
            .audio_source = null,
            .color_space_buffer = csb,
            .color_space = &csb[0],
            .video_color_space = -1,
            .video_color_range = -1,
            .keyframes = undefined,
            .timecodes = undefined,
            .frame_intervals = undefined,
            .frame_count = undefined,
            .frame_width = 0,
            .frame_height = 0,
            .frame_pitch = 0,
            .channel_count = -1,
            .sample_rate = -1,
            .sample_count = -1,
        };
    }
};

pub const LibassContext = struct {
    renderer: ?*c.ass_renderer,
    track: ?*c.ass_track,
    current_hash: u32,

    pub fn Init() LibassContext {
        return LibassContext{
            .renderer = null,
            .track = null,
            .current_hash = 0,
        };
    }
};

pub const BuffersContext = struct {
    max_size: c_int,
    total_size: c_int,
    buffers: std.ArrayList(*frames.FrameGroup),
    audio_buffer: ?[]f32,
    audio_frame: ?*frames.AudioFrame,

    pub fn Init() BuffersContext {
        return BuffersContext{
            .max_size = 0,
            .total_size = 0,
            .buffers = undefined,
            .audio_buffer = null,
            .audio_frame = null,
        };
    }
};

pub const GlobalContext = struct {
    ffms: FFMSContext,
    libass: LibassContext,
    buffers: BuffersContext,

    pub fn Init() GlobalContext {
        return GlobalContext{
            .ffms = FFMSContext.Init(),
            .libass = LibassContext.Init(),
            .buffers = BuffersContext.Init(),
        };
    }
};

/// Create a context
pub fn CreateContext() !*GlobalContext {
    const context = try common.allocator.create(GlobalContext);
    context.* = GlobalContext.Init();
    return context;
}

/// Destroy a context
pub fn DestroyContext(ctx: *GlobalContext) void {
    common.allocator.destroy(ctx);
}
