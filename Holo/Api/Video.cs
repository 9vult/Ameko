using AssCS;
using Holo.Data;
using Holo.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holo.Api
{
    /// <summary>
    /// Experimental API class
    /// </summary>
    public static partial class Fern
    {
        /*
         * This class exposes various API methods, currently only for videos.
         * We may want to move to Holo members being marked internal,
         * leaving just the Fern class and basic data structures publicly accessible.
         * 
         * Alternatively, Holo members could be left as-is (public), and this classset
         * would serve as an ease-of-use wrapper, leaving the "internal" members
         * open for more advanced usecases.
         */

        /// <summary>
        /// Is there a video loaded?
        /// </summary>
        public static bool IsVideoLoaded
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.IsVideoLoaded;

        /// <summary>
        /// Track number of the loaded video
        /// </summary>
        public static int VideoTrack
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.VideoTrack;

        /// <summary>
        /// If the video is playing
        /// </summary>
        public static bool IsPlaying
           => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.IsPlaying;

        /// <summary>
        /// If the video is paused
        /// </summary>
        public static bool IsPaused
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.IsPaused;

        /// <summary>
        /// Currently selected scale multiplier
        /// </summary>
        public static ScalePercentage DisplayScale
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.DisplayScale;

        /// <summary>
        /// Scaled width
        /// </summary>
        public static double DisplayWidth
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.DisplayWidth;

        /// <summary>
        /// Scaled height
        /// </summary>
        public static double DisplayHeight
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.DisplayHeight;

        /// <summary>
        /// Actual width of the video
        /// </summary>
        public static double RealWidth
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.SAR.Numerator;

        /// <summary>
        /// Actual height of the video
        /// </summary>
        public static double RealHeight
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.SAR.Denominator;

        /// <summary>
        /// Screen aspect ratio
        /// </summary>
        public static Rational SAR
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.SAR;

        /// <summary>
        /// Framerate
        /// </summary>
        public static Rational Framerate
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.FrameRate;

        /// <summary>
        /// Number of frames in the video
        /// </summary>
        public static int FrameCount
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.FrameCount;

        /// <summary>
        /// Current frame
        /// </summary>
        public static int CurrentFrame
        {
            get => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.CurrentFrame;
            set => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.CurrentFrame = value;
        }

        /// <summary>
        /// Estimated current time
        /// </summary>
        public static Time CurrentTimeEstimated
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.CurrentTimeEstimated;

    }
}
