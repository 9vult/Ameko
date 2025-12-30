// SPDX-License-Identifier: MPL-2.0

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
    /// Ass-formatted override alpha string (<c>A</c>)
    /// </summary>
    /// <returns>Ass-formatted string</returns>
    public string AsOverrideAlpha()
    {
        return $"&H{_alpha:X2}&";
    }

    /// <summary>
    /// Initialize a color from an ass-formatted string
    /// </summary>
    /// <param name="data">Ass-formatted string</param>
    /// <returns>Color object represented by the string</returns>
    /// <exception cref="ArgumentException">If the data is malformed</exception>
    public static Color FromAss(string data)
    {
        var span = data.AsSpan();
        span = span.TrimStart();
        while (span[0] is '&' or 'H')
            span = span[1..];
        while (span[^1] is '&' or 'H')
            span = span[..^1];

        int value;
        try
        {
            value = Convert.ToInt32(span.ToString(), 16);
        }
        catch
        {
            value = 0;
        }

        return span.Length switch
        {
            2 => new Color { _alpha = (byte)value }, // Alpha only
            8 => new Color // ABGR
            {
                _alpha = (byte)((value >> 24) & 0xFF),
                _blue = (byte)((value >> 16) & 0xFF),
                _green = (byte)((value >> 8) & 0xFF),
                _red = (byte)(value & 0xFF),
            },
            _ => new Color // BGR
            {
                _alpha = 0x0,
                _blue = (byte)((value >> 16) & 0xFF),
                _green = (byte)((value >> 8) & 0xFF),
                _red = (byte)(value & 0xFF),
            },
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
    /// Initialize a color with an alpha value
    /// </summary>
    /// <param name="alpha">Alpha value</param>
    /// <returns>Color object set to the values provided<</returns>
    public static Color FromA(byte alpha)
    {
        return new Color { _alpha = alpha };
    }

    /// <summary>
    /// Initialize a color from hex <c>#RRGGBB[AA]</c> string
    /// </summary>
    /// <param name="hex">Hexadecimal representation of the color</param>
    /// <returns>Color object set to the values provided</returns>
    /// <exception cref="ArgumentException">If the hex is invalid</exception>
    public static Color FromHex(string hex)
    {
        if (hex.Length < 6)
            throw new ArgumentException($"Hex color {hex} is invalid.");
        if (hex[0] == '#')
            hex = hex[1..];

        var red = Convert.ToByte(hex[..2], 16);
        var green = Convert.ToByte(hex[2..4], 16);
        var blue = Convert.ToByte(hex[4..6], 16);

        if (hex.Length == 6)
            return FromRgb(red, green, blue);

        var alpha = Convert.ToByte(hex[6..8], 16);
        return FromRgba(red, green, blue, alpha);
    }

    /// <summary>
    /// Initialize a color from an HTML color name
    /// </summary>
    /// <param name="name">HTML color name</param>
    /// <returns>Color object set to the values provided</returns>
    public static Color FromHtml(string name)
    {
        var color = System.Drawing.Color.FromName(name);
        return FromRgb(color.R, color.G, color.B);
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
}
