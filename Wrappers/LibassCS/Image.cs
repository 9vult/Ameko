﻿using LibassCS.Enums;
using LibassCS.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibassCS
{
    public unsafe class Image
    {
        private readonly NativeImage _image;

        public int Width => _image.Width;
        public int Height => _image.Height;
        public int Stride => _image.Stride;
        public IntPtr Bitmap => _image.Bitmap;
        public uint Color => _image.Color;
        public int DistX => _image.DistX;
        public int DistY => _image.DistY;
        public Image? Next => _image.Next is not null ? new(_image.Next) : null;
        public ImageType Type => _image.Type;

        internal Image(NativeImage* image)
        {
            _image = *image;
        }
    }
}