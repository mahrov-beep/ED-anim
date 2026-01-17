// https://github.com/asik/FixedMath.Net

namespace Multicast.Numerics {
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a Q31.32 fixed-point number.
    /// </summary>
    [Serializable]
    public struct FixedDouble : IEquatable<FixedDouble>, IComparable<FixedDouble> {
        public long rawValue;

        // Precision of this type is 2^-32, that is 2,3283064365386962890625E-10
        public static readonly decimal Precision = (decimal)(new FixedDouble(1L)); //0.00000000023283064365386962890625m;

        public static readonly FixedDouble MaxValue = new FixedDouble(MAX_VALUE);
        public static readonly FixedDouble MinValue = new FixedDouble(MIN_VALUE);
        public static readonly FixedDouble One = new FixedDouble(ONE);
        public static readonly FixedDouble Zero = new FixedDouble();
        public static readonly FixedDouble Pi = new FixedDouble(PI);
        public static readonly FixedDouble PiOver2 = new FixedDouble(PI_OVER_2);
        public static readonly FixedDouble PiTimes2 = new FixedDouble(PI_TIMES_2);
        public static readonly FixedDouble PiInv = (FixedDouble)0.3183098861837906715377675267M;
        public static readonly FixedDouble PiOver2Inv = (FixedDouble)0.6366197723675813430755350535M;

        private static readonly FixedDouble Log2Max = new FixedDouble(LOG2_MAX);
        private static readonly FixedDouble Log2Min = new FixedDouble(LOG2_MIN);
        private static readonly FixedDouble Ln2 = new FixedDouble(LN2);

        private static readonly FixedDouble LutInterval = (FixedDouble)(LUT_SIZE - 1) / PiOver2;

        private const long MAX_VALUE = long.MaxValue;
        private const long MIN_VALUE = long.MinValue;
        private const int NUM_BITS = 64;
        private const int FRACTIONAL_PLACES = 32;
        private const long ONE = 1L << FRACTIONAL_PLACES;
        private const long PI_TIMES_2 = 0x6487ED511;
        private const long PI = 0x3243F6A88;
        private const long PI_OVER_2 = 0x1921FB544;
        private const long LN2 = 0xB17217F7;
        private const long LOG2_MAX = 0x1F00000000;
        private const long LOG2_MIN = -0x2000000000;
        private const int LUT_SIZE = (int)(PI_OVER_2 >> 15);

        /// <summary>
        /// Returns a number indicating the sign of a FixedDouble number.
        /// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
        /// </summary>
        public static int Sign(FixedDouble value) {
            return
                value.rawValue < 0 ? -1 :
                value.rawValue > 0 ? 1 :
                0;
        }

        /// <summary>
        /// Returns the absolute value of a FixedDouble number.
        /// Note: Abs(FixedDouble.MinValue) == FixedDouble.MaxValue.
        /// </summary>
        public static FixedDouble Abs(FixedDouble value) {
            if (value.rawValue == MIN_VALUE) {
                return MaxValue;
            }

            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value.rawValue >> 63;
            return new FixedDouble((value.rawValue + mask) ^ mask);
        }

        /// <summary>
        /// Returns the absolute value of a FixedDouble number.
        /// FastAbs(FixedDouble.MinValue) is undefined.
        /// </summary>
        public static FixedDouble FastAbs(FixedDouble value) {
            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value.rawValue >> 63;
            return new FixedDouble((value.rawValue + mask) ^ mask);
        }


        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        public static FixedDouble Floor(FixedDouble value) {
            // Just zero out the fractional part
            return new FixedDouble((long)((ulong)value.rawValue & 0xFFFFFFFF00000000));
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified number.
        /// </summary>
        public static FixedDouble Ceiling(FixedDouble value) {
            var hasFractionalPart = (value.rawValue & 0x00000000FFFFFFFF) != 0;
            return hasFractionalPart ? Floor(value) + One : value;
        }

        /// <summary>
        /// Rounds a value to the nearest integral value.
        /// If the value is halfway between an even and an uneven value, returns the even value.
        /// </summary>
        public static FixedDouble Round(FixedDouble value) {
            var fractionalPart = value.rawValue & 0x00000000FFFFFFFF;
            var integralPart = Floor(value);
            if (fractionalPart < 0x80000000) {
                return integralPart;
            }

            if (fractionalPart > 0x80000000) {
                return integralPart + One;
            }

            // if number is halfway between two values, round to the nearest even number
            // this is the method used by System.Math.Round().
            return (integralPart.rawValue & ONE) == 0
                ? integralPart
                : integralPart + One;
        }

        /// <summary>
        /// Adds x and y. Performs saturating addition, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static FixedDouble operator +(FixedDouble x, FixedDouble y) {
            var xl = x.rawValue;
            var yl = y.rawValue;
            var sum = xl + yl;
            // if signs of operands are equal and signs of sum and x are different
            if (((~(xl ^ yl) & (xl ^ sum)) & MIN_VALUE) != 0) {
                sum = xl > 0 ? MAX_VALUE : MIN_VALUE;
            }

            return new FixedDouble(sum);
        }

