using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// FFMpeg Index
    /// </summary>
    public class IndexerPtr : SafeHandle
    {
        private IndexerPtr() : base(IntPtr.Zero, true) { }

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
            External.CancelIndexing(handle);
            return true;
        }
    }

    public class Indexer
    {

    }
}
