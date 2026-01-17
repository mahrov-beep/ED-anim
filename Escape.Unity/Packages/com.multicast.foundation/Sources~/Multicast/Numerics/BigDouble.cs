namespace Multicast.Numerics {
    using System;
    using System.Diagnostics;
    using System.Globalization;

    [Serializable]
    public struct BigDouble : IEquatable<BigDouble>, IComparable<BigDouble> {
        private const double EPSILON = 1E-06f;

        private static readonly CultureInfo Culture;

        static BigDouble() {
            Culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();

            Culture.NumberFormat.NumberDecimalSeparator = ".";
        }

        public static readonly BigDouble Zero = new BigDouble(0.0, long.MinValue);

        public double numerator;
        public long exponent;

        public BigDouble(double numerator, long exponent) {
            this.numerator = numerator;
            this.exponent = exponent;
        }

        public static BigDouble Create(double numerator, long exponent = 0) {
            if (Math.Abs(numerator) <= EPSILON) {
                return Zero;
            }

            if (double.IsInfinity(numerator)) {
                Console.WriteLine("Trying to create BigDouble from Infinity");
                return Zero;
            }

            if ((numerator >= 1.0 && numerator < 10.0) || (numerator <= -1.0 && numerator > -10.0)) {
                return new BigDouble(numerator, exponent);
            }

            return Normalize(numerator, exponent);
        }

        private static BigDouble Normalize(double numerator, long exponent) {
            if (Math.Abs(numerator) <= EPSILON) {
                throw new InvalidOperationException("Too small");
            }

            var iter = 10000;

            while ((numerator < 1.0 && numerator > -1.0) && iter-- > 0) {
                numerator *= 10.0;
                exponent--;
            }

            while ((numerator > 10.0 - EPSILON || numerator < -10.0 + EPSILON) && iter-- > 0) {
                numerator *= 0.1;
                exponent++;
            }

            return new BigDouble(numerator, exponent);
        }

        public static BigDouble Round(BigDouble v) {
            return v == Zero ? Zero : v.exponent > 3 ? v : Math.Round(v.ToFloatUnsafe());
        }

        public static BigDouble Ceiling(BigDouble v) {
            return v == Zero ? Zero : v.exponent > 3 ? v : Math.Ceiling(v.ToFloatUnsafe());
        }

        public static BigDouble Floor(BigDouble v) {
            return v == Zero ? Zero : v.exponent > 3 ? v : Math.Floor(v.ToFloatUnsafe());
        }

        public override string ToString() {
            if (this.Equals(Zero)) {
                return "0";
            }

            if (this.exponent < 30 && this.exponent > -30) {
                return (this.numerator * Math.Pow(10.0, this.exponent)).ToString(Culture);
            }

            var expString = (this.exponent < 0) ? "E" : "E+";
            return this.numerator.ToString(Culture) + expString + this.exponent.ToString(Culture);
        }

        public static BigDouble Parse(string input) {
            if (input.IndexOf(',') != -1) {
                input = input.Replace(',', '.');
            }

            var eIndex = input.IndexOf('E', StringComparison.InvariantCulture);
            if (eIndex <= 0) {
                return Create(double.Parse(input, NumberStyles.Any, Culture));
            }

            var num = double.Parse(input.AsSpan().Slice(0, eIndex), NumberStyles.Any, Culture);
            var exp = long.Parse(input.AsSpan().Slice(eIndex + 1), NumberStyles.Any, Culture);
            return Create(num, exp);
        }

        public float ToFloatOrThrow() {
            var result = this.ToFloatUnsafe();

            if (float.IsInfinity(result)) {
                throw new OverflowException("Failed to covert BigDouble to float");
            }

            return result;
        }

        public float ToFloatUnsafe() {
            return (float)(this.numerator * Math.Pow(10.0, this.exponent));
        }

        public double ToDoubleUnsafe() {
            return this.numerator * Math.Pow(10.0, this.exponent);
        }

        public int RoundToIntUnsafe() {
            return (int)Math.Round(this.ToFloatUnsafe());
        }

        public int CompareTo(BigDouble another) {
            if (this > another) {
                return 1;
            }

            if (this == another) {
                return 0;
            }

            return -1;
        }

        public override bool Equals(object obj) {
            return obj is BigDouble other && this.Equals(other);
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            var numHash = this.numerator.GetHashCode();

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            var expHash = this.exponent.GetHashCode();

            return numHash ^ (expHash << 2);
        }

        public bool Equals(BigDouble other) {
            return Math.Abs(this.numerator - other.numerator) < EPSILON && this.exponent == other.exponent;
        }

        private static BigDouble Plus(BigDouble left, BigDouble right) {
            var bigger = left;
            var smaller = right;

            if (smaller.exponent > bigger.exponent) {
                (bigger, smaller) = (smaller, bigger);
            }

            if (smaller == Zero) {
                return bigger;
            }

            var num = smaller.numerator * Math.Pow(10.0, smaller.exponent - bigger.exponent);
            return Create(bigger.numerator + num, bigger.exponent);
        }

        public static BigDouble Pow(BigDouble num, double exp) {
            var e = num.exponent * exp % 1.0;
            var d = Math.Pow(num.numerator, exp) * Math.Pow(10.0, e);

            if (double.IsInfinity(d)) {
                var bigDouble = Pow(num, exp / 2.0);
                return bigDouble * bigDouble;
            }

            var num3 = (long)(exp * num.exponent - e);
            return Create(d, num3);
        }

        public static double Log10(BigDouble value) {
            return Math.Log10(value.numerator) + value.exponent;
        }

        public static double Log(BigDouble value, BigDouble newBase) {
            return Log10(value) / Log10(newBase);
        }

        public static BigDouble Abs(BigDouble a) {
            return a < 0 ? -a : a;
        }

        public static BigDouble Max(BigDouble a, BigDouble b) => b > a ? b : a;
        public static BigDouble Min(BigDouble a, BigDouble b) => b < a ? b : a;

        public static implicit operator BigDouble(long value) => Create(value);
        public static implicit operator BigDouble(double value) => Create(value);

        public static bool operator >(BigDouble left, BigDouble right) {
            if (left.exponent > right.exponent) {
                return left.numerator > 0.0;
            }

            if (left.exponent != right.exponent) {
                return right.numerator < 0.0;
            }

            if (Math.Abs(left.numerator - right.numerator) < EPSILON) {
                return false;
            }

            return left.numerator > right.numerator;
        }

        public static bool operator <(BigDouble left, BigDouble right) => right > left;

        public static bool operator >=(BigDouble left, BigDouble right) {
            if (left.exponent > right.exponent) {
                return left.numerator > 0.0;
            }

            if (left.exponent != right.exponent) {
                return right.numerator < 0.0;
            }

            if (Math.Abs(left.numerator - right.numerator) < EPSILON) {
                return true;
            }

            return left.numerator >= right.numerator;
        }

        public static bool operator <=(BigDouble left, BigDouble right) => right >= left;

        public static bool operator ==(BigDouble left, BigDouble right) {
            return Math.Abs(left.numerator - right.numerator) < EPSILON && left.exponent == right.exponent;
        }

        public static bool operator !=(BigDouble left, BigDouble right) {
            return !(Math.Abs(left.numerator - right.numerator) < EPSILON) || left.exponent != right.exponent;
        }

        public static BigDouble operator *(BigDouble left, BigDouble right) {
            return Create(left.numerator * right.numerator, left.exponent + right.exponent);
        }

        public static BigDouble operator /(BigDouble left, BigDouble right) {
            return Create(left.numerator / right.numerator, left.exponent - right.exponent);
        }

        public static BigDouble operator +(BigDouble left, BigDouble right) => Plus(left, right);
        public static BigDouble operator -(BigDouble left, BigDouble right) => Plus(left, -right);

        public static BigDouble operator -(BigDouble left) => Create(-left.numerator, left.exponent);
    }
}