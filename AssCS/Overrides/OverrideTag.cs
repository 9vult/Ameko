// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Overrides;

public abstract class OverrideTag
{
    public abstract string Name { get; }

    public class A(int? value) : OverrideTag
    {
        public override string Name => OverrideTags.A;
        public int? Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class A1(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.A1;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class A2(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.A2;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class A3(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.A3;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class A4(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.A4;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Alpha(string? alpha) : OverrideTag
    {
        public override string Name => OverrideTags.Alpha;
        public string? Value { get; set; } = alpha;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class An(int? value) : OverrideTag
    {
        public override string Name => OverrideTags.An;
        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class B(bool? value) : OverrideTag
    {
        public override string Name => OverrideTags.B;
        public bool? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{(Value is true ? 1 : 0)}" : $@"\{Name}";
    }

    public class Be(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.Be;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Blur(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.Blur;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Bord(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.Bord;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class C(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.C;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class C1(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.C1;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class C2(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.C2;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class C3(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.C3;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class C4(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.C4;
        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Clip(int x0, int y0, int x1, int y1) : OverrideTag
    {
        public override string Name => OverrideTags.Clip;
        public int X0 { get; set; } = x0;
        public int Y0 { get; set; } = y0;
        public int X1 { get; set; } = x1;
        public int Y1 { get; set; } = y1;

        public override string ToString() => $@"\{Name}({X0},{Y0},{X1},{Y1})";
    }

    public class Fad(int t2, int t3) : OverrideTag
    {
        public override string Name => OverrideTags.Fad;

        public int T2 { get; set; } = t2;
        public int T3 { get; set; } = t3;

        public override string ToString() => $@"\{Name}({T2},{T3})";
    }

    public class Fade(int a1, int a2, int a3, int t1, int t2, int t3, int t4) : OverrideTag
    {
        public override string Name => OverrideTags.Fade;

        public int Alpha1 { get; set; } = a1;
        public int Alpha2 { get; set; } = a2;
        public int Alpha3 { get; set; } = a3;
        public int T1 { get; set; } = t1;
        public int T2 { get; set; } = t2;
        public int T3 { get; set; } = t3;
        public int T4 { get; set; } = t4;

        public override string ToString() =>
            $@"\{Name}({Alpha1},{Alpha2},{Alpha3},{T1},{T2},{T3},{T4})";
    }

    public class FaX(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.FaX;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FaY(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.FaY;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fe(int? value) : OverrideTag
    {
        public override string Name => OverrideTags.Fe;

        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fn(string? value) : OverrideTag
    {
        public override string Name => OverrideTags.Fn;

        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fr(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.Fr;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FrX(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.FrX;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FrY(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.FrY;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FrZ(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.FrZ;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fs(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.Fs;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fsc : OverrideTag
    {
        public override string Name => OverrideTags.Fsc;

        public override string ToString() => $@"\{Name}";
    }

    public class FscX(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.FscX;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class FscY(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.FscY;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fsp(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.Fsp;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class I(bool? value) : OverrideTag
    {
        public override string Name => OverrideTags.I;
        public bool? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{(Value is true ? 1 : 0)}" : $@"\{Name}";
    }

    public class IClip(int x0, int y0, int x1, int y1) : OverrideTag
    {
        public override string Name => OverrideTags.IClip;
        public int X0 { get; set; } = x0;
        public int Y0 { get; set; } = y0;
        public int X1 { get; set; } = x1;
        public int Y1 { get; set; } = y1;

        public override string ToString() => $@"\{Name}({X0},{Y0},{X1},{Y1})";
    }

    public class K(double? duration) : OverrideTag
    {
        public override string Name => OverrideTags.K;

        public double? Duration { get; set; } = duration;

        public override string ToString() =>
            Duration is not null ? $@"\{Name}{Duration}" : $@"\{Name}";
    }

    public class Kf(double? duration) : K(duration)
    {
        public override string Name => OverrideTags.Kf;

        public override string ToString() =>
            Duration is not null ? $@"\{Name}{Duration}" : $@"\{Name}";
    }

    public class Ko(double? duration) : K(duration)
    {
        public override string Name => OverrideTags.Ko;

        public override string ToString() =>
            Duration is not null ? $@"\{Name}{Duration}" : $@"\{Name}";
    }

    public class Kt(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.Kt;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class KUpper(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.KUpper;

        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Move : OverrideTag
    {
        public override string Name => OverrideTags.Move;

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

    public class Org(double x, double y) : OverrideTag
    {
        public override string Name => OverrideTags.Org;

        public double X { get; set; } = x;
        public double Y { get; set; } = y;

        public override string ToString() => $@"\{Name}({X},{Y})";
    }

    public class P(int level) : OverrideTag
    {
        public override string Name => OverrideTags.P;

        public int Level { get; set; } = level;

        public override string ToString() => $@"\{Name}{Level}";
    }

    public class Pbo(double value) : OverrideTag
    {
        public override string Name => OverrideTags.Pbo;

        public double Value { get; set; } = value;

        public override string ToString() => $@"\{Name}{Value}";
    }

    public class Pos(double x, double y) : OverrideTag
    {
        public override string Name => OverrideTags.Pos;

        public double X { get; set; } = x;
        public double Y { get; set; } = y;

        public override string ToString() => $@"\{Name}({X},{Y})";
    }

    public class Q(int? value) : OverrideTag
    {
        public override string Name => OverrideTags.Q;

        public int? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class R(string? style) : OverrideTag
    {
        public override string Name => OverrideTags.R;

        public string? Style { get; set; } = style;

        public override string ToString() => Style is not null ? $@"\{Name}{Style}" : $@"\{Name}";
    }

    public class S(bool? value) : OverrideTag
    {
        public override string Name => OverrideTags.S;
        public bool? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{(Value is true ? 1 : 0)}" : $@"\{Name}";
    }

    public class Shad(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.Shad;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class T : OverrideTag
    {
        public override string Name => OverrideTags.T;

        public TransformVariant Variant { get; }

        public int T1 { get; set; }
        public int T2 { get; set; }
        public double Acceleration { get; set; }
        public List<OverrideTag> Block { get; }

        public T(List<OverrideTag> block)
        {
            Variant = TransformVariant.BlockOnly;
            Block = block;
        }

        public T(double acceleration, List<OverrideTag> block)
        {
            Variant = TransformVariant.AccelerationOnly;
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

        private string BlockString() => string.Join(string.Empty, Block.Select(x => x.ToString()));

        public override string ToString() =>
            Variant switch
            {
                TransformVariant.BlockOnly => $@"\{Name}({BlockString()})",
                TransformVariant.AccelerationOnly => $@"\{Name}({Acceleration},{BlockString()})",
                TransformVariant.TimeOnly => $@"\{Name}({T1},{T2},{BlockString()})",
                TransformVariant.Full => $@"\{Name}({T1},{T2},{Acceleration},{BlockString()})",
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

    public class U(bool? value) : OverrideTag
    {
        public override string Name => OverrideTags.U;
        public bool? Value { get; set; } = value;

        public override string ToString() =>
            Value is not null ? $@"\{Name}{(Value is true ? 1 : 0)}" : $@"\{Name}";
    }

    public class XBord(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.XBord;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class XShad(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.XShad;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class YBord(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.YBord;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class YShad(double? value) : OverrideTag
    {
        public override string Name => OverrideTags.YShad;
        public double? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Unknown(string name, params string[] args) : OverrideTag
    {
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
