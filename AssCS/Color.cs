// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;

namespace AssCS;

/// <summary>
/// A color in a subtitle document, either as part of a
/// <see cref="Style"/> or an <see cref="Overrides.Blocks.OverrideBlock"/>.
/// </summary>
public partial class Color : BindableBase
{
    private byte _red;
    private byte _green;
    private byte _blue;
    private byte _alpha;

    /// <summary>
    /// Red component
    /// </summary>
    public byte Red
    {
        get => _red;
        set => SetProperty(ref _red, value);
    }

    /// <summary>
    /// Green component
    /// </summary>
    public byte Green
    {
        get => _green;
        set => SetProperty(ref _green, value);
    }

    /// <summary>
    /// Blue component
    /// </summary>
    public byte Blue
    {
        get => _blue;
        set => SetProperty(ref _blue, value);
    }

    /// <summary>
    /// Alpha component
    /// </summary>
    /// <remarks>0xFF is transparent, 0x00 is opaque</remarks>
    public byte Alpha
    {
        get => _alpha;
        set => SetProperty(ref _alpha, value);
    }

    /// <summary>
    /// Luminance value of the color
    /// </summary>
    public double Luminance => ((0.2126 * _red) + (0.7152 * _green) + (0.0722 * _blue)) / 255;

    /// <summary>
    /// Ass-formatted style color string (<c>ABGR</c>)
    /// </summary>
    /// <returns>Ass-formatted string for styles</returns>
    /// <remarks>For compatibility reasons, the trailing <c>&amp;</c> is dropped</remarks>
    public string AsStyleColor()
    {
        return $"&H{_alpha:X2}{_blue:X2}{_green:X2}{_red:X2}";
    }

    /// <summary>
    /// Ass-formatted override color string (<c>BGR</c>)
    /// </summary>
    /// <returns>Ass-formatted string</returns>
    public string AsOverrideColor()
    {
        return $"&H{_blue:X2}{_green:X2}{_red:X2}&";
    }

    /// <summary>
    /// Initialize a color from an ass-formatted string
    /// </summary>
    /// <param name="data">Ass-formatted string</param>
    /// <returns>Color object represented by the string</returns>
    /// <exception cref="ArgumentException">If the data is malformed</exception>
    public static Color FromAss(string data)
    {
        var rgbMatch = RgbRegex().Match(data);
        var rgbaMatch = RgbaRegex().Match(data);
        if (!rgbMatch.Success && !rgbaMatch.Success)
            throw new ArgumentException($"Color {data} is invalid or malformed.");

        if (rgbaMatch.Success)
            return new Color
            {
                _alpha = Convert.ToByte(rgbaMatch.Groups[1].Value, 16),
                _blue = Convert.ToByte(rgbaMatch.Groups[2].Value, 16),
                _green = Convert.ToByte(rgbaMatch.Groups[3].Value, 16),
                _red = Convert.ToByte(rgbaMatch.Groups[4].Value, 16),
            };
        return new Color
        {
            _alpha = 0x0,
            _blue = Convert.ToByte(rgbMatch.Groups[1].Value, 16),
            _green = Convert.ToByte(rgbMatch.Groups[2].Value, 16),
            _red = Convert.ToByte(rgbMatch.Groups[3].Value, 16),
        };
    }

    /// <summary>
    /// Initialize a color from RGB values
    /// </summary>
    /// <param name="red">Red value</param>
    /// <param name="green">Green value</param>
    /// <param name="blue">Blue value</param>
    /// <returns>Color object set to the values provided</returns>
    /// <remarks>The color will be fully opaque</remarks>
    public static Color FromRgb(byte red, byte green, byte blue)
    {
        return new Color
        {
            _alpha = 0x0,
            _blue = blue,
            _green = green,
            _red = red,
        };
    }

    /// <summary>
    /// Initialize a color from RGBA values
    /// </summary>
    /// <param name="red">Red value</param>
    /// <param name="green">Green value</param>
    /// <param name="blue">Blue value</param>
    /// <param name="alpha">Alpha value</param>
    /// <returns>Color object set to the values provided</returns>
    public static Color FromRgba(byte red, byte green, byte blue, byte alpha)
    {
        return new Color
        {
            _alpha = alpha,
            _blue = blue,
            _green = green,
            _red = red,
        };
    }

    /// <summary>
    /// Initialize a color from another Color
    /// </summary>
    /// <param name="color">Color to use as the basis</param>
    /// <returns>Color object with the values provided</returns>
    public static Color FromColor(Color color)
    {
        return new Color
        {
            _alpha = color.Alpha,
            _blue = color.Blue,
            _green = color.Green,
            _red = color.Red,
        };
    }

    /// <summary>
    /// Calculate the relative contrast between two colors
    /// </summary>
    /// <param name="a">Color A</param>
    /// <param name="b">Color B</param>
    /// <returns>Relative contrast</returns>
    public static double Contrast(Color a, Color b)
    {
        return (a.Luminance + 0.05) / (b.Luminance + 0.05);
    }

    public override bool Equals(object? obj)
    {
        return obj is Color color
            && _red == color._red
            && _green == color._green
            && _blue == color._blue
            && _alpha == color._alpha;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_red, _green, _blue, _alpha);
    }

    public static bool operator ==(Color? left, Color? right)
    {
        return left?.Equals(right) ?? false;
    }

    public static bool operator !=(Color? left, Color? right)
    {
        return !(left == right);
    }

    #region Operators

    public static Color operator +(Color a, Color b)
    {
        return new Color
        {
            _alpha = (byte)Math.Clamp(a.Alpha + b.Alpha, 0, 255),
            _red = (byte)Math.Clamp(a.Red + b.Red, 0, 255),
            _green = (byte)Math.Clamp(a.Green + b.Green, 0, 255),
            _blue = (byte)Math.Clamp(a.Blue + b.Blue, 0, 255),
        };
    }

    public static Color operator -(Color a, Color b)
    {
        return new Color
        {
            _alpha = (byte)Math.Clamp(a.Alpha - b.Alpha, 0, 255),
            _red = (byte)Math.Clamp(a.Red - b.Red, 0, 255),
            _green = (byte)Math.Clamp(a.Green - b.Green, 0, 255),
            _blue = (byte)Math.Clamp(a.Blue - b.Blue, 0, 255),
        };
    }

    #endregion

    [GeneratedRegex(@"&H([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})&?$")]
    private static partial Regex RgbRegex();

    [GeneratedRegex(@"&H([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})&?$")]
    private static partial Regex RgbaRegex();
}