        /// <summary>
        /// Adds x and y witout performing overflow checking. Should be inlined by the CLR.
        /// </summary>
        public static FixedDouble FastAdd(FixedDouble x, FixedDouble y) {
            return new FixedDouble(x.rawValue + y.rawValue);
        }

        /// <summary>
        /// Subtracts y from x. Performs saturating substraction, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static FixedDouble operator -(FixedDouble x, FixedDouble y) {
            var xl = x.rawValue;
            var yl = y.rawValue;
            var diff = xl - yl;
            // if signs of operands are different and signs of sum and x are different
            if ((((xl ^ yl) & (xl ^ diff)) & MIN_VALUE) != 0) {
                diff = xl < 0 ? MIN_VALUE : MAX_VALUE;
            }

            return new FixedDouble(diff);
        }

        /// <summary>
        /// Subtracts y from x witout performing overflow checking. Should be inlined by the CLR.
        /// </summary>
        public static FixedDouble FastSub(FixedDouble x, FixedDouble y) {
            return new FixedDouble(x.rawValue - y.rawValue);
        }

        private static long AddOverflowHelper(long x, long y, ref bool overflow) {
            var sum = x + y;
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & MIN_VALUE) != 0;
            return sum;
        }

        public static FixedDouble operator *(FixedDouble x, FixedDouble y) {
            var xl = x.rawValue;
            var yl = y.rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            bool overflow = false;
            var sum = AddOverflowHelper((long)loResult, midResult1, ref overflow);
            sum = AddOverflowHelper(sum, midResult2, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);

            bool opSignsEqual = ((xl ^ yl) & MIN_VALUE) == 0;

            // if signs of operands are equal and sign of result is negative,
            // then multiplication overflowed positively
            // the reverse is also true
            if (opSignsEqual) {
                if (sum < 0 || (overflow && xl > 0)) {
                    return MaxValue;
                }
            }
            else {
                if (sum > 0) {
                    return MinValue;
                }
            }

            // if the top 32 bits of hihi (unused in the result) are neither all 0s or 1s,
            // then this means the result overflowed.
            var topCarry = hihi >> FRACTIONAL_PLACES;
            if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/) {
                return opSignsEqual ? MaxValue : MinValue;
            }

            // If signs differ, both operands' magnitudes are greater than 1,
            // and the result is greater than the negative operand, then there was negative overflow.
            if (!opSignsEqual) {
                long posOp, negOp;
                if (xl > yl) {
                    posOp = xl;
                    negOp = yl;
                }
                else {
                    posOp = yl;
                    negOp = xl;
                }

                if (sum > negOp && negOp < -ONE && posOp > ONE) {
                    return MinValue;
                }
            }

            return new FixedDouble(sum);
        }

        /// <summary>
        /// Performs multiplication without checking for overflow.
        /// Useful for performance-critical code where the values are guaranteed not to cause overflow
        /// </summary>
        public static FixedDouble FastMul(FixedDouble x, FixedDouble y) {
            var xl = x.rawValue;
            var yl = y.rawValue;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            var sum = (long)loResult + midResult1 + midResult2 + hiResult;
            return new FixedDouble(sum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CountLeadingZeroes(ulong x) {
            int result = 0;
            while ((x & 0xF000000000000000) == 0) {
                result += 4;
                x <<= 4;
            }

            while ((x & 0x8000000000000000) == 0) {
                result += 1;
                x <<= 1;
            }

            return result;
        }

        public static FixedDouble operator /(FixedDouble x, FixedDouble y) {
            var xl = x.rawValue;
            var yl = y.rawValue;

            if (yl == 0) {
                throw new DivideByZeroException();
            }

            var remainder = (ulong)(xl >= 0 ? xl : -xl);
            var divider = (ulong)(yl >= 0 ? yl : -yl);
            var quotient = 0UL;
            var bitPos = NUM_BITS / 2 + 1;

            // If the divider is divisible by 2^n, take advantage of it.
            while ((divider & 0xF) == 0 && bitPos >= 4) {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0) {
                int shift = CountLeadingZeroes(remainder);
                if (shift > bitPos) {
                    shift = bitPos;
                }

                remainder <<= shift;
                bitPos -= shift;

                var div = remainder / divider;
                remainder = remainder % divider;
                quotient += div << bitPos;

                // Detect overflow
                if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0) {
                    return ((xl ^ yl) & MIN_VALUE) == 0 ? MaxValue : MinValue;
                }

                remainder <<= 1;
                --bitPos;
            }

            // rounding
            ++quotient;
            var result = (long)(quotient >> 1);
            if (((xl ^ yl) & MIN_VALUE) != 0) {
                result = -result;
            }

            return new FixedDouble(result);
        }

        public static FixedDouble operator %(FixedDouble x, FixedDouble y) {
            return new FixedDouble(
                x.rawValue == MIN_VALUE & y.rawValue == -1 ? 0 : x.rawValue % y.rawValue);
        }

        /// <summary>
        /// Performs modulo as fast as possible; throws if x == MinValue and y == -1.
        /// Use the operator (%) for a more reliable but slower modulo.
        /// </summary>
        public static FixedDouble FastMod(FixedDouble x, FixedDouble y) {
            return new FixedDouble(x.rawValue % y.rawValue);
        }

        public static FixedDouble operator -(FixedDouble x) {
            return x.rawValue == MIN_VALUE ? MaxValue : new FixedDouble(-x.rawValue);
        }

        public static bool operator ==(FixedDouble x, FixedDouble y) {
            return x.rawValue == y.rawValue;
        }

        public static bool operator !=(FixedDouble x, FixedDouble y) {
            return x.rawValue != y.rawValue;
        }

        public static bool operator >(FixedDouble x, FixedDouble y) {
            return x.rawValue > y.rawValue;
        }

        public static bool operator <(FixedDouble x, FixedDouble y) {
            return x.rawValue < y.rawValue;
        }

        public static bool operator >=(FixedDouble x, FixedDouble y) {
            return x.rawValue >= y.rawValue;
        }

        public static bool operator <=(FixedDouble x, FixedDouble y) {
            return x.rawValue <= y.rawValue;
        }

        /// <summary>
        /// Returns 2 raised to the specified power.
        /// Provides at least 6 decimals of accuracy.
        /// </summary>
        public static FixedDouble Pow2(FixedDouble x) {
            if (x.rawValue == 0) {
                return One;
            }

            // Avoid negative arguments by exploiting that exp(-x) = 1/exp(x).
            bool neg = x.rawValue < 0;
            if (neg) {
                x = -x;
            }

            if (x == One) {
                return neg ? One / (FixedDouble)2 : (FixedDouble)2;
            }

            if (x >= Log2Max) {
                return neg ? One / MaxValue : MaxValue;
            }

            if (x <= Log2Min) {
                return neg ? MaxValue : Zero;
            }

            /* The algorithm is based on the power series for exp(x):
             * http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
             * 
             * From term n, we get term n+1 by multiplying with x/n.
             * When the sum term drops to zero, we can stop summing.
             */

            int integerPart = (int)Floor(x);
            // Take fractional part of exponent
            x = new FixedDouble(x.rawValue & 0x00000000FFFFFFFF);

            var result = One;
            var term = One;
            int i = 1;
            while (term.rawValue != 0) {
                term = FastMul(FastMul(x, term), Ln2) / (FixedDouble)i;
                result += term;
                i++;
            }

            result = FromRaw(result.rawValue << integerPart);
            if (neg) {
                result = One / result;
            }

            return result;
        }

        /// <summary>
        /// Returns the base-2 logarithm of a specified number.
        /// Provides at least 9 decimals of accuracy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        public static FixedDouble Log2(FixedDouble x) {
            if (x.rawValue <= 0) {
                throw new ArgumentOutOfRangeException("Non-positive value passed to Ln", "x");
            }

            // This implementation is based on Clay. S. Turner's fast binary logarithm
            // algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
            //     Processing Mag., pp. 124,140, Sep. 2010.)

            long b = 1U << (FRACTIONAL_PLACES - 1);
            long y = 0;

            long rawX = x.rawValue;
            while (rawX < ONE) {
                rawX <<= 1;
                y -= ONE;
            }

            while (rawX >= (ONE << 1)) {
                rawX >>= 1;
                y += ONE;
            }

            var z = new FixedDouble(rawX);

            for (int i = 0; i < FRACTIONAL_PLACES; i++) {
                z = FastMul(z, z);
                if (z.rawValue >= (ONE << 1)) {
                    z = new FixedDouble(z.rawValue >> 1);
                    y += b;
                }

                b >>= 1;
            }

            return new FixedDouble(y);
        }

        /// <summary>
        /// Returns the natural logarithm of a specified number.
        /// Provides at least 7 decimals of accuracy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        public static FixedDouble Ln(FixedDouble x) {
            return FastMul(Log2(x), Ln2);
        }

        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// Provides about 5 digits of accuracy for the result.
        /// </summary>
        /// <exception cref="DivideByZeroException">
        /// The base was zero, with a negative exponent
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The base was negative, with a non-zero exponent
        /// </exception>
        public static FixedDouble Pow(FixedDouble b, FixedDouble exp) {
            if (b == One) {
                return One;
            }

            if (exp.rawValue == 0) {
                return One;
            }

            if (b.rawValue == 0) {
                if (exp.rawValue < 0) {
                    throw new DivideByZeroException();
                }

                return Zero;
            }

            FixedDouble log2 = Log2(b);
            return Pow2(exp * log2);
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was negative.
        /// </exception>
        public static FixedDouble Sqrt(FixedDouble x) {
            var xl = x.rawValue;
            if (xl < 0) {
                // We cannot represent infinities like Single and Double, and Sqrt is
                // mathematically undefined for x < 0. So we just throw an exception.
                throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");
            }

            var num = (ulong)xl;
            var result = 0UL;

            // second-to-top bit
            var bit = 1UL << (NUM_BITS - 2);

            while (bit > num) {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 128 bit values in computations.
            for (var i = 0; i < 2; ++i) {
                // First we get the top 48 bits of the answer.
                while (bit != 0) {
                    if (num >= result + bit) {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else {
                        result = result >> 1;
                    }

                    bit >>= 2;
                }

                if (i == 0) {
                    // Then process it again to get the lowest 16 bits.
                    if (num > (1UL << (NUM_BITS / 2)) - 1) {
                        // The remainder 'num' is too large to be shifted left
                        // by 32, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << (NUM_BITS / 2)) - 0x80000000UL;
                        result = (result << (NUM_BITS / 2)) + 0x80000000UL;
                    }
                    else {
                        num <<= (NUM_BITS / 2);
                        result <<= (NUM_BITS / 2);
                    }

                    bit = 1UL << (NUM_BITS / 2 - 2);
                }
            }

            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result) {
                ++result;
            }

            return new FixedDouble((long)result);
        }

        public static explicit operator FixedDouble(long value) {
            return new FixedDouble(value * ONE);
        }

        public static explicit operator long(FixedDouble value) {
            return value.rawValue >> FRACTIONAL_PLACES;
        }

        public static explicit operator FixedDouble(float value) {
            return new FixedDouble((long)(value * ONE));
        }

        public static explicit operator float(FixedDouble value) {
            return (float)value.rawValue / ONE;
        }

        public static explicit operator FixedDouble(double value) {
            return new FixedDouble((long)(value * ONE));
        }

        public static explicit operator double(FixedDouble value) {
            return (double)value.rawValue / ONE;
        }

        public static explicit operator FixedDouble(decimal value) {
            return new FixedDouble((long)(value * ONE));
        }

        public static explicit operator decimal(FixedDouble value) {
            return (decimal)value.rawValue / ONE;
        }

        public override bool Equals(object obj) {
            return obj is FixedDouble && ((FixedDouble)obj).rawValue == this.rawValue;
        }

        public override int GetHashCode() {
            return this.rawValue.GetHashCode();
        }

        public bool Equals(FixedDouble other) {
            return this.rawValue == other.rawValue;
        }

        public int CompareTo(FixedDouble other) {
            return this.rawValue.CompareTo(other.rawValue);
        }

        public override string ToString() {
            // Up to 10 decimal places
            return ((decimal)this).ToString("0.##########");
        }

        public static FixedDouble FromRaw(long rawValue) {
            return new FixedDouble(rawValue);
        }

        /// <summary>
        /// This is the constructor from raw value; it can only be used interally.
        /// </summary>
        /// <param name="rawValue"></param>
        private FixedDouble(long rawValue) {
            this.rawValue = rawValue;
        }

        public FixedDouble(int value) {
            this.rawValue = value * ONE;
        }
    }
}