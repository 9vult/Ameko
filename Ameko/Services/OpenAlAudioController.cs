// SPDX-License-Identifier: GPL-3.0-only

using Ameko.DataModels.OpenAl;
using Holo;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.EXT;

namespace Ameko.Services;

// I have no idea where to put this file so it goes here for now
public class OpenAlAudioController(MediaController mediaController)
{
    private AL _al;
    private ALContext _alc;
    private unsafe Device* _device;
    private unsafe Context* _context;

    private Source _source;

    private bool _isPlaying;
    private uint _bufferHandle;

    public unsafe void Initialize()
    {
        if (_context is not null)
            return;

        _al = AL.GetApi();
        _alc = ALContext.GetApi();

        _device = _alc.OpenDevice(null);
        if (_device is null)
            throw new AudioDeviceException("Failed to open device");

        _context = _alc.CreateContext(_device, null);
        _alc.MakeContextCurrent(_context);

        _al.GetError();

        _source = new Source(_al);
        if (_al.GetError() != AudioError.NoError)
        {
            // _al.DeleteBuffers()
            throw new AudioException("Error generating OpenAL source");
        }

        _bufferHandle = _al.GenBuffer();

        var frame = mediaController.GetAudioFrame();
        _al.BufferData(
            _bufferHandle,
            FloatBufferFormat.Stereo,
            frame->Data,
            (int)frame->Length * sizeof(float),
            frame->SampleRate
        );
        _source.QueueBuffers([_bufferHandle]);
        _source.Play();
    }

    public unsafe void Play(long start, long count)
    {
        Initialize();
        _alc.MakeContextCurrent(_context);

        if (_isPlaying)
        {
            _isPlaying = false;
            _source.Stop();
            // dequeue buffers?
        }

        _source.Play();
    }
}
