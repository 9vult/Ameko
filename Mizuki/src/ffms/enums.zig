// SPDX-License-Identifier: MPL-2.0

//! FFMS2 enum types

/// Type of track
const TrackType = enum(c_int) {
    unknown = -1,
    video,
    audio,
    data,
    subtitle,
    attachment,
};

/// How to handle errors during indexing
const IndexErrorHandling = enum(c_int) {
    abort = 0,
    clearTrack,
    stopTrack,
    ignore,
};
