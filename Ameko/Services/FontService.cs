using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Ameko.Services
{
    internal class FontService
    {
        private static readonly Lazy<FontService> _instance = new Lazy<FontService>(() => new FontService());
        public static FontService Instance => _instance.Value;

        private SKFontManager _fontManager;

        public IEnumerable<string> FontFamilies => _fontManager.FontFamilies;

        private FontService()
        {
            _fontManager = SKFontManager.CreateDefault();
        }
        
    }
}
