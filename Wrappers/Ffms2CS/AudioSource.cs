using Ffms2CS.Enums;
using Ffms2CS.Structures;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// An audio source
    /// </summary>
    public class AudioSource
    {
        private readonly IntPtr _ptr;
        private AudioProperties _properties;
        private Track? _track;

        /// <summary>
        /// Sample format for this audio source
        /// </summary>
        public SampleFormat SampleFormat => (SampleFormat)_properties.SampleFormat;

        /// <summary>
        /// Sample rate
        /// </summary>
        public int SampleRate => _properties.SampleRate;

        /// <summary>
        /// Number of bits per sample
        /// </summary>
        public int BitsPerSample => _properties.BitsPerSample;

        /// <summary>
        /// Number of audio channels
        /// </summary>
        public int ChannelCount => _properties.Channels;

        /// <summary>
        /// Channel layout
        /// </summary>
        public long ChannelLayout => _properties.ChannelLayout;

        /// <summary>
        /// Number of samples
        /// </summary>
        public long SampleCount => _properties.NumSamples;

        /// <summary>
        /// First timestamp
        /// </summary>
        public double FirstTime => _properties.FirstTime;

        /// <summary>
        /// Last timestamp
        /// </summary>
        public double LastTime => _properties.LastTime;

        /// <summary>
        /// Get the track for this audio source
        /// </summary>
        public Track Track
        {
            get
            {
                if (_track != null) return Track;

                _track = new Track(External.GetTrackFromAudio(_ptr));
                return _track;
            }
        }

        /// <summary>
        /// Get some audio samples
        /// </summary>
        /// <seealso cref="External.GetAudio(IntPtr, byte[], long, long, ref ErrorInfo)"/>
        /// <param name="start">Sample to start at</param>
        /// <param name="count">Number of samples to get</param>
        /// <returns>Audio data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Samples are out of bounds</exception>
        /// <exception cref="Exception">An error occured while getting data</exception>
        public byte[] GetAudio(long start, long count)
        {
            if (start < 0 || start > SampleCount - 1) throw new ArgumentOutOfRangeException(nameof(start));
            if (count < 0 || start + count > SampleCount - 1) throw new ArgumentOutOfRangeException(nameof(count));

            var errorInfo = Ffms2.NewErrorInfo();

            var bufferSize = (_properties.BitsPerSample / 8) * ChannelCount * count;
            var buffer = new byte[bufferSize];

            int success;
            lock (this)
            {
                success = External.GetAudio(_ptr, buffer, start, count, ref errorInfo);
            }

            if (success == 0) return buffer;
            throw new Exception($"Exception getting audio: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Instantiate an audio source
        /// </summary>
        /// <param name="sourcePtr">Pointer to the audio source</param>
        internal AudioSource(IntPtr sourcePtr)
        {
            _ptr = sourcePtr;
            _properties = (AudioProperties)Marshal.PtrToStructure(External.GetAudioProperties(_ptr), typeof(AudioProperties));
        }

        /// <summary>
        /// Destroy the audio source
        /// </summary>
        /// <seealso cref="External.DestroyAudioSource(IntPtr)"/>
        ~AudioSource()
        { 
            External.DestroyAudioSource(_ptr);
        }

    }
}
