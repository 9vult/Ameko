// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using AssCS.Overrides.Blocks;
using AssCS.Utilities;

namespace AssCS.Overrides;

/// <summary>
/// A component in an <see cref="OverrideBlock"/>
/// </summary>
public abstract class OverrideTag
{
    /// <summary>
    /// Name of the tag, excluding the backslash <c>\</c>
    /// </summary>
    public abstract string Name { get; }

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

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    /// <summary>
    /// Border size
    /// </summary>
    /// <example>\bord2</example>
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
    public class Fad : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fad;

        /// <summary>
        /// Fade-in duration in milliseconds
        /// </summary>
        public int T1
        {
            get => RawT1?.ParseAssInt() ?? 0;
            set => RawT1 = value.ToString();
        }

        /// <summary>
        /// Fade-out duration in milliseconds
        /// </summary>
        public int T2
        {
            get => RawT2?.ParseAssInt() ?? 0;
            set => RawT2 = value.ToString();
        }

        /// <summary>
        /// Fade-in duration
        /// </summary>
        public string? RawT1 { get; set; }

        /// <summary>
        /// Fade-out duration
        /// </summary>
        public string? RawT2 { get; set; }

        /// <summary>
        /// Create a <c>\fad</c> tag
        /// </summary>
        /// <param name="t1">Fade-in duration</param>
        /// <param name="t2">Fade-out duration</param>
        public Fad(int t1, int t2)
        {
            T1 = t1;
            T2 = t2;
        }

        /// <summary>
        /// Create a <c>\fad</c> tag
        /// </summary>
        /// <param name="t1">Fade-in duration</param>
        /// <param name="t2">Fade-out duration</param>
        public Fad(string t1, string t2)
        {
            RawT1 = t1;
            RawT2 = t2;
        }

        public override string ToString() => $@"\{Name}({RawT1},{RawT2})";
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
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="a3"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t3"></param>
        /// <param name="t4"></param>
        public Fade(int a1, int a2, int a3, int t1, int t2, int t3, int t4)
        {
            Alpha1 = a1;
            Alpha2 = a2;
            Alpha3 = a3;
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
        }

