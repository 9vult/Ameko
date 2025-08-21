// SPDX-License-Identifier: MPL-2.0

const c = @import("c.zig").c;

pub const VideoFrame = extern struct {
    frame_number: c_int = -1,
    timestamp: c_longlong = 0,
    width: c_int,
    height: c_int,
    pitch: c_int,
    flipped: c_int,
    data: [*]u8, // ffms_Frame.Data[0]
    valid: c_int = 0, // bool
};

pub const SubtitleFrame = extern struct {
    hash: u32 = 0,
    timestamp: c_longlong = 0,
    width: c_int,
    height: c_int,
    pitch: c_int,
    flipped: c_int,
    data: [*]u8,
    valid: c_int = 0, // bool
};

pub const FrameGroup = extern struct {
    video_frame: *VideoFrame,
    subtitle_frame: *SubtitleFrame,
};
