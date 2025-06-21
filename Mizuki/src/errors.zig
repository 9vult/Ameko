// SPDX-License-Identifier: MPL-2.0

const FfmsError = @import("ffms.zig").FfmsError;

pub fn IntFromFfmsError(err: FfmsError) c_int {
    return switch (err) {
        FfmsError.FileNotFound => 1,
        FfmsError.VideoNotSupported => 2,
        FfmsError.NoVideoTracks => 3,
        FfmsError.VideoTrackLoadingFailed => 4,
        FfmsError.DecodingFirstFrameFailed => 5,
        FfmsError.GetTrackInfoFailed => 6,
        FfmsError.GetTrackTimeBaseFailed => 7,
        FfmsError.GetFrameInfoFailed => 8,
        FfmsError.OutOfMemory => 9,
        FfmsError.VideoDecodeError => 10,
        FfmsError.SettingOutputFormatFailed => 11,
    };
}
