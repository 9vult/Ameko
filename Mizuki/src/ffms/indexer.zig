// SPDX-License-Identifier: MPL-2.0

//! File Indexer methods

const std = @import("std");
const c = @import("../c.zig").c;

const enums = @import("enums.zig");

const IndexerError = error{
    TrackIndexOutOfRange,
};

/// Opens a file for indexing
pub fn CreateIndexer(file_path: [1024][*c]u8) ?*c.FFMS_Indexer {
    const error_info = c.FFMS_ErrorInfo{
        .BufferSize = 1024,
        .Buffer = [1024]u8,
    };

    return c.FFMS_CreateIndexer(file_path, error_info);
    // TODO: indexing callback
}

/// Get the type of track
pub fn GetTrackType(indexer: *c.FFMS_Indexer, track: c_int) enums.TrackType!IndexerError {
    if (!IsTrackInRange(indexer, track))
        return IndexerError.TrackIndexOutOfRange;

    return @enumFromInt(c.FFMS_GetTrackTypeI(indexer, track));
}

/// Get the name of a track's codec
pub fn GetCodecName(indexer: *c.FFMS_Indexer, track: c_int) [*c]u8!IndexerError {
    if (!IsTrackInRange(indexer, track))
        return IndexerError.TrackIndexOutOfRange;

    return c.FFMS_GetCodecNameI(indexer, track);
}

/// Set whether or not a track should be indexed
pub fn SetTrackShouldIndex(indexer: *c.FFMS_Indexer, track: c_int, should_index: bool) void!IndexerError {
    if (!IsTrackInRange(indexer, track))
        return IndexerError.TrackIndexOutOfRange;

    c.FFMS_TrackIndexSettings(indexer, track, (if (should_index) 1 else 0), 0);
}

/// Set whether or not a type of track should be indexed
pub fn SetTrackTypeShouldIndex(indexer: *c.FFMS_Indexer, track_type: enums.TrackType, should_index: bool) void {
    c.FFMS_TrackTypeIndexSettings(indexer, @intFromEnum(track_type), (if (should_index) 1 else 0), 0);
}

/// Index a track
pub fn IndexTrack(indexer: *c.FFMS_Indexer, track_index: c_int) *c.FFMS_Index {
    const total_track_count = c.FFMS_GetNumTracksI(indexer);
    var i: c_int = 0;
    while (i < total_track_count) : (i += 1) {
        if (i != track_index) {
            SetTrackShouldIndex(indexer, i, false);
        }
    }

    const error_info = c.FFMS_ErrorInfo{
        .BufferSize = 1024,
        .Buffer = [1024]u8,
    };
    return c.FFMS_DoIndexing2(indexer, @intFromEnum(enums.IndexErrorHandling.abort), error_info);
}

/// Check if a given track index is within range of the indexer
fn IsTrackInRange(indexer: *c.FFMS_Indexer, track: c_int) bool {
    if ((track < 0) || (track > c.FFMS_GetNumTracksI(indexer)))
        return false;

    return true;
}
