// SPDX-License-Identifier: MPL-2.0

const std = @import("std");
const common = @import("common.zig");
const frames = @import("frames.zig");
const logger = @import("logger.zig");
const context = @import("context.zig");

pub fn RenderWaveform(g_ctx: *context.GlobalContext, bmp: *frames.Bitmap, start_time: f64, frame_time: f64) void {
    const ctx = &g_ctx.*.visualization;

    const end_time = start_time + @as(f64, @floatFromInt(bmp.*.width)) * @as(f64, ctx.pixel_ms);
    const audio_data = g_ctx.*.buffers.audio_buffer;
    const stereo = g_ctx.*.ffms.channel_count == 2;
    const sr: f32 = @floatFromInt(g_ctx.*.ffms.sample_rate);

    const pixel_samples: usize = @intFromFloat(ctx.pixel_ms * sr / 1000.0);

    if (audio_data) |audio| {
        const effective_length: usize = if (stereo) audio.len / 2 else audio.len;

        const width: usize = @intCast(bmp.*.width);
        const pitch: usize = @intCast(bmp.*.pitch);
        const mid: i16 = @intCast(@divFloor(bmp.*.height, 2));

        // Clear
        @memset(bmp.*.data[0 .. ctx.waveform_height * pitch], 0);

        var current_sample: usize = @intFromFloat(start_time * @as(f64, sr / 1000.0));

        var x: usize = 0;
        while (x < width) : (x += 1) {
            const s0: usize = current_sample;
            const s1: usize = @min(s0 + pixel_samples, effective_length);
            current_sample += pixel_samples;

            if (s0 >= effective_length) {
                break;
            }

            var peak_min: i16 = 0x7fff; // Max i16
            var peak_max: i16 = -0x8000; // Min i16

            // Compute peaks
            var i = s0;
            if (stereo) { // Stereo, need to downmix
                while (i < s1) : (i += 1) {
                    const left = audio[2 * i];
                    const right = audio[2 * i + 1];
                    const mono: i16 = @intCast((@as(i32, left) + @as(i32, right)) >> 1);

                    if (mono > peak_max) peak_max = mono;
                    if (mono < peak_min) peak_min = mono;
                }
            } else { // Mono
                while (i < s1) : (i += 1) {
                    const mono = audio[i];

                    if (mono > peak_max) peak_max = mono;
                    if (mono < peak_min) peak_min = mono;
                }
            }

            // Scale to bitmap
            const mid_f: f32 = @floatFromInt(mid);
            const min_f: f32 = @floatFromInt(peak_min);
            const max_f: f32 = @floatFromInt(peak_max);

            const scaled_min: i16 = @intFromFloat(@max((min_f * ctx.amplitude_scale * mid_f) / 0x8000, -mid_f));
            const scaled_max: i16 = @intFromFloat(@min((max_f * ctx.amplitude_scale * mid_f) / 0x8000, mid_f));

            drawLine(bmp, x, mid - scaled_min, mid - scaled_max, 0xff00ff00); // green
        }

        // Draw frame time
        if (frame_time >= start_time and frame_time <= end_time) {
            const delta = frame_time - start_time;
            const frame_x: usize = @intFromFloat(delta / ctx.pixel_ms);

            drawLine(bmp, frame_x, 0, @intCast(ctx.waveform_height), 0xffff0000); // red
        }
    }
}

fn drawLine(
    bmp: *frames.Bitmap,
    x: usize,
    y1_in: i16,
    y2_in: i16,
    color: u32,
) void {
    const w: usize = @intCast(bmp.width);
    const h: usize = @intCast(bmp.height);
    const pitch: usize = @intCast(bmp.pitch);

    if (x >= w) return;

    var y1: usize = @intCast(y1_in);
    var y2: usize = @intCast(y2_in);

    if (y1 > y2) {
        const tmp = y1;
        y1 = y2;
        y2 = tmp;
    }

    if (y2 < 0 or y1 >= h) return;
    if (y1 < 0) y1 = 0;
    if (y2 >= h) y2 = h - 1;

    const pixel_size = 4; // 32-bit
    var y = y1;

    while (y <= y2) : (y += 1) {
        const row_ptr = bmp.data + (y * pitch);
        const px_ptr: *u32 = @ptrCast(@alignCast(row_ptr + x * pixel_size));
        px_ptr.* = color;
    }
}
