const std = @import("std");
const builtin = @import("builtin");

const common = @import("common.zig");
const logger = @import("logger.zig");
const context = @import("context.zig");

pub const KeyframesError = error{ InvalidKeyframeType, EmptyFile, IoFailure, OutOfMemory, Overflow, InvalidCharacter };

const delimiter: u8 = '\n';
const KeyframeType = enum {
    None,
    Agi,
    XviD,
    DivX, // Not implemented
    X264, // Not implemented
    Wwxd,
};

pub fn Load(file_name: [*c]u8, g_ctx: *context.GlobalContext) KeyframesError!void {
    const path: []const u8 = std.mem.span(file_name);
    const file = std.fs.cwd().openFile(path, .{ .mode = .read_only }) catch return KeyframesError.IoFailure;
    defer file.close();
    var read_buf: [1024]u8 = undefined; // 1kb buffer
    var f_reader: std.fs.File.Reader = file.reader(&read_buf);

    var line = std.Io.Writer.Allocating.init(common.allocator);
    defer line.deinit();

    // Get the header
    _ = f_reader.interface.streamDelimiter(&line.writer, delimiter) catch |err| {
        if (err == error.EndOfStream) return KeyframesError.EmptyFile else return KeyframesError.IoFailure;
    };
    _ = f_reader.interface.toss(1); // skip the delimiter byte.

    var kf_type: KeyframeType = .None;
    if (std.mem.startsWith(u8, line.written(), "# keyframe format v1")) {
        kf_type = .Agi;
    } else if (std.mem.startsWith(u8, line.written(), "# XviD 2pass stat file") or std.mem.startsWith(u8, line.written(), "# ffmpeg 2-pass log file, using xvid codec") or std.mem.startsWith(u8, line.written(), "# avconv 2-pass log file, using xvid codec")) {
        kf_type = .XviD;
    } else if (std.mem.startsWith(u8, line.written(), "# WWXD log file, using qpfile format")) {
        kf_type = .Wwxd;
    }

    if (kf_type == .None) return;

    switch (kf_type) {
        .Agi => {
            try ProcessAgiKeyframes(&f_reader, g_ctx);
        },
        .XviD => {
            try ProcessXvidKeyframes(&f_reader, g_ctx);
        },
        .Wwxd => {
            try ProcessWwxdKeyframes(&f_reader, g_ctx);
        },
        else => {
            return KeyframesError.InvalidKeyframeType;
        },
    }
}

fn ProcessAgiKeyframes(f_reader: *std.fs.File.Reader, g_ctx: *context.GlobalContext) !void {
    var ctx = &g_ctx.*.ffms;

    var keyframes_list: std.ArrayList(c_int) = .empty;
    errdefer keyframes_list.deinit(common.allocator);

    var line = std.Io.Writer.Allocating.init(common.allocator);
    defer line.deinit();

    // skip FPS entry
    _ = f_reader.interface.streamDelimiter(&line.writer, delimiter) catch |err| {
        if (err == error.EndOfStream) return else return KeyframesError.IoFailure; // Do nothing if empty
    };
    _ = f_reader.interface.toss(1);

    // Read in keyframes
    while (true) {
        _ = f_reader.interface.streamDelimiter(&line.writer, delimiter) catch |err| {
            if (err == error.EndOfStream) break else return KeyframesError.IoFailure; // Stop
        };
        _ = f_reader.interface.toss(1); // skip the delimiter byte.

        const kf = try std.fmt.parseInt(c_int, line.written(), 10);
        try keyframes_list.append(common.allocator, kf);
    }

    if (ctx.keyframes) |keyframes| {
        common.allocator.free(keyframes);
        ctx.keyframes = null;
    }
    ctx.keyframes = keyframes_list.toOwnedSlice(common.allocator) catch unreachable;
}

fn ProcessXvidKeyframes(f_reader: *std.fs.File.Reader, g_ctx: *context.GlobalContext) !void {
    var ctx = &g_ctx.*.ffms;

    var keyframes_list: std.ArrayList(c_int) = .empty;
    errdefer keyframes_list.deinit(common.allocator);

    var count: c_int = 0;
    var line = std.Io.Writer.Allocating.init(common.allocator);
    defer line.deinit();

    // Read in keyframes
    while (true) {
        _ = f_reader.interface.streamDelimiter(&line.writer, delimiter) catch |err| {
            if (err == error.EndOfStream) break else return KeyframesError.IoFailure; // Stop
        };
        _ = f_reader.interface.toss(1); // skip the delimiter byte.

        // Empty or comment line
        if (line.written().len == 0 or std.mem.startsWith(u8, line.written(), "#")) {
            continue;
        }

        const key = line.written()[0];
        if (key == 'i' or key == 'I') {
            count += 1;
            try keyframes_list.append(common.allocator, count);
        } else if (key == 'p' or key == 'P' or key == 'b' or key == 'B') {
            count += 1;
        }
    }

    if (ctx.keyframes) |keyframes| {
        common.allocator.free(keyframes);
        ctx.keyframes = null;
    }
    ctx.keyframes = keyframes_list.toOwnedSlice(common.allocator) catch unreachable;
}

fn ProcessWwxdKeyframes(f_reader: *std.fs.File.Reader, g_ctx: *context.GlobalContext) !void {
    var ctx = &g_ctx.*.ffms;

    var keyframes_list: std.ArrayList(c_int) = .empty;
    errdefer keyframes_list.deinit(common.allocator);

    var line = std.Io.Writer.Allocating.init(common.allocator);
    defer line.deinit();

    // Read in keyframes
    while (true) {
        _ = f_reader.interface.streamDelimiter(&line.writer, delimiter) catch |err| {
            if (err == error.EndOfStream) break else return KeyframesError.IoFailure; // Stop
        };
        _ = f_reader.interface.toss(1); // skip the delimiter byte.

        // Empty or comment line
        if (line.written().len == 0 or std.mem.startsWith(u8, line.written(), "#")) {
            continue;
        }

        if (std.mem.count(u8, line.written(), "I") > 0) {
            if (std.mem.indexOfScalar(u8, line.written(), ' ')) |space_idx| {
                const kf = try std.fmt.parseInt(c_int, line.written()[0..space_idx], 10);
                try keyframes_list.append(common.allocator, kf);
            }
        }
    }

    if (ctx.keyframes) |keyframes| {
        common.allocator.free(keyframes);
        ctx.keyframes = null;
    }
    ctx.keyframes = keyframes_list.toOwnedSlice(common.allocator) catch unreachable;
}
