// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using AssCS.Overrides.Blocks;
using AssCS.Utilities;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace AssCS.Overrides;

/// <summary>
/// A component in an <see cref="OverrideBlock"/>
/// </summary>
/// <remarks>
/// <para>
/// Each tag has "raw" and "controlled" accessors. When working with a tag,
/// you can choose which accessors to use based on your needs.
/// </para><para>
/// For example, if you are creating a tag with known values, you may choose
/// to use the controlled accessors and constructor. On the other hand, the raw
/// accessors may be more well-suited for parsing, or when inline code or
/// variables are involved.
/// </para><para>
/// For each, the raw value is what will be used when generating the output
/// string. The controlled values, then, are wrappers around the raw value that
/// may perform additional parsing to get the "real type" from the raw string.
/// </para>
/// </remarks>
public abstract class OverrideTag
{
    /// <summary>
    /// Name of the tag, excluding the backslash <c>\</c>
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// ASS-formated representation of the tag and its components
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $@"\{Name}";

    /// <summary>
    /// Line alignment (legacy)
    /// </summary>
    /// <remarks>
    /// Specify the alignment of the line using legacy alignment codes
    /// from SubStation Alpha.
    /// Use <see cref="OverrideTag.An"/> instead.
    /// </remarks>
    /// <example>\a5</example>
    public class A : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.A;

