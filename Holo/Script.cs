using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TextCopy;
using AssCS;


namespace Holo
{
    /// <summary>
    /// Utility functions for scripts to use.
    /// </summary>
    public static class Script
    {

        /// <summary>
        /// Convert milliseconds to frames.
        /// </summary>
        /// <param name="ms"></param>
        public static int frame_from_ms(int ms)
        {
            // Get FPS from video source
            // fake it for now
            // TODO
            float? fps = 24000/1001;

            // convert frame number to milliseconds
            return (int)(ms * fps / 1000);
        }

        /// <summary>
        /// Convert frames to milliseconds.
        /// </summary>
        /// <param name="frame"></param>
        public static int ms_from_frame(int frame)
        {
            // Get FPS from video source
            // fake it for now
            // TODO
            float? fps = 24000/1001;

            // convert frame number to milliseconds
            return (int)(frame * 1000 / fps);
        }

        /// <summary>
        /// Get the size of the video.
        /// Returns a dictionary with the following keys: width, height, ar, artype.
        /// </summary>
        public static Dictionary<string, object> video_size()
        {
            // Get video size from video source
            // fake it for now
            // TODO

            // Width and height of the video
            int width = 1920;
            int height = 1080;

            // Calculate the aspect ratio (ar)
            float ar = (float)width / height;

            // Determine the aspect ratio type (artype)
            int artype;
            if (Math.Abs(ar - 1.33) < 0.01)
            {
                artype = 1; // 4:3
            }
            else if (Math.Abs(ar - 1.78) < 0.01)
            {
                artype = 2; // 16:9
            }
            else if (Math.Abs(ar - 2.35) < 0.01)
            {
                artype = 3; // 2.35 format
            }
            else
            {
                artype = 4; // Custom aspect ratio
            }

            // Create a dictionary to hold the results
            Dictionary<string, object> result = new Dictionary<string, object>
            {
                { "width", width },
                { "height", height },
                { "ar", ar },
                { "artype", artype }
            };

            return result;
        }

        /// <summary>
        /// Class for interfacing with the clibpoard, with set and get functions.
        /// </summary>
        public static class Clipboard
        {
            /// <summary>
            /// Set the clipboard contents.
            /// </summary>
            public static void Set(string text)
            {
                ClipboardService.SetText(text);
            }

            /// <summary>
            /// Get the clipboard contents.
            /// </summary>
            public static string Get()
            {
                // Retrieve the current text from the clipboard
                string? clipboardText = ClipboardService.GetText();

                // Check if the clipboard text is null or empty
                if (string.IsNullOrEmpty(clipboardText))
                {
                    // If it is, return an empty string
                    return string.Empty;
                }
                else
                {
                    // Otherwise, return the clipboard text
                    return clipboardText;
                }
            }
        }
    }
}