        /// <summary>
        /// Create a <c>\fade</c> tag
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
            RawAlpha1 = a1;
            RawAlpha2 = a2;
            RawAlpha3 = a3;
            RawT1 = t1;
            RawT2 = t2;
            RawT3 = t3;
            RawT4 = t4;
        }

        public override string ToString() =>
            $@"\{Name}({RawAlpha1},{RawAlpha2},{RawAlpha3},{RawT1},{RawT2},{RawT3},{RawT4})";
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

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Fe : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fe;

        public int? Value
        {
            get => RawValue?.ParseAssInt() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public Fe(int? value)
        {
            Value = value;
        }

        public Fe(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Fn(string? value) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fn;

        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fr : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fr;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public Fr(double? value)
        {
            Value = value;
        }

        public Fr(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class FrX : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FrX;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public FrX(double? value)
        {
            Value = value;
        }

        public FrX(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class FrY : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FrY;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public FrY(double? value)
        {
            Value = value;
        }

        public FrY(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class FrZ : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FrZ;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public FrZ(double? value)
        {
            Value = value;
        }

        public FrZ(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Fs : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fs;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }
        public FsVariant Variant { get; set; }

        public Fs(double? value, FsVariant variant)
        {
            Value = value;
            Variant = variant;
        }

        public Fs(string? value, FsVariant variant)
        {
            RawValue = value;
            Variant = variant;
        }

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

    public class Fsc : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fsc;

        public override string ToString() => $@"\{Name}";
    }

    public class FscX : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FscX;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public FscX(double? value)
        {
            Value = value;
        }

        public FscX(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class FscY : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.FscY;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public FscY(double? value)
        {
            Value = value;
        }

        public FscY(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Fsp : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Fsp;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public Fsp(double? value)
        {
            Value = value;
        }

        public Fsp(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class I : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.I;

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
        public string? RawValue { get; set; }

        public I(bool? value)
        {
            Value = value;
        }

        public I(string? value)
        {
            RawValue = value;
        }

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

    public class K : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.K;

        public double? Duration
        {
            get => RawDuration?.ParseAssDouble() ?? 0;
            set => RawDuration = value?.ToString();
        }

        public string? RawDuration { get; set; }

        public K(double? duration)
        {
            Duration = duration;
        }

        public K(string? duration)
        {
            RawDuration = duration;
        }

        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    public class Kf : K
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Kf;

        public Kf(double? duration)
            : base(duration) { }

        public Kf(string? duration)
            : base(duration) { }

        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    public class Ko : K
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Ko;

        public Ko(double? duration)
            : base(duration) { }

        public Ko(string? duration)
            : base(duration) { }

        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    public class Kt : K
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Kt;

        public Kt(double? duration)
            : base(duration) { }

        public Kt(string? duration)
            : base(duration) { }

        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    public class KUpper : K
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.KUpper;

        public KUpper(double? duration)
            : base(duration) { }

        public KUpper(string? duration)
            : base(duration) { }

        public override string ToString() =>
            RawDuration is not null ? $@"\{Name}{RawDuration}" : $@"\{Name}";
    }

    public class Move : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Move;

        public bool IsShortVariant { get; }

        public double X1
        {
            get => RawX1?.ParseAssDouble() ?? 0;
            set => RawX1 = value.ToString(CultureInfo.InvariantCulture);
        }
        public double Y1
        {
            get => RawY1?.ParseAssDouble() ?? 0;
            set => RawY1 = value.ToString(CultureInfo.InvariantCulture);
        }
        public double X2
        {
            get => RawX2?.ParseAssDouble() ?? 0;
            set => RawX2 = value.ToString(CultureInfo.InvariantCulture);
        }
        public double Y2
        {
            get => RawY2?.ParseAssDouble() ?? 0;
            set => RawY2 = value.ToString(CultureInfo.InvariantCulture);
        }
        public int T1
        {
            get => RawT1?.ParseAssInt() ?? 0;
            set => RawT1 = value.ToString(CultureInfo.InvariantCulture);
        }

        public int T2
        {
            get => RawT2?.ParseAssInt() ?? 0;
            set => RawT2 = value.ToString(CultureInfo.InvariantCulture);
        }

        public string? RawX1 { get; set; }
        public string? RawY1 { get; set; }
        public string? RawX2 { get; set; }
        public string? RawY2 { get; set; }
        public string? RawT1 { get; set; }
        public string? RawT2 { get; set; }

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

        public override string ToString() =>
            IsShortVariant
                ? $@"\{Name}({RawX1},{RawY1},{RawX2},{RawY2})"
                : $@"\{Name}({RawX1},{RawY1},{RawX2},{RawY2},{RawT1},{RawT2})";
    }

    public class Org : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Org;

        public double X
        {
            get => RawX?.ParseAssDouble() ?? 0;
            set => RawX = value.ToString(CultureInfo.InvariantCulture);
        }
        public double Y
        {
            get => RawY?.ParseAssDouble() ?? 0;
            set => RawY = value.ToString(CultureInfo.InvariantCulture);
        }
        public string? RawX { get; set; }
        public string? RawY { get; set; }

        public Org(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Org(string x, string y)
        {
            RawX = x;
            RawY = y;
        }

        public override string ToString() => $@"\{Name}({RawX},{RawY})";
    }

    public class P : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.P;

        public int Level
        {
            get => RawLevel?.ParseAssInt() ?? 0;
            set => RawLevel = value.ToString();
        }

        public string? RawLevel { get; set; }

        public P(int level)
        {
            Level = level;
        }

        public P(string level)
        {
            RawLevel = level;
        }

        public override string ToString() => $@"\{Name}{RawLevel}";
    }

    public class Pbo : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Pbo;

        public double Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value.ToString(CultureInfo.InvariantCulture);
        }

        public string? RawValue { get; set; }

        public Pbo(double value)
        {
            Value = value;
        }

        public Pbo(string value)
        {
            RawValue = value;
        }

        public override string ToString() => $@"\{Name}{RawValue}";
    }

    public class Pos : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Pos;

        public double X
        {
            get => RawX?.ParseAssDouble() ?? 0;
            set => RawX = value.ToString(CultureInfo.InvariantCulture);
        }
        public double Y
        {
            get => RawY?.ParseAssDouble() ?? 0;
            set => RawY = value.ToString(CultureInfo.InvariantCulture);
        }
        public string? RawX { get; set; }
        public string? RawY { get; set; }

        public Pos(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Pos(string x, string y)
        {
            RawX = x;
            RawY = y;
        }

        public override string ToString() => $@"\{Name}({RawX},{RawY})";
    }

    public class Q : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Q;

        public int? Value
        {
            get => RawValue?.ParseAssInt() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public Q(int? value)
        {
            Value = value;
        }

        public Q(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class R(string? style) : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.R;

        public string? Style { get; set; } = style;

        public override string ToString() => Style is not null ? $@"\{Name}{Style}" : $@"\{Name}";
    }

    public class S : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.S;
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
        public string? RawValue { get; set; }

        public S(bool? value)
        {
            Value = value;
        }

        public S(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Shad : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.Shad;
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public Shad(double? value)
        {
            Value = value;
        }

        public Shad(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class T : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.T;

        public TransformVariant Variant { get; }

        public int T1
        {
            get => RawT1?.ParseAssInt() ?? 0;
            set => RawT1 = value.ToString();
        }
        public int T2
        {
            get => RawT2?.ParseAssInt() ?? 0;
            set => RawT2 = value.ToString();
        }
        public double Acceleration
        {
            get => RawAcceleration?.ParseAssDouble() ?? 0;
            set => RawAcceleration = value.ToString(CultureInfo.InvariantCulture);
        }
        public List<OverrideTag> Block { get; }

        public string? RawT1 { get; set; }
        public string? RawT2 { get; set; }
        public string? RawAcceleration { get; set; }

        public T(List<OverrideTag> block)
        {
            Variant = TransformVariant.BlockOnly;
            Block = block;
        }

        public T(double acceleration, List<OverrideTag> block)
        {
            Variant = TransformVariant.AccelerationOnly;
            Acceleration = acceleration;
            Block = block;
        }

        public T(double t1, double t2, List<OverrideTag> block)
        {
            Variant = TransformVariant.TimeOnly;
            T1 = Convert.ToInt32(Math.Truncate(t1));
            T2 = Convert.ToInt32(Math.Truncate(t2));
            Block = block;
        }

        public T(int t1, int t2, double acceleration, List<OverrideTag> block)
        {
            Variant = TransformVariant.Full;
            T1 = t1;
            T2 = t2;
            Acceleration = acceleration;
            Block = block;
        }

        public T(string acceleration, List<OverrideTag> block)
        {
            Variant = TransformVariant.AccelerationOnly;
            RawAcceleration = acceleration;
            Block = block;
        }

        public T(string t1, string t2, List<OverrideTag> block)
        {
            Variant = TransformVariant.TimeOnly;
            RawT1 = t1;
            RawT2 = t2;
            Block = block;
        }

        public T(string t1, string t2, string acceleration, List<OverrideTag> block)
        {
            Variant = TransformVariant.Full;
            RawT1 = t1;
            RawT2 = t2;
            RawAcceleration = acceleration;
            Block = block;
        }

        private string BlockString() => string.Join(string.Empty, Block.Select(x => x.ToString()));

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

    public class U : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.U;
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
        public string? RawValue { get; set; }

        public U(bool? value)
        {
            Value = value;
        }

        public U(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class XBord : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.XBord;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public XBord(double? value)
        {
            Value = value;
        }

        public XBord(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class XShad : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.XShad;
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public XShad(double? value)
        {
            Value = value;
        }

        public XShad(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class YBord : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.YBord;
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public YBord(double? value)
        {
            Value = value;
        }

        public YBord(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class YShad : OverrideTag
    {
        /// <inheritdoc />
        public override string Name => OverrideTags.YShad;
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public YShad(double? value)
        {
            Value = value;
        }

        public YShad(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Unknown(string name, params string[] args) : OverrideTag
    {
        /// <inheritdoc />
        /// <inheritdoc />
        public override string Name { get; } = name;
        public string[] Args { get; } = args;

        public override string ToString() =>
            Args.Length switch
            {
                0 => $@"\{Name}",
                1 => $@"\{Name}{Args[0]}",
                _ => $@"\{Name}({string.Join(',', Args)})",
            };
    }
}
