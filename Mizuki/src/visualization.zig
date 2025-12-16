// SPDX-License-Identifier: MPL-2.0

const std = @import("std");
const common = @import("common.zig");
const frames = @import("frames.zig");
const logger = @import("logger.zig");
const context = @import("context.zig");

// Colors
const color_waveform: u32 = 0xff00ff00;
const color_playhead: u32 = 0xffff0000;
const color_qseconds: u32 = 0xffd0d0d0;
const color_seconds: u32 = 0xfff85797;
const color_event: u32 = 0xff937df8;
const color_kf: u32 = 0xffe1e1e1;

/// Render a waveform representation of the audio
pub fn RenderWaveform(
    g_ctx: *context.GlobalContext,
    bmp: *frames.Bitmap,
    pixels_per_ms: f64,
    amplitude_scale: f64,
    start_time: f64,
    frame_time: f64,
    event_bounds: [*]i64,
    event_bounds_len: usize,
) void {
    const audio_data = g_ctx.*.buffers.audio_buffer;
    const stereo = g_ctx.*.ffms.channel_count == 2;
    const sr: f64 = @floatFromInt(g_ctx.*.ffms.sample_rate);

    const pixel_samples: usize = @intFromFloat(pixels_per_ms * sr / 1000.0);

    const bmp_width_u: usize = @intCast(bmp.*.width);
    const bmp_height_u: usize = @intCast(bmp.*.height);
    const bmp_pitch_u: usize = @intCast(bmp.*.pitch);
    const bmp_width_f: f64 = @floatFromInt(bmp.*.width);
    const bmp_height: i32 = @intCast(bmp_height_u);
    const bmp_mid: i32 = @divFloor(bmp_height, 2);
    const wfv_height: i32 = @divFloor(bmp_height * 9, 10); // 90% height
    const wvf_mid: i32 = @divFloor(wfv_height, 2);
    const gutter_height: i32 = @divFloor(bmp_height, 20); // 5% height
    const gutter_half: i32 = @divFloor(gutter_height, 2);

    if (bmp_height_u < 1 or bmp_width_u < 1 or bmp_pitch_u < 1) return;

    if (audio_data) |audio| {
        const effective_length: usize = if (stereo) audio.len / 2 else audio.len;

        const visible_ms: f64 = @as(f64, @floatFromInt(bmp_width_u)) * pixels_per_ms;
        const duration_ms: f64 = (@as(f64, @floatFromInt(effective_length)) * 1000.0) / sr;

        // Calculate start and end times
        var start = start_time;

        if (duration_ms > visible_ms) {
            const max_start = duration_ms - visible_ms;
            if (start > max_start) {
                start = max_start;
            }
        } else {
            start = 0; // Audio is shorter than the viewport
        }
        const end = start + visible_ms;

        // Clear
        const bmp_total_bytes = bmp_height_u * bmp_pitch_u;
        @memset(bmp.*.data[0..bmp_total_bytes], 0);

        // Draw second hashes in the gutter
        const pixels_per_sec: f64 = 1000.0 / pixels_per_ms;
        if (pixels_per_sec >= 5.0) {
            var t = std.math.ceil(start / 1000.0) * 1000.0;
            while (t <= end) : (t += 1000.0) {
                const delta = t - start;
                const frame_x_f: f64 = delta / pixels_per_ms;
                if (frame_x_f >= 0 and frame_x_f < bmp_width_f) {
                    const frame_x: usize = @intFromFloat(frame_x_f);
                    drawLine(bmp, frame_x, 0, gutter_height, color_seconds);
                    drawLine(bmp, frame_x, bmp_height - gutter_height, bmp_height, color_seconds);
                }
            }
        }
        // Draw quarter-second hashes at half-gutter-height
        if (pixels_per_sec >= 20.0) {
            var t = std.math.ceil(start / 250.0) * 250.0;
            while (t <= end) : (t += 250.0) {
                if (@rem(t, 1000) == 0) continue; // Skip whole seconds
                const delta = t - start;
                const frame_x_f: f64 = delta / pixels_per_ms;
                if (frame_x_f >= 0 and frame_x_f < bmp_width_f) {
                    const frame_x: usize = @intFromFloat(frame_x_f);
                    drawLine(bmp, frame_x, 0, gutter_half, color_qseconds);
                    drawLine(bmp, frame_x, bmp_height - gutter_half, bmp_height, color_qseconds);
                }
            }
        }

        // Draw keyframes behind the spectrum
        for (g_ctx.*.ffms.kf_timecodes.?) |kf| {
            const kf_ms: f64 = @floatFromInt(kf);
            if (kf_ms >= start and kf_ms <= end) {
                const delta = kf_ms - start;
                const frame_x_f: f64 = delta / pixels_per_ms;
                if (frame_x_f >= 0 and frame_x_f < bmp_width_f) {
                    const frame_x: usize = @intFromFloat(frame_x_f);
                    drawLine(bmp, frame_x, 0, @intCast(bmp_height_u), color_kf);
                }
            }
        }

        // Draw the waveform
        var current_sample: usize = @intFromFloat(start * sr / 1000.0);

        for (0..bmp_width_u) |x| {
            const s0: usize = current_sample;
            const s1: usize = @min(s0 + pixel_samples, effective_length);
            current_sample += pixel_samples;

            if (s0 >= effective_length) {
                break;
            }

            var peak_min: i32 = 0x7fff; // Max
            var peak_max: i32 = -0x8000; // Min

            // Compute peaks
            var i = s0;
            if (stereo) { // Stereo, need to downmix
                while (i < s1) : (i += 1) {
                    const left = audio[2 * i];
                    const right = audio[2 * i + 1];
                    const mono: i32 = (@as(i32, left) + @as(i32, right)) >> 1;

                    if (mono > peak_max) peak_max = mono;
                    if (mono < peak_min) peak_min = mono;
                }
            } else { // Mono
                while (i < s1) : (i += 1) {
                    const mono = @as(i32, audio[i]);

                    if (mono > peak_max) peak_max = mono;
                    if (mono < peak_min) peak_min = mono;
                }
            }

            // Scale to bitmap
            const mid_f: f64 = @floatFromInt(wvf_mid);
            const min_f: f64 = @floatFromInt(peak_min);
            const max_f: f64 = @floatFromInt(peak_max);

            const scaled_min: i32 = @intFromFloat(@max((min_f * amplitude_scale * mid_f) / 0x8000, -mid_f));
            const scaled_max: i32 = @intFromFloat(@min((max_f * amplitude_scale * mid_f) / 0x8000, mid_f));

            drawLine(bmp, x, bmp_mid - scaled_min, bmp_mid - scaled_max, color_waveform);
        }

        // Draw event bounds over the spectrum
        var ei: usize = 0;
        while (ei < event_bounds_len) : (ei += 2) {
            const evt_start: f64 = @floatFromInt(event_bounds[ei]);
            const evt_end: f64 = @floatFromInt(event_bounds[ei + 1]);

            if ((evt_start < start and evt_end < start) or (evt_start > end and evt_end > end))
                continue;

            const start_x: isize = @intFromFloat((evt_start - start) / pixels_per_ms);
            const end_x: isize = @intFromFloat((evt_end - start) / pixels_per_ms);

            // Draw posts
            if (start_x >= 0) {
                drawLine(bmp, @intCast(start_x), 0, bmp_height, color_event);
            }

            if (evt_start == evt_end) continue; // Stop here if 0-duration

            if (end_x < bmp_width_u) {
                drawLine(bmp, @intCast(end_x), 0, bmp_height, color_event);
            }

            // Draw border
            for (@max(0, start_x)..@min(bmp_width_u, @as(usize, @intCast(end_x)))) |x| {
                drawLine(bmp, x, 0, 1, color_event);
                drawLine(bmp, x, bmp_height - 1, bmp_height, color_event);
            }
        }

        // Draw playhead over everything
        if (frame_time >= start and frame_time <= end) {
            const delta = frame_time - start;
            const frame_x_f: f64 = delta / pixels_per_ms;
            if (frame_x_f >= 0 and frame_x_f < bmp_width_f) {
                const frame_x: usize = @intFromFloat(frame_x_f);
                drawLine(bmp, frame_x, 0, bmp_height, color_playhead);
            }
        }
    }
}

fn drawLine(
    bmp: *frames.Bitmap,
    x: usize,
    y1_in: i32,
    y2_in: i32,
    color: u32,
) void {
    const w: usize = @intCast(bmp.width);
    const h: i32 = @intCast(bmp.height);
    const pitch: usize = @intCast(bmp.pitch);

    if (x >= w) return;

    var y1 = y1_in;
    var y2 = y2_in;

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
        const row_ptr = bmp.data + (@as(usize, @intCast(y)) * pitch);
        const px_ptr: *u32 = @ptrCast(@alignCast(row_ptr + x * pixel_size));
        px_ptr.* = color;
    }
}
