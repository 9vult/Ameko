using System;
using System.Collections.Generic;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// A rectangle representing how much to crop in on a frame
    /// </summary>
    public class Crop
    {
        /// <summary>
        /// How much to crop in from the top
        /// </summary>
        public int Top { get; private set; }
        /// <summary>
        /// How much to crop in from the left
        /// </summary>
        public int Left { get; private set; }
        /// <summary>
        /// How much to crop in from the right
        /// </summary>
        public int Right { get; private set; }
        /// <summary>
        /// How much to crop in from the bottom
        /// </summary>
        public int Bottom { get; private set; }

        /// <summary>
        /// Create a crop
        /// </summary>
        /// <param name="top">How much to crop in from the top</param>
        /// <param name="left">How much to crop in from the left</param>
        /// <param name="right">How much to crop in from the right</param>
        /// <param name="bottom">How much to crop in from the bottom</param>
        internal Crop(int top, int left, int right, int bottom)
        {
            Top = top;
            Left = left;
            Right = right;
            Bottom = bottom;
        }
    }
}
