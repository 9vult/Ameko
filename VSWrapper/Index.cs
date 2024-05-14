﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// FFMpeg Index
    /// </summary>
    public class IndexPtr : SafeHandle
    {
        private IndexPtr() : base(IntPtr.Zero, true) { }

        /// <summary>
        /// Returns true if the handle is null (IntPtr.Zero)
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Release the handle
        /// </summary>
        /// <returns>True</returns>
        protected override bool ReleaseHandle()
        {
            External.DestroyIndex(handle);
            return true;
        }
    }

    public class Index
    {

    }
}
