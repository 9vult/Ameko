using LibassCS.Enums;
using LibassCS.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibassCS
{
    public unsafe class Track
    {
        private readonly NativeTrack* _handle;
        internal bool Invalid = false;

        internal NativeTrack* GetHandle() => !Invalid ? _handle : throw new ObjectDisposedException(nameof(Track));

        public void ProcessForceStyle()
        {
            if (!Invalid)
                External.ProcessForceStyle(_handle);
            else throw new ObjectDisposedException(nameof(Track));
        }

        public void SetFeature(Feature feature, bool enable)
        {
            if (!Invalid)
                External.SetTrackFeature(_handle, feature, enable);
            else throw new ObjectDisposedException(nameof(Track));
        }

        public int AllocStyle()
        {
            if (!Invalid)
                return External.AllocStyle(_handle);
            else throw new ObjectDisposedException(nameof(Track));
        }

        public int AllocEvent()
        {
            if (!Invalid)
                return External.AllocEvent(_handle);
            else throw new ObjectDisposedException(nameof(Track));
        }

        public void FreeStyle(int styleId)
        {
            if (!Invalid)
                External.FreeStyle(_handle, styleId);
            else throw new ObjectDisposedException(nameof(Track));
        }

        public void FreeEvent(int eventId)
        {
            if (!Invalid)
                External.FreeEvent(_handle, eventId);
            else throw new ObjectDisposedException(nameof(Track));
        }

        // TODO: Process data, etc...

        public void Uninitialize()
        {
            if (!Invalid)
            {
                External.FreeTrack(_handle);
                Invalid = true;
            }
            else throw new ObjectDisposedException(nameof(Track));
        }

        internal Track(NativeTrack* handle)
        {
            _handle = handle;
        }

        ~Track()
        {
            if (!Invalid)
                Uninitialize();
        }
    }
}
