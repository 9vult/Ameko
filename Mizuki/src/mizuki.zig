// SPDX-License-Identifier: MPL-2.0

const c = @import("c.zig").c;

const std = @import("std");
const ffms = @import("ffms.zig");
const libass = @import("libass.zig");
const frames = @import("frames.zig");
const common = @import("common.zig");
const errors = @import("errors.zig");
const logger = @import("logger.zig");
const buffers = @import("buffers.zig");
const context = @import("context.zig");

var is_initialized = false;

/// Initialize the library
pub export fn Initialize() void {
    if (is_initialized) {
        return;
    }
    logger.Debug("Initializing Mizuki...");

    ffms.Initialize();
    libass.Initialize();

    is_initialized = true;
    logger.Debug("Done!");
}

/// Create a context
pub export fn CreateContext() ?*context.GlobalContext {
    const g_ctx = context.CreateContext() catch {
        return null;
    };
    return g_ctx;
}

/// Destroy a context
pub export fn DestroyContext(g_ctx: *context.GlobalContext) void {
    context.DestroyContext(g_ctx);
}

/// Open a video file
pub export fn LoadVideo(g_ctx: *context.GlobalContext, file_name: [*c]u8, cache_file_name: [*c]u8, color_matrix: [*c]u8) c_int {
    const ffms_ctx = g_ctx.*.ffms;
    ffms.LoadVideo(g_ctx, file_name, cache_file_name, color_matrix) catch |err| {
        return errors.IntFromFfmsError(err);
    };
    libass.LoadVideo(g_ctx, @intCast(ffms_ctx.frame_width), @intCast(ffms_ctx.frame_height));
    return 0;
}

/// Close the open video file
pub export fn CloseVideo(g_ctx: *context.GlobalContext) c_int {
    _ = g_ctx;
    return 0; // TODO: implement
}

pub export fn SetSubtitles(g_ctx: *context.GlobalContext, data: [*c]u8, data_len: c_int, code_page: [*c]u8) c_int {
    libass.SetSubtitles(g_ctx, data, data_len, code_page);
    return 0;
}

/// Allocate frame buffers
pub export fn AllocateBuffers(g_ctx: *context.GlobalContext, num_buffers: c_int, max_cache_mb: c_int) c_int {
    const ffms_ctx = g_ctx.*.ffms;
    buffers.Init(g_ctx, @intCast(num_buffers), max_cache_mb, ffms_ctx.frame_width, ffms_ctx.frame_height, ffms_ctx.frame_pitch) catch |err| {
        return errors.IntFromFfmsError(err);
    };
    return 0;
}

pub export fn AllocateAudioBuffer(g_ctx: *context.GlobalContext) c_int {
    buffers.InitAudio(g_ctx) catch |err| {
        return errors.IntFromFfmsError(err);
    };
    return 0;
}

/// Free frame buffers
pub export fn FreeBuffers(g_ctx: *context.GlobalContext) c_int {
    buffers.Deinit(g_ctx);
    return 0;
}

/// Get a frame
pub export fn GetFrame(g_ctx: *context.GlobalContext, frame_number: c_int, timestamp: c_longlong, raw: c_int) ?*frames.FrameGroup {
    return buffers.ProcFrame(g_ctx, frame_number, timestamp, raw) catch {
        return null;
    };
}

/// Get the audio
pub export fn GetAudio(g_ctx: *context.GlobalContext) ?*frames.AudioFrame {
    return buffers.GetAudio(g_ctx) catch {
        return null;
    };
}

/// Get the number of frames in the video
pub export fn GetFrameCount(g_ctx: *context.GlobalContext) c_int {
    return g_ctx.*.ffms.frame_count;
}

/// Get array of keyframes
pub export fn GetKeyframes(g_ctx: *context.GlobalContext) common.IntArray {
    return .{
        .ptr = g_ctx.*.ffms.keyframes.ptr,
        .len = g_ctx.*.ffms.keyframes.len,
    };
}

/// Get array of timecodes
pub export fn GetTimecodes(g_ctx: *context.GlobalContext) common.LongArray {
    return .{
        .ptr = g_ctx.*.ffms.timecodes.ptr,
        .len = g_ctx.*.ffms.timecodes.len,
    };
}

/// Get array of frame intervals
pub export fn GetFrameIntervals(g_ctx: *context.GlobalContext) common.LongArray {
    return .{
        .ptr = g_ctx.*.ffms.frame_intervals.ptr,
        .len = g_ctx.*.ffms.frame_intervals.len,
    };
}

/// Set the logging callback
pub export fn SetLoggerCallback(callback: logger.LogCallback) void {
    logger.SetCallback(callback);
}

/// Free an int array
///
/// Call this on Deinit
fn FreeIntArray(array: common.IntArray) void {
    const slice = @as([*]c_int, array.ptr)[0..array.len];
    common.allocator.free(slice);
}

/// Free a long array
///
/// Call this on Deinit
fn FreeLongArray(array: common.LongArray) void {
    const slice = @as([*]c_longlong, array.ptr)[0..array.len];
    common.allocator.free(slice);
}

pub fn main() !void {
    const ver = c.avcodec_version();
    std.debug.print("Avcodec version: {}.{}.{}\n", .{ (ver >> 16) & 0xFF, (ver >> 8) & 0xFF, ver & 0xFF });
    // const ffms_version = ffms.GetVersion();
    // std.debug.print("FFMS2 Version: {x}.{x}.{x}\n", .{
    //     ffms_version.major,
    //     ffms_version.minor,
    //     ffms_version.patch,
    // });

    // libass.Initialize();
    // const libass_version = libass.GetVersion();
    // std.debug.print("Libass Version: {x}.{x}.{x}\n", .{
    //     libass_version.major,
    //     libass_version.minor,
    //     libass_version.patch,
    // });
    //
    //

    var fmt_ctx: ?*c.AVFormatContext = null;

    if (c.avformat_open_input(&fmt_ctx, "input.mkv", null, null) < 0) {
        return error.OpenFailed;
    }
    defer c.avformat_close_input(&fmt_ctx);

    if (c.avformat_find_stream_info(fmt_ctx, null) < 0) {
        return error.StreamInfoFailed;
    }

    const nb_streams = fmt_ctx.?.nb_streams;
    for (0..@intCast(nb_streams)) |i| {
        const stream = fmt_ctx.?.streams[i];
        if (stream.*.codecpar.*.codec_type == c.AVMEDIA_TYPE_AUDIO) {
            const lang = c.av_dict_get(stream.*.metadata, "language", null, 0);
            const title = c.av_dict_get(stream.*.metadata, "title", null, 0);

            const lang_str = if (lang != null) std.mem.span(lang.*.value) else "unknown";
            const title_str = if (title != null) std.mem.span(title.*.value) else "unknown";

            std.debug.print("Audio stream {d}: language={s} title={s}\n", .{ i, lang_str, title_str });
        }
    }
}
