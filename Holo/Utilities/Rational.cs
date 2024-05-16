using System;
using System.Collections.Generic;
using System.Text;

namespace Holo.Utilities
{
    public struct Rational
    {
        public int Numerator { get; }
        public int Denominator { get; }
        public double Value => Numerator / (double)Denominator;

        public Rational(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
    }
}