        /// <summary>
        /// Line alignment between 1 and 11
        /// </summary>
        public int? Value
        {
            get => RawValue?.ParseAssInt() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Line alignment
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\a</c> tag
        /// </summary>
        /// <param name="value">Line alignment</param>
        public A(int? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\a</c> tag
        /// </summary>
        /// <param name="value">Line alignment</param>
        public A(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() => $@"\{Name}{RawValue}";
    }

    /// <summary>
    /// Primary fill alpha transparency
    /// </summary>
    /// <param name="value">Hexadecimal alpha</param>
    /// <example>\1a&amp;HFF&amp;</example>
    public class A1(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.A1;

        /// <summary>
        /// Primary fill alpha
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Secondary fill alpha transparency
    /// </summary>
    /// <param name="value">Hexadecimal alpha</param>
    /// <example>\2a&amp;HFF&amp;</example>
    public class A2(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.A2;

        /// <summary>
        /// Secondary fill alpha
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Outline alpha transparency
    /// </summary>
    /// <param name="value">Hexadecimal alpha</param>
    /// <example>\3a&amp;HFF&amp;</example>
    public class A3(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.A3;

        /// <summary>
        /// Outline alpha
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Shadow alpha transparency
    /// </summary>
    /// <param name="value">Hexadecimal alpha</param>
    /// <example>\4a&amp;HFF&amp;</example>
    public class A4(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.A4;

        /// <summary>
        /// Shadow alpha
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Primary, secondary, outline, and shadow alpha
    /// </summary>
    /// <param name="alpha">Hexadecimal alpha</param>
    /// <example>\alpha&amp;HFF&amp;</example>
    public class Alpha(string? alpha) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Alpha;

        /// <summary>
        /// Alpha
        /// </summary>
        public string? Value { get; set; } = alpha;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Line alignment
    /// </summary>
    /// <example>\an2</example>
    public class An : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.An;

        /// <summary>
        /// Line alignment between 1 and 9
        /// </summary>
        public int? Value
        {
            get => RawValue?.ParseAssInt() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Line alignment
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\an</c> tag
        /// </summary>
        /// <param name="value">line alignment</param>
        public An(int? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\an</c> tag
        /// </summary>
        /// <param name="value">line alignment</param>
        public An(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Bold
    /// </summary>
    /// <example>\b1</example>
    public class B : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.B;

        /// <summary>
        /// Boldness
        /// </summary>
        /// <remarks>
        /// Off=0, on=1, weight=n*100
        /// </remarks>
        public int? Value
        {
            get => RawValue?.ParseAssInt();
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Boldness
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\b</c> tag
        /// </summary>
        /// <param name="value">Boldness value</param>
        public B(int? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\b</c> tag
        /// </summary>
        /// <param name="value">Boldness value</param>
        public B(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Blur edges
    /// </summary>
    /// <example>\be1</example>
    public class Be : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Be;

        /// <summary>
        /// Blurriness
        /// </summary>
        /// <remarks>
        /// Off=0, on=1, strength=n
        /// </remarks>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Blurriness
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\be</c> tag
        /// </summary>
        /// <param name="value">Blurriness value</param>
        public Be(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\be</c> tag
        /// </summary>
        /// <param name="value">Blurriness value</param>
        public Be(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Blur edges (Gaussian kernel)
    /// </summary>
    /// <example>\blur4.5</example>
    public class Blur : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Blur;

        /// <summary>
        /// Blurriness
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Blurriness
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\blur</c> tag
        /// </summary>
        /// <param name="value">Blurriness</param>
        public Blur(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\blur</c> tag
        /// </summary>
        /// <param name="value">Blurriness</param>
        public Blur(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Border size
    /// </summary>
    /// <example>\bord2</example>
    /// <seealso cref="OverrideTag.XBord"/>
    /// <seealso cref="OverrideTag.YBord"/>
    public class Bord : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Bord;

        /// <summary>
        /// Thickness of the border
        /// </summary>
        public double? Thickness
        {
            get => RawThickness?.ParseAssDouble() ?? 0;
            set => RawThickness = value?.ToString();
        }

        /// <summary>
        /// Thickness of the border
        /// </summary>
        public string? RawThickness { get; set; }

        /// <summary>
        /// Create a <c>\bord</c> tag
        /// </summary>
        /// <param name="thickness">Thickness of the border</param>
        public Bord(double? thickness)
        {
            Thickness = thickness;
        }

        /// <summary>
        /// Create a <c>\bord</c> tag
        /// </summary>
        /// <param name="thickness">Thickness of the border</param>
        public Bord(string? thickness)
        {
            RawThickness = thickness;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawThickness is not null ? $@"\{Name}{RawThickness}" : $@"\{Name}";
    }

    /// <summary>
    /// Primary fill color
    /// </summary>
    /// <param name="value">Hexadecimal color value</param>
    /// <example>\c&amp;HFF5398&amp;</example>
    /// <seealso cref="OverrideTag.C1"/>
    public class C(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.C;

        /// <summary>
        /// Hexadecimal color
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Primary fill color
    /// </summary>
    /// <param name="value">Hexadecimal color value</param>
    /// <example>\1c&amp;HFF5398&amp;</example>
    /// <seealso cref="OverrideTag.C"/>
    public class C1(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.C1;

        /// <summary>
        /// Hexadecimal color
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Secondary fill color
    /// </summary>
    /// <param name="value">Hexadecimal color value</param>
    /// <example>\2c&amp;HFF5398&amp;</example>
    public class C2(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.C2;

        /// <summary>
        /// Hexadecimal color
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Outline color
    /// </summary>
    /// <param name="value">Hexadecimal color value</param>
    /// <example>\3c&amp;HFF5398&amp;</example>
    public class C3(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.C3;

        /// <summary>
        /// Hexadecimal color
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Shadow color
    /// </summary>
    /// <param name="value">Hexadecimal color value</param>
    /// <example>\4c&amp;HFF5398&amp;</example>
    public class C4(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.C4;

        /// <summary>
        /// Hexadecimal color
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Clip to a section
    /// </summary>
    /// <example>\clip(0,0,100,175)</example>
    public class Clip : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Clip;

        /// <summary>
        /// x0 coordinate for rectangular clips
        /// </summary>
        public int X0
        {
            get => RawX0?.ParseAssInt() ?? 0;
            set => RawX0 = value.ToString();
        }

        /// <summary>
        /// y0 coordinate for rectangular clips
        /// </summary>
        public int Y0
        {
            get => RawY0?.ParseAssInt() ?? 0;
            set => RawY0 = value.ToString();
        }

        /// <summary>
        /// x1 coordinate for rectangular clips
        /// </summary>
        public int X1
        {
            get => RawX1?.ParseAssInt() ?? 0;
            set => RawX1 = value.ToString();
        }

        /// <summary>
        /// y1 coordinate for rectangular clips
        /// </summary>
        public int Y1
        {
            get => RawY1?.ParseAssInt() ?? 0;
            set => RawY1 = value.ToString();
        }

        /// <summary>
        /// Scale of a drawing clip
        /// </summary>
        public double Scale
        {
            get => RawScale?.ParseAssDouble() ?? 0;
            set => RawScale = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Drawing commands
        /// </summary>
        public string? Drawing { get; set; }

        /// <summary>
        /// x0 coordinate for rectangular clips
        /// </summary>
        public string? RawX0 { get; set; }

        /// <summary>
        /// y0 coordinate for rectangular clips
        /// </summary>
        public string? RawY0 { get; set; }

        /// <summary>
        /// x1 coordinate for rectangular clips
        /// </summary>
        public string? RawX1 { get; set; }

        /// <summary>
        /// y1 coordinate for rectangular clips
        /// </summary>
        public string? RawY1 { get; set; }

        /// <summary>
        /// Scale of a drawing clip
        /// </summary>
        public string? RawScale { get; set; }

        /// <summary>
        /// Type of clip
        /// </summary>
        public ClipVariant? Variant { get; set; }

        /// <summary>
        /// Create a rectangular <c>\clip</c>
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public Clip(int x0, int y0, int x1, int y1)
        {
            Variant = ClipVariant.Rectangle;
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        /// <summary>
        /// Create a drawing <c>\clip</c>
        /// </summary>
        /// <param name="drawing">Drawing commands</param>
        public Clip(string drawing)
        {
            Variant = ClipVariant.Drawing;
            Drawing = drawing;
        }

        /// <summary>
        /// Create a scaled drawing <c>\clip</c>
        /// </summary>
        /// <param name="scale">Scale factor</param>
        /// <param name="drawing">Drawing commands</param>
        public Clip(double scale, string drawing)
        {
            Variant = ClipVariant.ScaledDrawing;
            Scale = scale;
            Drawing = drawing;
        }

        /// <summary>
        /// Create a rectangular <c>\clip</c>
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public Clip(string x0, string y0, string x1, string y1)
        {
            Variant = ClipVariant.Rectangle;
            RawX0 = x0;
            RawY0 = y0;
            RawX1 = x1;
            RawY1 = y1;
        }

        /// <summary>
        /// Create a scaled drawing <c>\clip</c>
        /// </summary>
        /// <param name="scale">Scale factor</param>
        /// <param name="drawing">Drawing commands</param>
        public Clip(string scale, string drawing)
        {
            Variant = ClipVariant.ScaledDrawing;
            RawScale = scale;
            Drawing = drawing;
        }

        /// <inheritdoc />
        public override string ToString() =>
            Variant switch
            {
                ClipVariant.Rectangle => $@"\{Name}({RawX0},{RawY0},{RawX1},{RawY1})",
                ClipVariant.Drawing => $@"\{Name}({Drawing})",
                ClipVariant.ScaledDrawing => $@"\{Name}({RawScale},{Drawing})",
                _ => throw new ArgumentOutOfRangeException(nameof(Variant), Variant, null),
            };

        /// <summary>
        /// Type of clip
        /// </summary>
        public enum ClipVariant
        {
            Rectangle,
            Drawing,
            ScaledDrawing,
        }
    }

    /// <summary>
    /// Simple fade
    /// </summary>
    /// <example>\fad(100,100)</example>
    public class Fad : Fade
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fad;

        /// <inheritdoc />
        public Fad(int t2, int t3)
            : base(t2, t3) { }

        /// <inheritdoc />
        public Fad(string t2, string t3)
            : base(t2, t3) { }

        /// <inheritdoc />
        public Fad(int a1, int a2, int a3, int t1, int t2, int t3, int t4)
            : base(a1, a2, a3, t1, t2, t3, t4) { }

        /// <inheritdoc />
        public Fad(string a1, string a2, string a3, string t1, string t2, string t3, string t4)
            : base(a1, a2, a3, t1, t2, t3, t4) { }
    }

    /// <summary>
    /// Complex (5-part) fade
    /// </summary>
    /// <example>\fade(255,32,224,0,500,2000,2200)</example>
    public class Fade : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fade;

        /// <summary>
        /// Fade variant
        /// </summary>
        public bool IsShortVariant { get; set; }

        /// <summary>
        /// Alpha before <see cref="T1"/>
        /// </summary>
        public int Alpha1
        {
            get => RawAlpha1?.ParseAssInt() ?? 0;
            set => RawAlpha1 = value.ToString();
        }

        /// <summary>
        /// Fade to this alpha between <see cref="T1"/> and <see cref="T2"/>
        /// </summary>
        public int Alpha2
        {
            get => RawAlpha2?.ParseAssInt() ?? 0;
            set => RawAlpha2 = value.ToString();
        }

        /// <summary>
        /// Fade to this alpha between <see cref="T3"/> and <see cref="T4"/>
        /// </summary>
        public int Alpha3
        {
            get => RawAlpha3?.ParseAssInt() ?? 0;
            set => RawAlpha3 = value.ToString();
        }

        /// <summary>
        /// Timestamp 1 in milliseconds
        /// </summary>
        public int T1
        {
            get => RawT1?.ParseAssInt() ?? 0;
            set => RawT1 = value.ToString();
        }

        /// <summary>
        /// Timestamp 2 in milliseconds
        /// </summary>
        public int T2
        {
            get => RawT2?.ParseAssInt() ?? 0;
            set => RawT2 = value.ToString();
        }

        /// <summary>
        /// Timestamp 3 in milliseconds
        /// </summary>
        public int T3
        {
            get => RawT3?.ParseAssInt() ?? 0;
            set => RawT3 = value.ToString();
        }

        /// <summary>
        /// Timestamp 4 in milliseconds
        /// </summary>
        public int T4
        {
            get => RawT4?.ParseAssInt() ?? 0;
            set => RawT4 = value.ToString();
        }

        /// <summary>
        /// Alpha before <see cref="T1"/>
        /// </summary>
        public string? RawAlpha1 { get; set; }

        /// <summary>
        /// Fade to this alpha between <see cref="T1"/> and <see cref="T2"/>
        /// </summary>
        public string? RawAlpha2 { get; set; }

        /// <summary>
        /// Fade to this alpha between <see cref="T3"/> and <see cref="T4"/>
        /// </summary>
        public string? RawAlpha3 { get; set; }

        /// <summary>
        /// Timestamp 1
        /// </summary>
        public string? RawT1 { get; set; }

        /// <summary>
        /// Timestamp 2
        /// </summary>
        public string? RawT2 { get; set; }

        /// <summary>
        /// Timestamp 3
        /// </summary>
        public string? RawT3 { get; set; }

        /// <summary>
        /// Timestamp 4
        /// </summary>
        public string? RawT4 { get; set; }

        /// <summary>
        /// Create a <c>\fade</c> tag
        /// </summary>
        /// <param name="t2">Fade-in duration</param>
        /// <param name="t3">Fade-out duration</param>
        public Fade(int t2, int t3)
        {
            IsShortVariant = true;
            T2 = t2;
            T3 = t3;
        }

        /// <summary>
        /// Create a <c>\fad</c> tag
        /// </summary>
        /// <param name="t2">Fade-in duration</param>
        /// <param name="t3">Fade-out duration</param>
        public Fade(string t2, string t3)
        {
            IsShortVariant = true;
            RawT2 = t2;
            RawT3 = t3;
        }

        /// <summary>
        /// Create a 5-part <c>\fade</c> tag
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="a3"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        public Fade(int a1, int a2, int a3, int t1, int t2, int t3, int t4)
        {
            IsShortVariant = false;
            Alpha1 = a1;
            Alpha2 = a2;
            Alpha3 = a3;
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
        }

        /// <summary>
        /// Create a 5-part <c>\fade</c> tag
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="a3"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        public Fade(string a1, string a2, string a3, string t1, string t2, string t3, string t4)
        {
            IsShortVariant = false;
            RawAlpha1 = a1;
            RawAlpha2 = a2;
            RawAlpha3 = a3;
            RawT1 = t1;
            RawT2 = t2;
            RawT3 = t3;
            RawT4 = t4;
        }

        /// <inheritdoc />
        public override string ToString() =>
            IsShortVariant
                ? $@"\{Name}({RawT2},{RawT3})"
                : $@"\{Name}({RawAlpha1},{RawAlpha2},{RawAlpha3},{RawT1},{RawT2},{RawT3},{RawT4})";
    }

    /// <summary>
    /// Shearing distortion about the x-axis
    /// </summary>
    /// <example>\fax-1.5</example>
    public class FaX : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FaX;

        /// <summary>
        /// Distortion amount
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Distortion amount
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\fax</c> tag
        /// </summary>
        /// <param name="value">Distortion amount</param>
        public FaX(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\fax</c> tag
        /// </summary>
        /// <param name="value">Distortion amount</param>
        public FaX(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Shearing distortion about the y-axis
    /// </summary>
    /// <example>\fay-1.5</example>
    public class FaY : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FaY;

        /// <summary>
        /// Distortion amount
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Distortion amount
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\fay</c> tag
        /// </summary>
        /// <param name="value">Distortion amount</param>
        public FaY(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\fay</c> tag
        /// </summary>
        /// <param name="value">Distortion amount</param>
        public FaY(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Font encoding
    /// </summary>
    /// <remarks>
    /// Using this is rarely a good idea
    /// </remarks>
    /// <example>\fe1</example>
    public class Fe : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fe;

        /// <summary>
        /// Font encoding id
        /// </summary>
        public int? Value
        {
            get => RawValue?.ParseAssInt() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Font encoding id
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\fe</c> tag
        /// </summary>
        /// <param name="value">Encoding id</param>
        public Fe(int? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\fe</c> tag
        /// </summary>
        /// <param name="value">Encoding id</param>
        public Fe(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Font name
    /// </summary>
    /// <param name="value">Name of the font</param>
    /// <example>\fnTimes New Roman</example>
    public class Fn(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fn;

        /// <summary>
        /// Name of the font
        /// </summary>
        public string? Value { get; set; } = value;

        /// <inheritdoc />
        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    /// <summary>
    /// Rotation about the z-axis
    /// </summary>
    /// <example>\fr120</example>
    /// <seealso cref="OverrideTag.FrZ"/>
    public class Fr : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fr;

        /// <summary>
        /// Rotation in degrees
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Rotation
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\fr</c> tag
        /// </summary>
        /// <param name="value">Rotation</param>
        public Fr(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\fr</c> tag
        /// </summary>
        /// <param name="value">Rotation</param>
        public Fr(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Rotation about the x-axis
    /// </summary>
    /// <example>\frx120</example>
    public class FrX : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FrX;

        /// <summary>
        /// Rotation in degrees
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Rotation
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\frx</c> tag
        /// </summary>
        /// <param name="value">Rotation</param>
        public FrX(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\frx</c> tag
        /// </summary>
        /// <param name="value">Rotation</param>
        public FrX(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Rotation about the y-axis
    /// </summary>
    /// <example>\fry120</example>
    public class FrY : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FrY;

        /// <summary>
        /// Rotation in degrees
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Rotation
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\fry</c> tag
        /// </summary>
        /// <param name="value">Rotation</param>
        public FrY(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\fry</c> tag
        /// </summary>
        /// <param name="value">Rotation</param>
        public FrY(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Rotation about the z-axis
    /// </summary>
    /// <example>\frz120</example>
    /// <seealso cref="OverrideTag.Fr"/>
    public class FrZ : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FrZ;

        /// <summary>
        /// Rotation in degrees
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Rotation
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\frz</c> tag
        /// </summary>
        /// <param name="value">Rotation</param>
        public FrZ(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\frz</c> tag
        /// </summary>
        /// <param name="value">Rotation</param>
        public FrZ(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Font size
    /// </summary>
    /// <remarks>
    /// The font size is in script pixels (height)
    /// </remarks>
    /// <example>\fs72</example>
    public class Fs : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fs;

        /// <summary>
        /// Font size in pixel height
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Font size
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Tag variant
        /// </summary>
        public FsVariant Variant { get; set; }

        /// <summary>
        /// Create an <c>fs</c> tag
        /// </summary>
        /// <param name="value">Font size</param>
        /// <param name="variant">Variant</param>
        public Fs(double? value, FsVariant variant)
        {
            Value = value;
            Variant = variant;
        }

        /// <summary>
        /// Create an <c>fs</c> tag
        /// </summary>
        /// <param name="value">Font size</param>
        /// <param name="variant">Variant</param>
        public Fs(string? value, FsVariant variant)
        {
            RawValue = value;
            Variant = variant;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Value is null)
                return $@"\{Name}";

            switch (Variant)
            {
                case FsVariant.Absolute:
                case FsVariant.Relative when Value is < 0:
                    return $@"\{Name}{RawValue}";
                case FsVariant.Relative:
                    if (RawValue is not null && RawValue[0] is not '+')
                        return $@"\{Name}+{RawValue}";
                    return $@"\{Name}{RawValue}"; // If RawValue already has a leading +
                default:
                    return $@"\{Name}{RawValue}"; // ?
            }
        }

        public enum FsVariant
        {
            Absolute,
            Relative,
        }
    }

    /// <summary>
    /// Reset font scale
    /// </summary>
    /// <example>\fsc</example>
    public class Fsc : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fsc;

        /// <inheritdoc />
        public override string ToString() => $@"\{Name}";
    }

    /// <summary>
    /// Font scale in the x-direction
    /// </summary>
    /// <example>\fscx140</example>
    public class FscX : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FscX;

        /// <summary>
        /// Scale percentage
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Scale
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\fscx</c> tag
        /// </summary>
        /// <param name="value">Scale</param>
        public FscX(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\fscx</c> tag
        /// </summary>
        /// <param name="value">Scale</param>
        public FscX(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Font scale in the y-direction
    /// </summary>
    /// <example>\fscy140</example>
    public class FscY : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FscY;

        /// <summary>
        /// Scale percentage
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Scale
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\fscy</c> tag
        /// </summary>
        /// <param name="value">Scale</param>
        public FscY(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\fscy</c> tag
        /// </summary>
        /// <param name="value">Scale</param>
        public FscY(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Letter spacing
    /// </summary>
    /// <example>\fsp2</example>
    public class Fsp : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fsp;

        /// <summary>
        /// Spacing between characters in pixel width
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Space between characters
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\fsp</c> tag
        /// </summary>
        /// <param name="value">Spacing</param>
        public Fsp(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\fsp</c> tag
        /// </summary>
        /// <param name="value">Spacing</param>
        public Fsp(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Italics
    /// </summary>
    /// <example>\i1</example>
    public class I : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.I;

        /// <summary>
        /// If italics is enabled
        /// </summary>
        public bool? Value
        {
            get => RawValue?.ParseAssInt().Equals(1);
            set =>
                RawValue = value is not null
                    ? value is true
                        ? "1"
                        : "0"
                    : null;
        }

        /// <summary>
        /// Italics
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\i</c> tag
        /// </summary>
        /// <param name="value">If italics are to be enabled</param>
        public I(bool? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\i</c> tag
        /// </summary>
        /// <param name="value">If italics are to be enabled</param>
        public I(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Inverse clip to a section
    /// </summary>
    /// <example>\iclip(0,0,100,175)</example>
    public class IClip : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.IClip;

        /// <summary>
        /// x0 coordinate for rectangular clips
        /// </summary>
        public int X0
        {
            get => RawX0?.ParseAssInt() ?? 0;
            set => RawX0 = value.ToString();
        }

        /// <summary>
        /// y0 coordinate for rectangular clips
        /// </summary>
        public int Y0
        {
            get => RawY0?.ParseAssInt() ?? 0;
            set => RawY0 = value.ToString();
        }

        /// <summary>
        /// x1 coordinate for rectangular clips
        /// </summary>
        public int X1
        {
            get => RawX1?.ParseAssInt() ?? 0;
            set => RawX1 = value.ToString();
        }

        /// <summary>
        /// y1 coordinate for rectangular clips
        /// </summary>
        public int Y1
        {
            get => RawY1?.ParseAssInt() ?? 0;
            set => RawY1 = value.ToString();
        }

        /// <summary>
        /// Scale of a drawing clip
        /// </summary>
        public double Scale
        {
            get => RawScale?.ParseAssDouble() ?? 0;
            set => RawScale = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Drawing commands
        /// </summary>
        public string? Drawing { get; set; }

        /// <summary>
        /// x0 coordinate for rectangular clips
        /// </summary>
        public string? RawX0 { get; set; }

        /// <summary>
        /// y0 coordinate for rectangular clips
        /// </summary>
        public string? RawY0 { get; set; }

        /// <summary>
        /// x1 coordinate for rectangular clips
        /// </summary>
        public string? RawX1 { get; set; }

        /// <summary>
        /// y1 coordinate for rectangular clips
        /// </summary>
        public string? RawY1 { get; set; }

        /// <summary>
        /// Scale of a drawing clip
        /// </summary>
        public string? RawScale { get; set; }

        /// <summary>
        /// Type of clip
        /// </summary>
        public IClipVariant? Variant { get; set; }

        /// <summary>
        /// Create a rectangular <c>\iclip</c>
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public IClip(int x0, int y0, int x1, int y1)
        {
            Variant = IClipVariant.Rectangle;
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        /// <summary>
        /// Create a drawing <c>\iclip</c>
        /// </summary>
        /// <param name="drawing">Drawing commands</param>
        public IClip(string drawing)
        {
            Variant = IClipVariant.Drawing;
            Drawing = drawing;
        }

        /// <summary>
        /// Create a scaled drawing <c>\iclip</c>
        /// </summary>
        /// <param name="scale">Scale factor</param>
        /// <param name="drawing">Drawing commands</param>
        public IClip(double scale, string drawing)
        {
            Variant = IClipVariant.ScaledDrawing;
            Scale = scale;
            Drawing = drawing;
        }

        /// <summary>
        /// Create a rectangular <c>\iclip</c>
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public IClip(string x0, string y0, string x1, string y1)
        {
            Variant = IClipVariant.Rectangle;
            RawX0 = x0;
            RawY0 = y0;
            RawX1 = x1;
            RawY1 = y1;
        }

        /// <summary>
        /// Create a scaled drawing <c>\iclip</c>
        /// </summary>
        /// <param name="scale">Scale factor</param>
        /// <param name="drawing">Drawing commands</param>
        public IClip(string scale, string drawing)
        {
            Variant = IClipVariant.ScaledDrawing;
            RawScale = scale;
            Drawing = drawing;
        }

        /// <inheritdoc />
        public override string ToString() =>
            Variant switch
            {
                IClipVariant.Rectangle => $@"\{Name}({RawX0},{RawY0},{RawX1},{RawY1})",
                IClipVariant.Drawing => $@"\{Name}({Drawing})",
                IClipVariant.ScaledDrawing => $@"\{Name}({RawScale},{Drawing})",
                _ => throw new ArgumentOutOfRangeException(nameof(Variant), Variant, null),
            };

        /// <summary>
        /// Type of clip
        /// </summary>
        public enum IClipVariant
        {
            Rectangle,
            Drawing,
            ScaledDrawing,
        }
    }

    /// <summary>
    /// Karaoke
    /// </summary>
    /// <remarks>
    /// Color changes from secondary to primary instantly
    /// </remarks>
    /// <example>\k16</example>
    public class K : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.K;

        /// <summary>
        /// Duration in centiseconds
        /// </summary>
        public double? Duration
        {
            get => RawDuration?.ParseAssDouble() ?? 0;
            set => RawDuration = value?.ToString();
        }

        /// <summary>
        /// Duration
        /// </summary>
        public string? RawDuration { get; set; }

        /// <summary>
        /// Create a <c>\k</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public K(double? duration)
        {
            Duration = duration;
        }

        /// <summary>
        /// Create a <c>\k</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public K(string? duration)
        {
            RawDuration = duration;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    /// <summary>
    /// Karaoke
    /// </summary>
    /// <remarks>
    /// Color changes from secondary to primary in an animated
    /// sweep from left to right, over the full duration
    /// </remarks>
    /// <example>\kf16</example>
    /// <seealso cref="OverrideTag.KUpper"/>
    public class Kf : K
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Kf;

        /// <summary>
        /// Create a <c>\kf</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public Kf(double? duration)
            : base(duration) { }

        /// <summary>
        /// Create a <c>\kf</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public Kf(string? duration)
            : base(duration) { }

        /// <inheritdoc />
        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    /// <summary>
    /// Karaoke
    /// </summary>
    /// <remarks>
    /// Outline color goes from invisible to visible instantly
    /// </remarks>
    /// <example>\ko16</example>
    public class Ko : K
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Ko;

        /// <summary>
        /// Create a <c>\ko</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public Ko(double? duration)
            : base(duration) { }

        /// <summary>
        /// Create a <c>\ko</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public Ko(string? duration)
            : base(duration) { }

        /// <inheritdoc />
        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    /// <summary>
    /// Karaoke
    /// </summary>
    /// <remarks>
    /// Sets the start time of the subsequent syllable
    /// relative to the start time of the event. Syllable start
    /// times are implicitly determined as sum(previous).
    /// </remarks>
    /// <example>\kt16</example>
    public class Kt : K
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Kt;

        /// <summary>
        /// Create a <c>\kt</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public Kt(double? duration)
            : base(duration) { }

        /// <summary>
        /// Create a <c>\kt</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public Kt(string? duration)
            : base(duration) { }

        /// <inheritdoc />
        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    /// <summary>
    /// Karaoke
    /// </summary>
    /// <remarks>
    /// Color changes from secondary to primary in an animated
    /// sweep from left to right, over the full duration
    /// </remarks>
    /// <example>\K16</example>
    /// <seealso cref="OverrideTag.Kf"/>
    public class KUpper : K
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.KUpper;

        /// <summary>
        /// Create a <c>\K</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public KUpper(double? duration)
            : base(duration) { }

        /// <summary>
        /// Create a <c>\K</c> tag
        /// </summary>
        /// <param name="duration">Duration</param>
        public KUpper(string? duration)
            : base(duration) { }

        /// <inheritdoc />
        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    /// <summary>
    /// Linear movement
    /// </summary>
    /// <example>\move(100,200,100,700)</example>
    public class Move : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Move;

        /// <summary>
        /// Move variant
        /// </summary>
        public bool IsShortVariant { get; set; }

        /// <summary>
        /// Initial x position
        /// </summary>
        public double X1
        {
            get => RawX1?.ParseAssDouble() ?? 0;
            set => RawX1 = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Initial y position
        /// </summary>
        public double Y1
        {
            get => RawY1?.ParseAssDouble() ?? 0;
            set => RawY1 = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Final x position
        /// </summary>
        public double X2
        {
            get => RawX2?.ParseAssDouble() ?? 0;
            set => RawX2 = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Final y position
        /// </summary>
        public double Y2
        {
            get => RawY2?.ParseAssDouble() ?? 0;
            set => RawY2 = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Start time
        /// </summary>
        public int T1
        {
            get => RawT1?.ParseAssInt() ?? 0;
            set => RawT1 = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// End time
        /// </summary>
        public int T2
        {
            get => RawT2?.ParseAssInt() ?? 0;
            set => RawT2 = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Initial x position
        /// </summary>
        public string? RawX1 { get; set; }

        /// <summary>
        /// Initial y position
        /// </summary>
        public string? RawY1 { get; set; }

        /// <summary>
        /// Final x position
        /// </summary>
        public string? RawX2 { get; set; }

        /// <summary>
        /// Final y position
        /// </summary>
        public string? RawY2 { get; set; }

        /// <summary>
        /// Start time
        /// </summary>
        public string? RawT1 { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        public string? RawT2 { get; set; }

        /// <summary>
        /// Create a <c>\move</c> tag
        /// </summary>
        /// <param name="x1">Initial x position</param>
        /// <param name="y1">Initial y position</param>
        /// <param name="x2">Final x position</param>
        /// <param name="y2">Final y position</param>
        public Move(double x1, double y1, double x2, double y2)
        {
            IsShortVariant = true;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            T1 = 0;
            T2 = 0;
        }

        /// <summary>
        /// Create a <c>\move</c> tag
        /// </summary>
        /// <param name="x1">Initial x position</param>
        /// <param name="y1">Initial y position</param>
        /// <param name="x2">Final x position</param>
        /// <param name="y2">Final y position</param>
        /// <param name="t1">Start time</param>
        /// <param name="t2">End time</param>
        public Move(double x1, double y1, double x2, double y2, int t1, int t2)
        {
            IsShortVariant = false;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            T1 = t1;
            T2 = t2;
        }

        /// <summary>
        /// Create a <c>\move</c> tag
        /// </summary>
        /// <param name="x1">Initial x position</param>
        /// <param name="y1">Initial y position</param>
        /// <param name="x2">Final x position</param>
        /// <param name="y2">Final y position</param>
        public Move(string x1, string y1, string x2, string y2)
        {
            IsShortVariant = true;
            RawX1 = x1;
            RawY1 = y1;
            RawX2 = x2;
            RawY2 = y2;
            RawT1 = "0";
            RawT2 = "0";
        }

        /// <summary>
        /// Create a <c>\move</c> tag
        /// </summary>
        /// <param name="x1">Initial x position</param>
        /// <param name="y1">Initial y position</param>
        /// <param name="x2">Final x position</param>
        /// <param name="y2">Final y position</param>
        /// <param name="t1">Start time</param>
        /// <param name="t2">End time</param>
        public Move(string x1, string y1, string x2, string y2, string t1, string t2)
        {
            IsShortVariant = false;
            RawX1 = x1;
            RawY1 = y1;
            RawX2 = x2;
            RawY2 = y2;
            RawT1 = t1;
            RawT2 = t2;
        }

        /// <inheritdoc />
        public override string ToString() =>
            IsShortVariant
                ? $@"\{Name}({RawX1},{RawY1},{RawX2},{RawY2})"
                : $@"\{Name}({RawX1},{RawY1},{RawX2},{RawY2},{RawT1},{RawT2})";
    }

    /// <summary>
    /// Rotation origin
    /// </summary>
    /// <example>\org(100,300)</example>
    public class Org : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Org;

        /// <summary>
        /// X position
        /// </summary>
        public double X
        {
            get => RawX?.ParseAssDouble() ?? 0;
            set => RawX = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Y position
        /// </summary>
        public double Y
        {
            get => RawY?.ParseAssDouble() ?? 0;
            set => RawY = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// X position
        /// </summary>
        public string? RawX { get; set; }

        /// <summary>
        /// Y position
        /// </summary>
        public string? RawY { get; set; }

        /// <summary>
        /// Create an <c>\org</c> tag
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public Org(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Create an <c>\org</c> tag
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public Org(string x, string y)
        {
            RawX = x;
            RawY = y;
        }

        /// <inheritdoc />
        public override string ToString() => $@"\{Name}({RawX},{RawY})";
    }

    /// <summary>
    /// Drawing mode
    /// </summary>
    /// <example>\p1</example>
    /// <seealso cref="OverrideTag.Pbo"/>
    public class P : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.P;

        /// <summary>
        /// Drawing level
        /// </summary>
        /// <remarks>
        /// Off=0, on=n. Use increasing integers for nesting.
        /// </remarks>
        public int Level
        {
            get => RawLevel?.ParseAssInt() ?? 0;
            set => RawLevel = value.ToString();
        }

        /// <summary>
        /// Drawing level
        /// </summary>
        public string? RawLevel { get; set; }

        /// <summary>
        /// Create a <c>\p</c> tag
        /// </summary>
        /// <param name="level">Drawing level</param>
        public P(int level)
        {
            Level = level;
        }

        /// <summary>
        /// Create a <c>\p</c> tag
        /// </summary>
        /// <param name="level">Drawing level</param>
        public P(string level)
        {
            RawLevel = level;
        }

        /// <inheritdoc />
        public override string ToString() => $@"\{Name}{RawLevel}";
    }

    /// <summary>
    /// Baseline y-offset for drawings
    /// </summary>
    /// <seealso cref="OverrideTag.P"/>
    /// <example>\pbo-100</example>
    public class Pbo : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Pbo;

        /// <summary>
        /// Y-offset in pixels
        /// </summary>
        public double Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Y-offset
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\pbo</c> tag
        /// </summary>
        /// <param name="value">Y-offset</param>
        public Pbo(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\pbo</c> tag
        /// </summary>
        /// <param name="value">Y-offset</param>
        public Pbo(string value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() => $@"\{Name}{RawValue}";
    }

    /// <summary>
    /// Line position
    /// </summary>
    /// <example>\pos(100,500)</example>
    public class Pos : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Pos;

        /// <summary>
        /// X position
        /// </summary>
        public double X
        {
            get => RawX?.ParseAssDouble() ?? 0;
            set => RawX = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Y position
        /// </summary>
        public double Y
        {
            get => RawY?.ParseAssDouble() ?? 0;
            set => RawY = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// X position
        /// </summary>
        public string? RawX { get; set; }

        /// <summary>
        /// Y position
        /// </summary>
        public string? RawY { get; set; }

        /// <summary>
        /// Create a <c>\pos</c> tag
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public Pos(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Create a <c>\pos</c> tag
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public Pos(string x, string y)
        {
            RawX = x;
            RawY = y;
        }

        /// <inheritdoc />
        public override string ToString() => $@"\{Name}({RawX},{RawY})";
    }

    /// <summary>
    /// Wrap style
    /// </summary>
    /// <example>\q0</example>
    public class Q : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Q;

        /// <summary>
        /// Wrap style setting
        /// </summary>
        public int? Value
        {
            get => RawValue?.ParseAssInt() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Wrap style setting
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\q</c> tag
        /// </summary>
        /// <param name="value">Wrap style</param>
        public Q(int? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\q</c> tag
        /// </summary>
        /// <param name="value">Wrap style</param>
        public Q(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Reset style
    /// </summary>
    /// <param name="style">Style to reset to (optional)</param>
    /// <example>\r, \rStyleName</example>
    public class R(string? style) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.R;

        /// <summary>
        /// Name of the style to reset to
        /// </summary>
        public string? Style { get; set; } = style;

        /// <inheritdoc />
        public override string ToString() => Style is not null ? $@"\{Name}{Style}" : $@"\{Name}";
    }

    /// <summary>
    /// Strikethrough
    /// </summary>
    /// <example>\s1</example>
    public class S : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.S;

        /// <summary>
        /// If strikethrough is enabled
        /// </summary>
        public bool? Value
        {
            get => RawValue?.ParseAssInt().Equals(1);
            set =>
                RawValue = value is not null
                    ? value is true
                        ? "1"
                        : "0"
                    : null;
        }

        /// <summary>
        /// Strikethrough
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\s</c> tag
        /// </summary>
        /// <param name="value">If strikethrough is to be enabled</param>
        public S(bool? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\s</c> tag
        /// </summary>
        /// <param name="value">If strikethrough is to be enabled</param>
        public S(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Shadow distance
    /// </summary>
    /// <remarks>Controls both X and Y shadows</remarks>
    /// <example>\shad10</example>
    public class Shad : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Shad;

        /// <summary>
        /// Shadow depth
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Shadow depth
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\shad</c> tag
        /// </summary>
        /// <param name="value">Shadow depth</param>
        public Shad(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\shad</c> tag
        /// </summary>
        /// <param name="value">Shadow depth</param>
        public Shad(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Animated Transform
    /// </summary>
    /// <example>\t(0,5000,\fscx120\fscy140)</example>
    public class T : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.T;

        /// <summary>
        /// Transform variant
        /// </summary>
        public TransformVariant Variant { get; }

        /// <summary>
        /// Start time
        /// </summary>
        public int T1
        {
            get => RawT1?.ParseAssInt() ?? 0;
            set => RawT1 = value.ToString();
        }

        /// <summary>
        /// End time
        /// </summary>
        public int T2
        {
            get => RawT2?.ParseAssInt() ?? 0;
            set => RawT2 = value.ToString();
        }

        /// <summary>
        /// Acceleration multiplier
        /// </summary>
        public double Acceleration
        {
            get => RawAcceleration?.ParseAssDouble() ?? 0;
            set => RawAcceleration = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tags to apply during the transformation
        /// </summary>
        public List<OverrideTag> Tags { get; }

        /// <summary>
        /// Start time
        /// </summary>
        public string? RawT1 { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        public string? RawT2 { get; set; }

        /// <summary>
        /// Acceleration multiplier
        /// </summary>
        public string? RawAcceleration { get; set; }

        /// <summary>
        /// Create a <c>\t</c> tag
        /// </summary>
        /// <param name="tags">Tags to apply during the transformation</param>
        public T(List<OverrideTag> tags)
        {
            Variant = TransformVariant.BlockOnly;
            Tags = tags;
        }

        /// <summary>
        /// Create a <c>\t</c> tag
        /// </summary>
        /// <param name="acceleration">Acceleration multiplier</param>
        /// <param name="tags">Tags to apply during the transformation</param>
        public T(double acceleration, List<OverrideTag> tags)
        {
            Variant = TransformVariant.AccelerationOnly;
            Acceleration = acceleration;
            Tags = tags;
        }

        /// <summary>
        /// Create a <c>\t</c> tag
        /// </summary>
        /// <param name="t1">Start time</param>
        /// <param name="t2">End time</param>
        /// <param name="tags">Tags to apply during the transformation</param>
        public T(double t1, double t2, List<OverrideTag> tags)
        {
            Variant = TransformVariant.TimeOnly;
            T1 = Convert.ToInt32(Math.Truncate(t1));
            T2 = Convert.ToInt32(Math.Truncate(t2));
            Tags = tags;
        }

        /// <summary>
        /// Create a <c>\t</c> tag
        /// </summary>
        /// <param name="t1">Start time</param>
        /// <param name="t2">End time</param>
        /// <param name="acceleration">Acceleration multiplier</param>
        /// <param name="tags">Tags to apply during the transformation</param>
        public T(int t1, int t2, double acceleration, List<OverrideTag> tags)
        {
            Variant = TransformVariant.Full;
            T1 = t1;
            T2 = t2;
            Acceleration = acceleration;
            Tags = tags;
        }

        /// <summary>
        /// Create a <c>\t</c> tag
        /// </summary>
        /// <param name="acceleration">Acceleration multiplier</param>
        /// <param name="tags">Tags to apply during the transformation</param>
        public T(string acceleration, List<OverrideTag> tags)
        {
            Variant = TransformVariant.AccelerationOnly;
            RawAcceleration = acceleration;
            Tags = tags;
        }

        /// <summary>
        /// Create a <c>\t</c> tag
        /// </summary>
        /// <param name="t1">Start time</param>
        /// <param name="t2">End time</param>
        /// <param name="tags">Tags to apply during the transformation</param>
        public T(string t1, string t2, List<OverrideTag> tags)
        {
            Variant = TransformVariant.TimeOnly;
            RawT1 = t1;
            RawT2 = t2;
            Tags = tags;
        }

        /// <summary>
        /// Create a <c>\t</c> tag
        /// </summary>
        /// <param name="t1">Start time</param>
        /// <param name="t2">End time</param>
        /// <param name="acceleration">Acceleration multiplier</param>
        /// <param name="tags">Tags to apply during the transformation</param>
        public T(string t1, string t2, string acceleration, List<OverrideTag> tags)
        {
            Variant = TransformVariant.Full;
            RawT1 = t1;
            RawT2 = t2;
            RawAcceleration = acceleration;
            Tags = tags;
        }

        private string BlockString() => string.Join(string.Empty, Tags.Select(x => x.ToString()));

        /// <inheritdoc />
        public override string ToString() =>
            Variant switch
            {
                TransformVariant.BlockOnly => $@"\{Name}({BlockString()})",
                TransformVariant.AccelerationOnly => $@"\{Name}({RawAcceleration},{BlockString()})",
                TransformVariant.TimeOnly => $@"\{Name}({RawT1},{RawT2},{BlockString()})",
                TransformVariant.Full =>
                    $@"\{Name}({RawT1},{RawT2},{RawAcceleration},{BlockString()})",
                _ => throw new ArgumentOutOfRangeException(nameof(Variant), Variant, null),
            };

        public enum TransformVariant
        {
            BlockOnly,
            AccelerationOnly,
            TimeOnly,
            Full,
        }
    }

    /// <summary>
    /// Underline
    /// </summary>
    /// <example>\u1</example>
    public class U : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.U;

        /// <summary>
        /// If underline is enabled
        /// </summary>
        public bool? Value
        {
            get => RawValue?.ParseAssInt().Equals(1);
            set =>
                RawValue = value is not null
                    ? value is true
                        ? "1"
                        : "0"
                    : null;
        }

        /// <summary>
        /// Underline
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\u</c> tag
        /// </summary>
        /// <param name="value">If underline is enabled</param>
        public U(bool? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\u</c> tag
        /// </summary>
        /// <param name="value">If underline is enabled</param>
        public U(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Border size on the x-axis
    /// </summary>
    /// <example>\xbord2</example>
    /// <seealso cref="OverrideTag.Bord"/>
    /// <seealso cref="OverrideTag.YBord"/>
    public class XBord : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.XBord;

        /// <summary>
        /// Border thickness
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Border thickness
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create an <c>\xbord</c> tag
        /// </summary>
        /// <param name="value">Border thickness</param>
        public XBord(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create an <c>\xbord</c> tag
        /// </summary>
        /// <param name="value">Border thickness</param>
        public XBord(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Shadow depth on the x-axis
    /// </summary>
    /// <example>\xshad10</example>
    /// <seealso cref="OverrideTag.Shad"/>
    /// <seealso cref="OverrideTag.YShad"/>
    public class XShad : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.XShad;

        /// <summary>
        /// Shadow depth
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Shadow depth
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\xshad</c> tag
        /// </summary>
        /// <param name="value">X-shadow depth</param>
        public XShad(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\xshad</c> tag
        /// </summary>
        /// <param name="value">X-shadow depth</param>
        public XShad(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Border size on the y-axis
    /// </summary>
    /// <example>\ybord2</example>
    /// <seealso cref="OverrideTag.Bord"/>
    /// <seealso cref="OverrideTag.XBord"/>
    public class YBord : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.YBord;

        /// <summary>
        /// Border thickness
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Border thickness
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\ybord</c> tag
        /// </summary>
        /// <param name="value">Border thickness</param>
        public YBord(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\ybord</c> tag
        /// </summary>
        /// <param name="value">Border thickness</param>
        public YBord(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Shadow depth on the y-axis
    /// </summary>
    /// <example>\yshad10</example>
    /// <seealso cref="OverrideTag.Shad"/>
    /// <seealso cref="OverrideTag.XShad"/>
    public class YShad : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.YShad;

        /// <summary>
        /// Shadow depth
        /// </summary>
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        /// <summary>
        /// Shadow depth
        /// </summary>
        public string? RawValue { get; set; }

        /// <summary>
        /// Create a <c>\yshad</c> tag
        /// </summary>
        /// <param name="value">Y-shadow depth</param>
        public YShad(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <c>\yshad</c> tag
        /// </summary>
        /// <param name="value">Y-shadow depth</param>
        public YShad(string? value)
        {
            RawValue = value;
        }

        /// <inheritdoc />
        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// An unknown tag
    /// </summary>
    /// <param name="name">Name of the tag</param>
    /// <param name="args">Tag arguments</param>
    public class Unknown(string name, params string[] args) : OverrideTag
    {
        /// <inheritdoc />
        /// <inheritdoc />
        public override string Name { get; } = name;
        public string[] Args { get; } = args;

        /// <inheritdoc />
        public override string ToString() =>
            Args.Length switch
            {
                0 => $@"\{Name}",
                1 => $@"\{Name}{Args[0]}",
                _ => $@"\{Name}({string.Join(',', Args)})",
            };
    }
}
