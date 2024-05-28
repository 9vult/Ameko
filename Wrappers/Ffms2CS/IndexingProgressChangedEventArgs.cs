using System;
using System.Collections.Generic;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// EventArgs for <see cref="External.TIndexCallback"/>
    /// </summary>
    public class IndexingProgressChangedEventArgs
    {
        /// <summary>
        /// Current progress
        /// </summary>
        public long Current { get; private set; }
        /// <summary>
        /// What progress will be when complete
        /// </summary>
        public long Total { get; private set; }

        /// <summary>
        /// Set up the EventArgs
        /// </summary>
        /// <param name="current">Current progress</param>
        /// <param name="total">What progress will be when complete</param>
        internal IndexingProgressChangedEventArgs(long current, long total)
        {
            Current = current;
            Total = total;
        }
    }
}
