// SPDX-License-Identifier: MPL-2.0

namespace Holo.Media;

public readonly struct Rational
{
    public int Numerator { get; init; }
    public int Denominator { get; init; }
    public double Ratio => Numerator / (double)Denominator;
}
