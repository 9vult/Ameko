using AssCS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holo.Api
{
    /// <summary>
    /// Collection of APIs
    /// </summary>
    public static partial class Fern
    {

        /// <summary>
        /// The ass file object
        /// </summary>
        public static File File
            => HoloContext.Instance.Workspace.WorkingFile.File;

        /// <summary>
        /// List of currently selected events
        /// </summary>
        public static List<Event>? SelectedEventCollection
            => HoloContext.Instance.Workspace.WorkingFile.SelectedEventCollection;

        /// <summary>
        /// Currently selected event
        /// </summary>
        public static Event? SelectedEvent
            => HoloContext.Instance.Workspace.WorkingFile.SelectedEvent;

        /// <summary>
        /// Path to the loaded subtitle file
        /// </summary>
        public static Uri? SubtitlePath
            => HoloContext.Instance.Workspace.WorkingFile.FilePath;

        /// <summary>
        /// Name of the subtitle file
        /// </summary>
        public static string SubtitleName
            => HoloContext.Instance.Workspace.WorkingFile.Title;

    }
}
