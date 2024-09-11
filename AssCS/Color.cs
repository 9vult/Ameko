using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AssCS
{
    /// <summary>
    /// Represents a Color
    /// </summary>
    public class Color : IAssComponent
    {
        /// <summary>
        /// Red component
        /// </summary>
        public uint Red { get; set; }
        /// <summary>
        /// Green component
        /// </summary>
        public uint Green { get; set; }
        /// <summary>
        /// Blue component
        /// </summary>
        public uint Blue { get; set; }
        /// <summary>
        /// Alpha component
        /// </summary>
        /// <remarks>0xFF is transparent, and 0x00 is opaque</remarks>
        public uint Alpha { get; set; }

        /// <summary>
        /// Luminance value
        /// </summary>
        public double Luminance => (((0.2126 * Red) + (0.7152 * Green) + (0.0722 * Blue)) / 255) * Alpha;

        public string AsAss()
        {
            return $"&H{Alpha:X2}{Blue:X2}{Green:X2}{Red:X2}";
        }

        public string AsOverride()
        {
            return $"&H{Blue:X2}{Green:X2}{Red:X2}&";
        }

        public void FromAss(string data)
        {
            var rgbRegex = @"&H([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})&?$";
            var rgbaRegex = @"&H([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})&?$";
            var rgbMatch = Regex.Match(data, rgbRegex);
            var rgbaMatch = Regex.Match(data, rgbaRegex);
            if (!rgbMatch.Success && !rgbaMatch.Success) throw new ArgumentException($"Color {data} is invalid or malformed.");

            if (rgbaMatch.Success)
            {
                Alpha = Convert.ToUInt32(rgbaMatch.Groups[1].Value, 16);
                Blue = Convert.ToUInt32(rgbaMatch.Groups[2].Value, 16);
                Green = Convert.ToUInt32(rgbaMatch.Groups[3].Value, 16);
                Red = Convert.ToUInt32(rgbaMatch.Groups[4].Value, 16);
            }
            else
            {
                Alpha = 0;
                Blue = Convert.ToUInt32(rgbMatch.Groups[1].Value, 16);
                Green = Convert.ToUInt32(rgbMatch.Groups[2].Value, 16);
                Red = Convert.ToUInt32(rgbMatch.Groups[3].Value, 16);
            }
        }

        /// <summary>
        /// Compose a Color from an ASS string &H[color]&.
        /// </summary>
        /// <param name="data">ASS String input in AABBGGRR or BBGGRR</param>
        /// <exception cref="ArgumentException">If the input is invalid</exception>
        public Color(string data) : this()
        {
            FromAss(data);
        }

        /// <summary>
        /// Clone a color
        /// </summary>
        /// <param name="c">Color to clone</param>
        public Color(Color c)
        {
            Red = c.Red;
            Green = c.Green;
            Blue = c.Blue;
            Alpha = c.Alpha;
        }

        /// <summary>
        /// Compose a Color from RGB values
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public Color(uint red, uint green, uint blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = 0;
        }

        /// <summary>
        /// Compose a Color from RGBA values
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public Color(uint red, uint green, uint blue, uint alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// Compose a new Color. By default, it will be
        /// black with full opacity.
        /// </summary>
        public Color()
        {
            Red = 0;
            Green = 0;
            Blue = 0;
            Alpha = 0;
        }

        public static Color operator +(Color a, Color b)
        {
            return new Color(
                Math.Clamp(a.Alpha + b.Alpha, 0, 255),
                Math.Clamp(a.Red + b.Red, 0, 255),
                Math.Clamp(a.Green + b.Green, 0, 255),
                Math.Clamp(a.Blue + b.Blue, 0, 255)
            );
        }

        public static Color operator -(Color a, Color b)
        {
            return new Color(
                Math.Clamp(a.Alpha - b.Alpha, 0, 255),
                Math.Clamp(a.Red - b.Red, 0, 255),
                Math.Clamp(a.Green - b.Green, 0, 255),
                Math.Clamp(a.Blue - b.Blue, 0, 255)
            );
        }

        /// <summary>
        /// Calculate the relative contrast between two colors
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Contrast</returns>
        public static double Contrast(Color a, Color b)
        {
            return (a.Luminance + 0.05) / (b.Luminance + 0.05);
        }
    }
}
