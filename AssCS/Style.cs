// SPDX-License-Identifier: MPL-2.0

using AssCS.Utilities;

namespace AssCS;

/// <summary>
/// A style in a subtitle document
/// </summary>
/// <param name="id">ID of the style</param>
public partial class Style(int id) : BindableBase
{
    private const string StyleHeader = "Style:";
    private const double Tolerance = 0.001;

    private string _name = "Default";
    private string _fontFamily = "Arial";
    private double _fontSize = 48.0d;
    private Color _primaryColor = Color.FromRgb(0xFF, 0xFF, 0xFF);
    private Color _secondaryColor = Color.FromRgb(0xFF, 0x0, 0x0);
    private Color _outlineColor = Color.FromRgb(0x0, 0x0, 0x0);
    private Color _shadowColor = Color.FromRgb(0x0, 0x0, 0x0);
    private bool _isBold;
    private bool _isItalic;
    private bool _isUnderline;
    private bool _isStrikethrough;
    private double _scaleX = 100.0d;
    private double _scaleY = 100.0d;
    private double _spacing;
    private double _angle;
    private int _borderStyle = 1;
    private double _borderThickness = 2.0d;
    private double _shadowDistance = 2.0d;
    private int _alignment = 2;
    private Margins _margins = new(20, 20, 20);
    private int _encoding = 1;

    /// <summary>
    /// Style ID
    /// </summary>
    public int Id => id;

