// SPDX-License-Identifier: MPL-2.0

const std = @import("std");

pub var gpa: std.heap.GeneralPurposeAllocator(.{}) = .init;
pub const allocator = gpa.allocator();

/// Version of a backing library
pub const BackingVersion = extern struct {
    major: c_int,
    minor: c_int,
    patch: c_int,
};

/// A frame of a video
pub const VideoFrame = extern struct {
    isKeyFrame: bool,
    width: c_int,
    height: c_int,
    data: [4][*c]const u8,
    lineSize: [4]c_int,
};

/// Interoperable int array
pub const IntArray = extern struct {
    ptr: [*c]c_int,
    len: usize,
};
