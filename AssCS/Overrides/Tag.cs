// SPDX-License-Identifier: MPL-2.0

using AssCS.Overrides.Blocks;

namespace AssCS.Overrides;

public abstract class Tag
{
    public abstract string Name { get; }

    public class A(int value) : Tag
    {
        public override string Name => Tags.A;
        public int Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class A1(int value) : Tag
    {
        public override string Name => Tags.A1;
        public int Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class A2(int value) : Tag
    {
        public override string Name => Tags.A2;
        public int Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class A3(int value) : Tag
    {
        public override string Name => Tags.A3;
        public int Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class A4(int value) : Tag
    {
        public override string Name => Tags.A4;
        public int Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class Alpha(int? value) : Tag
    {
        public override string Name => Tags.Alpha;
        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class An(int value) : Tag
    {
        public override string Name => Tags.An;
        public int Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class B(bool value) : Tag
    {
        public override string Name => Tags.B;
        public bool Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{(Value ? 1 : 0)}";
    }

    public class Be(int? value) : Tag
    {
        public override string Name => Tags.Be;
        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Blur(int? value) : Tag
    {
        public override string Name => Tags.Blur;
        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Bord(int? value) : Tag
    {
        public override string Name => Tags.Bord;
        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class C(Color? value) : Tag
    {
        public override string Name => Tags.C;
        public Color? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{Value.AsOverrideColor()}" : $@"\{Name}";
    }

    public class C1(Color? value) : Tag
    {
        public override string Name => Tags.C1;
        public Color? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{Value.AsOverrideColor()}" : $@"\{Name}";
    }

    public class C2(Color? value) : Tag
    {
        public override string Name => Tags.C2;
        public Color? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{Value.AsOverrideColor()}" : $@"\{Name}";
    }

    public class C3(Color? value) : Tag
    {
        public override string Name => Tags.C3;
        public Color? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{Value.AsOverrideColor()}" : $@"\{Name}";
    }

    public class C4(Color? value) : Tag
    {
        public override string Name => Tags.C4;
        public Color? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{Value.AsOverrideColor()}" : $@"\{Name}";
    }

    public class Clip(int x0, int y0, int x1, int y1) : Tag
    {
        public override string Name => Tags.Clip;
        public int X0 { get; set; } = x0;
        public int Y0 { get; set; } = y0;
        public int X1 { get; set; } = x1;
        public int Y1 { get; set; } = y1;

        public override string ToString() => $@"\{Name}({X0},{Y0},{X1},{Y1})";
    }

    public class Fad : Tag
    {
        public override string Name => Tags.Fad;

        public bool IsShortVariant { get; }

        public int A1 { get; set; } = 0xFF;
        public int A2 { get; set; } = 0;
        public int A3 { get; set; } = 0xFF;
        public int T1 { get; set; } = -1;
        public int T2 { get; set; }
        public int T3 { get; set; }
        public int T4 { get; set; } = -1;

        public Fad(int t2, int t3)
        {
            IsShortVariant = true;
            T2 = t2;
            T3 = t3;
        }

        public Fad(int a1, int a2, int a3, int t1, int t2, int t3, int t4)
        {
            IsShortVariant = false;
            A1 = a1;
            A2 = a2;
            A3 = a3;
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
        }

        public override string ToString() =>
            IsShortVariant
                ? $@"\{Name}({T2},{T3})"
                : $@"\{Name}({A1},{A2},{A3},{T1},{T2},{T3},{T4})";
    }

    public class Fade : Fad
    {
        public override string Name => Tags.Fade;

        /// <inheritdoc />
        public Fade(int t2, int t3)
            : base(t2, t3) { }

        /// <inheritdoc />
        public Fade(int a1, int a2, int a3, int t1, int t2, int t3, int t4)
            : base(a1, a2, a3, t1, t2, t3, t4) { }
    }

    public class FaX(double value) : Tag
    {
        public override string Name => Tags.FaX;

        public double Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class FaY(double value) : Tag
    {
        public override string Name => Tags.FaY;

        public double Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class Fe(int? value) : Tag
    {
        public override string Name => Tags.Fe;

        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fn(string? value) : Tag
    {
        public override string Name => Tags.Fn;

        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fr(double? value) : Tag
    {
        public override string Name => Tags.Fr;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FrX(double? value) : Tag
    {
        public override string Name => Tags.FrX;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FrY(double? value) : Tag
    {
        public override string Name => Tags.FrY;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FrZ(double? value) : Tag
    {
        public override string Name => Tags.FrZ;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fs(double? value) : Tag
    {
        public override string Name => Tags.Fs;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fsc : Tag
    {
        public override string Name => Tags.Fsc;

        public override string ToString() => $@"\{Name}";
    }

    public class FscX(double? value) : Tag
    {
        public override string Name => Tags.FscX;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FscY(double? value) : Tag
    {
        public override string Name => Tags.FscY;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fsp(double? value) : Tag
    {
        public override string Name => Tags.Fsp;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class I(bool value) : Tag
    {
        public override string Name => Tags.I;
        public bool Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{(Value ? 1 : 0)}";
    }

    public class IClip(int x0, int y0, int x1, int y1) : Tag
    {
        public override string Name => Tags.IClip;
        public int X0 { get; set; } = x0;
        public int Y0 { get; set; } = y0;
        public int X1 { get; set; } = x1;
        public int Y1 { get; set; } = y1;

        public override string ToString() => $@"\{Name}({X0},{Y0},{X1},{Y1})";
    }

    public class K(double? value) : Tag
    {
        public override string Name => Tags.K;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Kf(double? value) : Tag
    {
        public override string Name => Tags.Kf;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Ko(double? value) : Tag
    {
        public override string Name => Tags.Ko;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Kt(double? value) : Tag
    {
        public override string Name => Tags.Kt;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class KUpper(double? value) : Tag
    {
        public override string Name => Tags.KUpper;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Move : Tag
    {
        public override string Name => Tags.Move;

        public bool IsShortVariant { get; }

        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public int T1 { get; set; } = 0;
        public int T2 { get; set; } = 0;

        public Move(double x1, double y1, double x2, double y2)
        {
            IsShortVariant = true;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
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

        public override string ToString() =>
            IsShortVariant
                ? $@"\{Name}({X1},{Y1},{X2},{Y2})"
                : $@"\{Name}({X1},{Y1},{X2},{Y2},{T1},{T2})";
    }

    public class Org(double x, double y) : Tag
    {
        public override string Name => Tags.Org;

        public double X { get; set; } = x;
        public double Y { get; set; } = y;

        public override string ToString() => $@"\{Name}({X},{Y})";
    }

    public class P(int value) : Tag
    {
        public override string Name => Tags.P;

        public int Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class Pbo(double value) : Tag
    {
        public override string Name => Tags.Pbo;

        public double Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class Pos(double x, double y) : Tag
    {
        public override string Name => Tags.Pos;

        public double X { get; set; } = x;
        public double Y { get; set; } = y;

        public override string ToString() => $@"\{Name}({X},{Y})";
    }

    public class Q(int? value) : Tag
    {
        public override string Name => Tags.Q;

        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class R(string? style) : Tag
    {
        public override string Name => Tags.R;

        public string? Style { get; set; } = style;

        public override string ToString() => Style is not null ? $@"\{Name}{Style}" : $@"\{Name}";
    }

    public class S(bool value) : Tag
    {
        public override string Name => Tags.S;
        public bool Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{(Value ? 1 : 0)}";
    }

    public class Shad(double? value) : Tag
    {
        public override string Name => Tags.Shad;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class T : Tag
    {
        public override string Name => Tags.T;

        public bool IsShortVariant { get; }

        public int T1 { get; set; }
        public int T2 { get; set; }
        public double Acceleration { get; set; } = 1;
        public OverrideBlock Block { get; }

        public T(double t1, double t2, OverrideBlock block)
        {
            IsShortVariant = true;
            T1 = Convert.ToInt32(Math.Truncate(t1));
            T2 = Convert.ToInt32(Math.Truncate(t2));
            Block = block;
        }

        public T(int t1, int t2, double acceleration, OverrideBlock block)
        {
            IsShortVariant = false;
            T1 = t1;
            T2 = t2;
            Acceleration = acceleration;
            Block = block;
        }

        public override string ToString() =>
            IsShortVariant
                ? $@"\{Name}({T1},{T2},{Block.SubBlockText})"
                : $@"\{Name}({T1},{T2},{Acceleration},{Block.SubBlockText})";
    }

    public class U(bool value) : Tag
    {
        public override string Name => Tags.U;
        public bool Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{(Value ? 1 : 0)}";
    }

    public class XBord(double? value) : Tag
    {
        public override string Name => Tags.XBord;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class XShad(double? value) : Tag
    {
        public override string Name => Tags.XShad;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class YBord(double? value) : Tag
    {
        public override string Name => Tags.YBord;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class YShad(double? value) : Tag
    {
        public override string Name => Tags.YShad;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Unknown(string name, params string[] args) : Tag
    {
        /// <inheritdoc />
        public override string Name { get; } = name;
        public string[] Args { get; } = args;
    }
}