    /// <summary>
    /// Name of the style
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Font family used by the style
    /// </summary>
    public string FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    /// <summary>
    /// Font size used by the style
    /// </summary>
    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }

    /// <summary>
    /// Primary fill color
    /// </summary>
    public Color PrimaryColor
    {
        get => _primaryColor;
        set => SetProperty(ref _primaryColor, value);
    }

    /// <summary>
    /// Secondary fill color, used for karaoke
    /// </summary>
    public Color SecondaryColor
    {
        get => _secondaryColor;
        set => SetProperty(ref _secondaryColor, value);
    }

    /// <summary>
    /// Outline color
    /// </summary>
    public Color OutlineColor
    {
        get => _outlineColor;
        set => SetProperty(ref _outlineColor, value);
    }

    /// <summary>
    /// Shadow color
    /// </summary>
    public Color ShadowColor
    {
        get => _shadowColor;
        set => SetProperty(ref _shadowColor, value);
    }

    /// <summary>
    /// If the style defaults to bold
    /// </summary>
    public bool IsBold
    {
        get => _isBold;
        set => SetProperty(ref _isBold, value);
    }

    /// <summary>
    /// If the style defaults to italics
    /// </summary>
    public bool IsItalic
    {
        get => _isItalic;
        set => SetProperty(ref _isItalic, value);
    }

    /// <summary>
    /// If the style defaults to underlined
    /// </summary>
    public bool IsUnderline
    {
        get => _isUnderline;
        set => SetProperty(ref _isUnderline, value);
    }

    /// <summary>
    /// If the style defaults to strikethrough
    /// </summary>
    public bool IsStrikethrough
    {
        get => _isStrikethrough;
        set => SetProperty(ref _isStrikethrough, value);
    }

    /// <summary>
    /// Scale on the X-axis. Defaults to 100
    /// </summary>
    public double ScaleX
    {
        get => _scaleX;
        set => SetProperty(ref _scaleX, value);
    }

    /// <summary>
    /// Scale on the Y-axis. Defaults to 100
    /// </summary>
    public double ScaleY
    {
        get => _scaleY;
        set => SetProperty(ref _scaleY, value);
    }

    /// <summary>
    /// Additional spacing between characters
    /// </summary>
    public double Spacing
    {
        get => _spacing;
        set => SetProperty(ref _spacing, value);
    }

    /// <summary>
    /// Rotation angle
    /// </summary>
    public double Angle
    {
        get => _angle;
        set => SetProperty(ref _angle, value);
    }

    /// <summary>
    /// Type of border (outline) to use
    /// </summary>
    public int BorderStyle
    {
        get => _borderStyle;
        set => SetProperty(ref _borderStyle, value);
    }

    /// <summary>
    /// Thickness of the border (outline)
    /// </summary>
    public double BorderThickness
    {
        get => _borderThickness;
        set => SetProperty(ref _borderThickness, value);
    }

    /// <summary>
    /// Distance of the shadow
    /// </summary>
    public double ShadowDistance
    {
        get => _shadowDistance;
        set => SetProperty(ref _shadowDistance, value);
    }

    /// <summary>
    /// Alignment in the video frame
    /// </summary>
    /// <remarks>
    /// Follows numpad order. Defaults to 2 (\an2),
    /// corresponding to the bottom middle of the frame
    /// </remarks>
    public int Alignment
    {
        get => _alignment;
        set => SetProperty(ref _alignment, value);
    }

    /// <summary>
    /// Margins in the frame
    /// </summary>
    public Margins Margins
    {
        get => _margins;
        set => SetProperty(ref _margins, value);
    }

    /// <summary>
    /// Character encoding
    /// </summary>
    public int Encoding
    {
        get => _encoding;
        set => SetProperty(ref _encoding, value);
    }

    /// <summary>
    /// Set fields in this style to the fields from another style
    /// </summary>
    /// <param name="fields">Fields to set</param>
    /// <param name="other">Source of new fields</param>
    public void SetFields(StyleField fields, Style other)
    {
        foreach (StyleField field in fields.GetSetFlags())
        {
            switch (field)
            {
                case StyleField.Name:
                    Name = other.Name;
                    break;
                case StyleField.FontFamily:
                    FontFamily = other.FontFamily;
                    break;
                case StyleField.FontSize:
                    FontSize = other.FontSize;
                    break;
                case StyleField.PrimaryColor:
                    PrimaryColor = other.PrimaryColor;
                    break;
                case StyleField.SecondaryColor:
                    SecondaryColor = other.SecondaryColor;
                    break;
                case StyleField.OutlineColor:
                    OutlineColor = other.OutlineColor;
                    break;
                case StyleField.ShadowColor:
                    ShadowColor = other.ShadowColor;
                    break;
                case StyleField.IsBold:
                    IsBold = other.IsBold;
                    break;
                case StyleField.IsItalic:
                    IsItalic = other.IsItalic;
                    break;
                case StyleField.IsUnderline:
                    IsUnderline = other.IsUnderline;
                    break;
                case StyleField.IsStrikethrough:
                    IsStrikethrough = other.IsStrikethrough;
                    break;
                case StyleField.ScaleX:
                    ScaleX = other.ScaleX;
                    break;
                case StyleField.ScaleY:
                    ScaleY = other.ScaleY;
                    break;
                case StyleField.Spacing:
                    Spacing = other.Spacing;
                    break;
                case StyleField.Angle:
                    Angle = other.Angle;
                    break;
                case StyleField.BorderStyle:
                    BorderStyle = other.BorderStyle;
                    break;
                case StyleField.BorderThickness:
                    BorderThickness = other.BorderThickness;
                    break;
                case StyleField.ShadowDistance:
                    ShadowDistance = other.ShadowDistance;
                    break;
                case StyleField.Alignment:
                    Alignment = other.Alignment;
                    break;
                case StyleField.MarginLeft:
                    Margins.Left = other.Margins.Left;
                    break;
                case StyleField.MarginRight:
                    Margins.Right = other.Margins.Right;
                    break;
                case StyleField.MarginVertical:
                    Margins.Vertical = other.Margins.Vertical;
                    break;
                case StyleField.Encoding:
                    Encoding = other.Encoding;
                    break;
                case StyleField.None:
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Get the ass representation of this style
    /// </summary>
    /// <returns>Ass-formatted string</returns>
    public string AsAss()
    {
        var cleanName = _name.Replace(',', ';');
        var cleanFont = _fontFamily.Replace(',', ';');
        return $"Style: {cleanName},{cleanFont},{_fontSize},"
            + $"{_primaryColor.AsStyleColor()},{_secondaryColor.AsStyleColor()},{_outlineColor.AsStyleColor()},{_shadowColor.AsStyleColor()},"
            + $"{(_isBold ? -1 : 0)},{(_isItalic ? -1 : 0)},{(_isUnderline ? -1 : 0)},{(_isStrikethrough ? -1 : 0)},"
            + $"{_scaleX},{_scaleY},{_spacing},{_angle},{_borderStyle},{_borderThickness},{_shadowDistance},{_alignment},"
            + $"{_margins.Left},{_margins.Right},{_margins.Vertical},{_encoding}";
    }

    /// <summary>
    /// Initialize a style from an ass-formatted string
    /// </summary>
    /// <param name="id">ID of the style</param>
    /// <param name="data">Ass-formatted string</param>
    /// <returns>Style object represented by the string</returns>
    /// <exception cref="ArgumentException">If the data is malformed</exception>
    public static Style? FromAss(int id, ReadOnlySpan<char> data)
    {
        // TODO: Parse format string
        data = data.TrimStart();
        if (!data.StartsWith(StyleHeader))
            return null;

        data = data[StyleHeader.Length..];

        return new Style(id)
        {
            _name = ParseString(ref data),
            _fontFamily = ParseString(ref data),
            _fontSize = ParseDouble(ref data),
            _primaryColor = Color.FromAss(ParseString(ref data)),
            _secondaryColor = Color.FromAss(ParseString(ref data)),
            _outlineColor = Color.FromAss(ParseString(ref data)),
            _shadowColor = Color.FromAss(ParseString(ref data)),
            _isBold = ParseInt(ref data) != 0,
            _isItalic = ParseInt(ref data) != 0,
            _isUnderline = ParseInt(ref data) != 0,
            _isStrikethrough = ParseInt(ref data) != 0,
            _scaleX = ParseDouble(ref data),
            _scaleY = ParseDouble(ref data),
            _spacing = ParseDouble(ref data),
            _angle = ParseDouble(ref data),
            _borderStyle = ParseInt(ref data),
            _borderThickness = ParseDouble(ref data),
            _shadowDistance = ParseDouble(ref data),
            _alignment = ParseInt(ref data),
            _margins = new Margins(
                left: ParseInt(ref data),
                right: ParseInt(ref data),
                vertical: ParseInt(ref data)
            ),
            _encoding = ParseInt(ref data),
        };
    }

    /// <summary>
    /// Initialize a style from an existing style
    /// </summary>
    /// <param name="id">ID of the style</param>
    /// <param name="data">Style to use as the basis</param>
    /// <returns>Style object with the same properties</returns>
    public static Style FromStyle(int id, Style data)
    {
        return new Style(id)
        {
            _name = data.Name,
            _fontFamily = data.FontFamily,
            _fontSize = data.FontSize,
            _primaryColor = Color.FromColor(data.PrimaryColor),
            _secondaryColor = Color.FromColor(data.SecondaryColor),
            _outlineColor = Color.FromColor(data.OutlineColor),
            _shadowColor = Color.FromColor(data.ShadowColor),
            _isBold = data.IsBold,
            _isItalic = data.IsItalic,
            _isUnderline = data.IsUnderline,
            _isStrikethrough = data.IsStrikethrough,
            _scaleX = data.ScaleX,
            _scaleY = data.ScaleY,
            _spacing = data.Spacing,
            _borderStyle = data.BorderStyle,
            _borderThickness = data.BorderThickness,
            _shadowDistance = data.ShadowDistance,
            _alignment = data.Alignment,
            _margins = new Margins(data.Margins.Left, data.Margins.Right, data.Margins.Vertical),
            _encoding = data.Encoding,
        };
    }

    /// <summary>
    /// Clone this style
    /// </summary>
    /// <returns>Clone of the style</returns>
    public Style Clone()
    {
        return new Style(Id)
        {
            Name = Name,
            FontFamily = FontFamily,
            FontSize = FontSize,
            PrimaryColor = Color.FromColor(PrimaryColor),
            SecondaryColor = Color.FromColor(SecondaryColor),
            OutlineColor = Color.FromColor(OutlineColor),
            ShadowColor = Color.FromColor(ShadowColor),
            IsBold = IsBold,
            IsItalic = IsItalic,
            IsUnderline = IsUnderline,
            IsStrikethrough = IsStrikethrough,
            ScaleX = ScaleX,
            ScaleY = ScaleY,
            Spacing = Spacing,
            BorderStyle = BorderStyle,
            BorderThickness = BorderThickness,
            ShadowDistance = ShadowDistance,
            Alignment = Alignment,
            Margins = new Margins(Margins.Left, Margins.Right, Margins.Vertical),
            Encoding = Encoding,
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is Style style && Id == style.Id && IsCongruentWith(style);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Checks equality of non-<see cref="Id"/> fields
    /// </summary>
    /// <param name="obj">Style to check</param>
    /// <returns><see langword="true"/> if,
    /// excluding the <see cref="Id"/>, <paramref name="obj"/> is equal.</returns>
    public bool IsCongruentWith(Style? obj)
    {
        return obj != null
            && _name == obj._name
            && _fontFamily == obj._fontFamily
            && Math.Abs(_fontSize - obj._fontSize) < Tolerance
            && EqualityComparer<Color>.Default.Equals(_primaryColor, obj._primaryColor)
            && EqualityComparer<Color>.Default.Equals(_secondaryColor, obj._secondaryColor)
            && EqualityComparer<Color>.Default.Equals(_outlineColor, obj._outlineColor)
            && EqualityComparer<Color>.Default.Equals(_shadowColor, obj._shadowColor)
            && _isBold == obj._isBold
            && _isItalic == obj._isItalic
            && _isUnderline == obj._isUnderline
            && _isStrikethrough == obj._isStrikethrough
            && Math.Abs(_scaleX - obj._scaleX) < Tolerance
            && Math.Abs(_scaleY - obj._scaleY) < Tolerance
            && Math.Abs(_spacing - obj._spacing) < Tolerance
            && Math.Abs(_angle - obj._angle) < Tolerance
            && _borderStyle == obj._borderStyle
            && Math.Abs(_borderThickness - obj._borderThickness) < Tolerance
            && Math.Abs(_shadowDistance - obj._shadowDistance) < Tolerance
            && _alignment == obj._alignment
            && EqualityComparer<Margins>.Default.Equals(_margins, obj._margins)
            && _encoding == obj._encoding;
    }

    public static bool operator ==(Style? left, Style? right)
    {
        return left?.Equals(right) ?? false;
    }

    public static bool operator !=(Style? left, Style? right)
    {
        return !(left == right);
    }

    #region Parsing Helpers

    /// <summary>
    /// Parse an integer
    /// </summary>
    /// <param name="data">Incoming data</param>
    /// <returns>Resulting integer</returns>
    private static int ParseInt(ref ReadOnlySpan<char> data)
    {
        data = data.TrimStart();
        var q = data.IndexOf(',');
        if (q < 0)
            q = data.Length;
        var result = data[..q].ToString().ParseAssInt();
        data = data[(q < data.Length ? q + 1 : q)..];
        return result;
    }

    /// <summary>
    /// Parse a double
    /// </summary>
    /// <param name="data">Incoming data</param>
    /// <returns>Resulting integer</returns>
    private static double ParseDouble(ref ReadOnlySpan<char> data)
    {
        data = data.TrimStart();
        var q = data.IndexOf(',');
        if (q < 0)
            q = data.Length;
        var result = data[..q].ToString().ParseAssDouble();
        data = data[(q < data.Length ? q + 1 : q)..];
        return result;
    }

    /// <summary>
    /// Parse a string
    /// </summary>
    /// <param name="data">incoming data</param>
    /// <returns>Resulting string</returns>
    private static string ParseString(ref ReadOnlySpan<char> data)
    {
        data = data.TrimStart();
        var q = data.IndexOf(',');
        if (q < 0)
            q = data.Length;
        var result = data[..q].ToString();
        data = data[(q < data.Length ? q + 1 : q)..];
        return result;
    }

    #endregion Parsing Helpers
}
