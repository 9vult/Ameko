using Ameko.DataModels;
using SixLabors.Fonts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ameko.Services
{
    internal class FontService
    {
        private static readonly Lazy<FontService> _instance = new(() => new FontService());
        public static FontService Instance => _instance.Value;

        public List<FontInfo> Fonts { get; private set; }

        private FontService()
        {
            Fonts = [];

            IOrderedEnumerable<FontFamily> ordered = SystemFonts.Families.OrderBy(x => x.Name);
            foreach (FontFamily family in ordered)
            {
                IOrderedEnumerable<FontStyle> styles = family.GetAvailableStyles().OrderBy(x => x);
                var styleNames = new List<FontStyleMapping>();
                foreach (FontStyle style in styles)
                {
                    Font font = family.CreateFont(0F, style);
                    font.TryGetPath(out string? path);
                    styleNames.Add(new FontStyleMapping(font.Name, path));
                }
                Fonts.Add(new FontInfo(family.Name, styleNames));
            }
        }

    }
}
