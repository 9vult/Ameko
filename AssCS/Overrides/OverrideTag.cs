// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using AssCS.Utilities;

namespace AssCS.Overrides;

public abstract class OverrideTag
{
    public abstract string Name { get; }

    public class A : OverrideTag
    {
        public override string Name => OverrideTags.A;
        public int? Value
        {
            get => RawValue?.ParseAssInt() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public A(int? value)
        {
            Value = value;
        }

        public A(string? value)
        {
            RawValue = value;
        }

        public override string ToString() => $@"\{Name}{RawValue}";
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

    public class An : OverrideTag
    {
        public override string Name => OverrideTags.An;
        public int? Value
        {
            get => RawValue?.ParseAssInt() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public An(int? value)
        {
            Value = value;
        }

        public An(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class B : OverrideTag
    {
        public override string Name => OverrideTags.B;

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

        public B(bool? value)
        {
            Value = value;
        }

        public B(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Be : OverrideTag
    {
        public override string Name => OverrideTags.Be;
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public Be(double? value)
        {
            Value = value;
        }

        public Be(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Blur : OverrideTag
    {
        public override string Name => OverrideTags.Blur;
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public Blur(double? value)
        {
            Value = value;
        }

        public Blur(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Bord : OverrideTag
    {
        public override string Name => OverrideTags.Bord;
        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public Bord(double? value)
        {
            Value = value;
        }

        public Bord(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
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

    public class Clip : OverrideTag
    {
        public override string Name => OverrideTags.Clip;
        public int X0
        {
            get => RawX0?.ParseAssInt() ?? 0;
            set => RawX0 = value.ToString();
        }

        public int Y0
        {
            get => RawY0?.ParseAssInt() ?? 0;
            set => RawY0 = value.ToString();
        }
        public int X1
        {
            get => RawX1?.ParseAssInt() ?? 0;
            set => RawX1 = value.ToString();
        }
        public int Y1
        {
            get => RawY1?.ParseAssInt() ?? 0;
            set => RawY1 = value.ToString();
        }

        public string? RawX0 { get; set; }
        public string? RawY0 { get; set; }
        public string? RawX1 { get; set; }
        public string? RawY1 { get; set; }

        public Clip(int x0, int y0, int x1, int y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        public Clip(string x0, string y0, string x1, string y1)
        {
            RawX0 = x0;
            RawY0 = y0;
            RawX1 = x1;
            RawY1 = y1;
        }

        public override string ToString() => $@"\{Name}({RawX0},{RawY0},{RawX1},{RawY1})";
    }

    public class Fad : OverrideTag
    {
        public override string Name => OverrideTags.Fad;

        public int T2
        {
            get => RawT2?.ParseAssInt() ?? 0;
            set => RawT2 = value.ToString();
        }
        public int T3
        {
            get => RawT3?.ParseAssInt() ?? 0;
            set => RawT3 = value.ToString();
        }

        public string? RawT2 { get; set; }
        public string? RawT3 { get; set; }

        public Fad(int t2, int t3)
        {
            T2 = t2;
            T3 = t3;
        }

        public Fad(string t2, string t3)
        {
            RawT2 = t2;
            RawT3 = t3;
        }

        public override string ToString() => $@"\{Name}({RawT2},{RawT3})";
    }

    public class Fade : OverrideTag
    {
        public override string Name => OverrideTags.Fade;

        public int Alpha1
        {
            get => RawAlpha1?.ParseAssInt() ?? 0;
            set => RawAlpha1 = value.ToString();
        }
        public int Alpha2
        {
            get => RawAlpha2?.ParseAssInt() ?? 0;
            set => RawAlpha2 = value.ToString();
        }
        public int Alpha3
        {
            get => RawAlpha3?.ParseAssInt() ?? 0;
            set => RawAlpha3 = value.ToString();
        }
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
        public int T3
        {
            get => RawT3?.ParseAssInt() ?? 0;
            set => RawT3 = value.ToString();
        }
        public int T4
        {
            get => RawT4?.ParseAssInt() ?? 0;
            set => RawT4 = value.ToString();
        }

        public string? RawAlpha1 { get; set; }
        public string? RawAlpha2 { get; set; }
        public string? RawAlpha3 { get; set; }
        public string? RawT1 { get; set; }
        public string? RawT2 { get; set; }
        public string? RawT3 { get; set; }
        public string? RawT4 { get; set; }

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

    public class FaX : OverrideTag
    {
        public override string Name => OverrideTags.FaX;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public FaX(double? value)
        {
            Value = value;
        }

        public FaX(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class FaY : OverrideTag
    {
        public override string Name => OverrideTags.FaY;

        public double? Value
        {
            get => RawValue?.ParseAssDouble() ?? 0;
            set => RawValue = value?.ToString();
        }

        public string? RawValue { get; set; }

        public FaY(double? value)
        {
            Value = value;
        }

        public FaY(string? value)
        {
            RawValue = value;
        }

        public override string ToString() =>
            RawValue is not null ? $@"\{Name}{RawValue}" : $@"\{Name}";
    }

    public class Fe : OverrideTag
    {
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
        public override string Name => OverrideTags.Fn;

        public string? Value { get; set; } = value;

        public override string ToString() => Value is not null ? $@"\{Name}{Value}" : $@"\{Name}";
    }

    public class Fr : OverrideTag
    {
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
                    return $@"\{Name}+{RawValue}";
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
        public override string Name => OverrideTags.Fsc;

        public override string ToString() => $@"\{Name}";
    }

    public class FscX : OverrideTag
    {
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

    public class IClip : OverrideTag
    {
        public override string Name => OverrideTags.IClip;

        public int X0
        {
            get => RawX0?.ParseAssInt() ?? 0;
            set => RawX0 = value.ToString();
        }
        public int Y0
        {
            get => RawY0?.ParseAssInt() ?? 0;
            set => RawY0 = value.ToString();
        }
        public int X1
        {
            get => RawX1?.ParseAssInt() ?? 0;
            set => RawX1 = value.ToString();
        }
        public int Y1
        {
            get => RawY1?.ParseAssInt() ?? 0;
            set => RawY1 = value.ToString();
        }

        public string? RawX0 { get; set; }
        public string? RawX1 { get; set; }
        public string? RawY0 { get; set; }
        public string? RawY1 { get; set; }

        public IClip(int x0, int y0, int x1, int y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        public IClip(string x0, string y0, string x1, string y1)
        {
            RawX0 = x0;
            RawX1 = x1;
            RawY0 = y0;
            RawY1 = y1;
        }

        public override string ToString() => $@"\{Name}({RawX0},{RawY0},{RawX1},{RawY1})";
    }

    public class K : OverrideTag
    {
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
        public override string Name => OverrideTags.R;

        public string? Style { get; set; } = style;

        public override string ToString() => Style is not null ? $@"\{Name}{Style}" : $@"\{Name}";
    }

    public class S : OverrideTag
    {
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
