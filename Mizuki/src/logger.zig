// SPDX-License-Identifier: MPL-2.0

const common = @import("common.zig");

// Function definition
pub const LogCallback = *const fn (c_int, [*]const u8) callconv(.c) void;

var logger: ?LogCallback = null;

/// Set the callback
pub fn SetCallback(callback: LogCallback) void {
    logger = callback;
    Info("Logger callback set");
}

/// Log a Trace message
pub fn Trace(msg: []const u8) void {
    if (logger) |callback| {
        callback(0, msg.ptr);
    }
}

/// Log a Debug message
pub fn Debug(msg: []const u8) void {
    if (logger) |callback| {
        callback(1, msg.ptr);
    }
}

/// Log an Info message
pub fn Info(msg: []const u8) void {
    if (logger) |callback| {
        callback(2, msg.ptr);
    }
}

/// Log a Warn message
pub fn Warn(msg: []const u8) void {
    if (logger) |callback| {
        callback(3, msg.ptr);
    }
}

/// Log an Error message
pub fn Error(msg: []const u8) void {
    if (logger) |callback| {
        callback(4, msg.ptr);
    }
}
