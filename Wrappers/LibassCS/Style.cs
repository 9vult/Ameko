using LibassCS.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibassCS
{
    public unsafe class Style
    {
        private readonly NativeStyle* _handle;
        internal bool Invalid = false;

        internal NativeStyle* GetHandle() => !Invalid ? _handle : throw new ObjectDisposedException(nameof(Style));

        internal Style(NativeStyle* handle)
        {
            _handle = handle;
        }
    }
}
