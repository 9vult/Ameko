using AssCS;
using Holo.Data;
using Holo.Utilities;

namespace Holo.Api
{
    /// <summary>
    /// Collection of APIs
    /// </summary>
    public static partial class Fern
    {

        /// <summary>
        /// Is there a video loaded?
        /// </summary>
        public static bool IsVideoLoaded
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.IsVideoLoaded;

        /// <summary>
        /// Path to the loaded video file
        /// </summary>
        public static string VideoPath
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.VideoPath;

        /// <summary>
        /// Track number of the loaded video
        /// </summary>
        public static int VideoTrack
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.VideoTrack;

        /// <summary>
        /// If the video is playing
        /// </summary>
        public static bool IsPlaying
           => HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.IsPlaying;

        /// <summary>
        /// If the video is paused
        /// </summary>
        public static bool IsPaused
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.IsPaused;

        /// <summary>
        /// Currently selected scale multiplier
        /// </summary>
        public static ScalePercentage DisplayScale
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.DisplayScale;

        /// <summary>
        /// Scaled width
        /// </summary>
        public static double DisplayWidth
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.DisplayWidth;

        /// <summary>
        /// Scaled height
        /// </summary>
        public static double DisplayHeight
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.DisplayHeight;

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
        /// Rational aspect ratio
        /// </summary>
        public static Rational SAR
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.Video.SAR;

        /// <summary>
        /// Rational framerate
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
            get => HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.CurrentFrame;
            set => HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.CurrentFrame = value;
        }

        /// <summary>
        /// Estimated current time
        /// </summary>
        public static Time CurrentTimeEstimated
            => HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.CurrentTimeEstimated;

    }
}
