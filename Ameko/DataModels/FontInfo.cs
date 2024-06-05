using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ameko.DataModels
{
    /// <summary>
    /// Simplified representation of a font
    /// </summary>
    /// <param name="name">Name of the font family</param>
    /// <param name="styles">List of styles in the family</param>
    internal class FontInfo(string name, List<FontStyleMapping> styles)
    {
        /// <summary>
        /// Name of the font family
        /// </summary>
        public string Name { get; private set; } = name;
        /// <summary>
        /// List of styles contained in the family
        /// </summary>
        public List<FontStyleMapping> Styles { get; private set; } = styles;
    }

    /// <summary>
    /// Mapping of font styles to the file containing the style
    /// </summary>
    /// <param name="name">Name of the font style</param>
    /// <param name="path">Location of the style</param>
    internal class FontStyleMapping(string name, string? path)
    {
        /// <summary>
        /// Name of the font style
        /// </summary>
        public string Name { get; private set; } = name;
        /// <summary>
        /// Location of the style
        /// </summary>
        public string? Path { get; private set; } = path;
    }
}
