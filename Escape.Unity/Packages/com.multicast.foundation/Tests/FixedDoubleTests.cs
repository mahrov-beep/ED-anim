namespace Multicast {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Numerics;
    using NUnit.Framework;
    using Debug = UnityEngine.Debug;

    public class FixedDoubleTests {
        private readonly long[] m_testCases = new[] {
            // Small numbers
            0L, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            -1, -2, -3, -4, -5, -6, -7, -8, -9, -10,

            // Integer numbers
            0x100000000, -0x100000000, 0x200000000, -0x200000000, 0x300000000, -0x300000000,
            0x400000000, -0x400000000, 0x500000000, -0x500000000, 0x600000000, -0x600000000,

            // Fractions (1/2, 1/4, 1/8)
            0x80000000, -0x80000000, 0x40000000, -0x40000000, 0x20000000, -0x20000000,

            // Problematic carry
            0xFFFFFFFF, -0xFFFFFFFF, 0x1FFFFFFFF, -0x1FFFFFFFF, 0x3FFFFFFFF, -0x3FFFFFFFF,

            // Smallest and largest values
            long.MaxValue, long.MinValue,

            // Large random numbers
            6791302811978701836, -8192141831180282065, 6222617001063736300, -7871200276881732034,
            8249382838880205112, -7679310892959748444, 7708113189940799513, -5281862979887936768,
            8220231180772321456, -5204203381295869580, 6860614387764479339, -9080626825133349457,
            6658610233456189347, -6558014273345705245, 6700571222183426493,

            // Small random numbers
            -436730658, -2259913246, 329347474, 2565801981, 3398143698, 137497017, 1060347500,
            -3457686027, 1923669753, 2891618613, 2418874813, 2899594950, 2265950765, -1962365447,
            3077934393

            // Tiny random numbers
            - 171,
            -359, 491, 844, 158, -413, -422, -737, -575, -330,
            -376, 435, -311, 116, 715, -1024, -487, 59, 724, 993,
        };

        [Test]
        public void Precision() {
            Assert.AreEqual(0.00000000023283064365386962890625m, FixedDouble.Precision);
        }

        [Test]
        public void LongToFixedDoubleAndBack() {
            var sources   = new[] {long.MinValue, int.MinValue - 1L, int.MinValue, -1L, 0L, 1L, int.MaxValue, int.MaxValue + 1L, long.MaxValue};
            var expecteds = new[] {0L, int.MaxValue, int.MinValue, -1L, 0L, 1L, int.MaxValue, int.MinValue, -1L};
            for (int i = 0; i < sources.Length; ++i) {
                var expected = expecteds[i];
                var f        = (FixedDouble) sources[i];
                var actual   = (long) f;
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void DoubleToFixedDoubleAndBack() {
            var sources = new[] {
                (double) int.MinValue,
                -(double) Math.PI,
                -(double) Math.E,
                -1.0,
                -0.0,
                0.0,
                1.0,
                (double) Math.PI,
                (double) Math.E,
                (double) int.MaxValue,
            };

            foreach (var value in sources) {
                AreEqualWithinPrecision(value, (double) (FixedDouble) value);
            }
        }

        static void AreEqualWithinPrecision(decimal value1, decimal value2) {
            Assert.True(Math.Abs(value2 - value1) < FixedDouble.Precision);
        }

        static void AreEqualWithinPrecision(double value1, double value2) {
            Assert.True(Math.Abs(value2 - value1) < (double) FixedDouble.Precision);
        }

        [Test]
        public void DecimalToFixedDoubleAndBack() {
            Assert.AreEqual(FixedDouble.MaxValue, (FixedDouble) (decimal) FixedDouble.MaxValue);
            Assert.AreEqual(FixedDouble.MinValue, (FixedDouble) (decimal) FixedDouble.MinValue);

            var sources = new[] {
                int.MinValue,
                -(decimal) Math.PI,
                -(decimal) Math.E,
                -1.0m,
                -0.0m,
                0.0m,
                1.0m,
                (decimal) Math.PI,
                (decimal) Math.E,
                int.MaxValue,
            };

            foreach (var value in sources) {
                AreEqualWithinPrecision(value, (decimal) (FixedDouble) value);
            }
        }

        [Test]
        public void Addition() {
            var terms1    = new[] {FixedDouble.MinValue, (FixedDouble) (-1), FixedDouble.Zero, FixedDouble.One, FixedDouble.MaxValue};
            var terms2    = new[] {(FixedDouble) (-1), (FixedDouble) 2, (FixedDouble) (-1.5m), (FixedDouble) (-2), FixedDouble.One};
            var expecteds = new[] {FixedDouble.MinValue, FixedDouble.One, (FixedDouble) (-1.5m), (FixedDouble) (-1), FixedDouble.MaxValue};
            for (int i = 0; i < terms1.Length; ++i) {
                var actual   = terms1[i] + terms2[i];
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Substraction() {
            var terms1    = new[] {FixedDouble.MinValue, (FixedDouble) (-1), FixedDouble.Zero, FixedDouble.One, FixedDouble.MaxValue};
            var terms2    = new[] {FixedDouble.One, (FixedDouble) (-2), (FixedDouble) (1.5m), (FixedDouble) (2), (FixedDouble) (-1)};
            var expecteds = new[] {FixedDouble.MinValue, FixedDouble.One, (FixedDouble) (-1.5m), (FixedDouble) (-1), FixedDouble.MaxValue};
            for (int i = 0; i < terms1.Length; ++i) {
                var actual   = terms1[i] - terms2[i];
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void BasicMultiplication() {
            var term1s    = new[] {0m, 1m, -1m, 5m, -5m, 0.5m, -0.5m, -1.0m};
            var term2s    = new[] {16m, 16m, 16m, 16m, 16m, 16m, 16m, -1.0m};
            var expecteds = new[] {0L, 16, -16, 80, -80, 8, -8, 1};
            for (int i = 0; i < term1s.Length; ++i) {
                var expected = expecteds[i];
                var actual   = (long) ((FixedDouble) term1s[i] * (FixedDouble) term2s[i]);
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void MultiplicationTestCases() {
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i) {
                for (int j = 0; j < m_testCases.Length; ++j) {
                    var x        = FixedDouble.FromRaw(m_testCases[i]);
                    var y        = FixedDouble.FromRaw(m_testCases[j]);
                    var xM       = (decimal) x;
                    var yM       = (decimal) y;
                    var expected = xM * yM;
                    expected =
                        expected > (decimal) FixedDouble.MaxValue
                            ? (decimal) FixedDouble.MaxValue
                            : expected < (decimal) FixedDouble.MinValue
                                ? (decimal) FixedDouble.MinValue
                                : expected;

                    var actual = x * y;
                    
                    var actualM  = (decimal) actual;
                    var maxDelta = (decimal) FixedDouble.FromRaw(1);
                    if (Math.Abs(actualM - expected) > maxDelta) {
                        Debug.LogErrorFormat("Failed for FromRaw({0}) * FromRaw({1}): expected {2} but got {3}",
                            m_testCases[i],
                            m_testCases[j],
                            (FixedDouble) expected,
                            actualM);
                        ++failures;
                    }
                }
            }

            Assert.True(failures < 1);
        }


        static void Ignore<T>(T value) {
        }

        [Test]
        public void DivisionTestCases() {
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i) {
                for (int j = 0; j < m_testCases.Length; ++j) {
                    var x  = FixedDouble.FromRaw(m_testCases[i]);
                    var y  = FixedDouble.FromRaw(m_testCases[j]);
                    var xM = (decimal) x;
                    var yM = (decimal) y;

                    if (m_testCases[j] == 0) {
                        Assert.Throws<DivideByZeroException>(() => Ignore(x / y));
                    }
                    else {
                        var expected = xM / yM;
                        expected =
                            expected > (decimal) FixedDouble.MaxValue
                                ? (decimal) FixedDouble.MaxValue
                                : expected < (decimal) FixedDouble.MinValue
                                    ? (decimal) FixedDouble.MinValue
                                    : expected;

                        var actual = x / y;

                        var actualM  = (decimal) actual;
                        var maxDelta = (decimal) FixedDouble.FromRaw(1);
                        if (Math.Abs(actualM - expected) > maxDelta) {
                            Debug.LogErrorFormat("Failed for FromRaw({0}) / FromRaw({1}): expected {2} but got {3}",
                                m_testCases[i],
                                m_testCases[j],
                                (FixedDouble) expected,
                                actualM);
                            ++failures;
                        }
                    }
                }
            }

            Assert.True(failures < 1);
        }


        [Test]
        public void Sign() {
            var sources   = new[] {FixedDouble.MinValue, (FixedDouble) (-1), FixedDouble.Zero, FixedDouble.One, FixedDouble.MaxValue};
            var expecteds = new[] {-1, -1, 0, 1, 1};
            for (int i = 0; i < sources.Length; ++i) {
                var actual   = FixedDouble.Sign(sources[i]);
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Abs() {
            Assert.AreEqual(FixedDouble.MaxValue, FixedDouble.Abs(FixedDouble.MinValue));
            var sources   = new[] {-1, 0, 1, int.MaxValue};
            var expecteds = new[] {1, 0, 1, int.MaxValue};
            for (int i = 0; i < sources.Length; ++i) {
                var actual   = FixedDouble.Abs((FixedDouble) sources[i]);
                var expected = (FixedDouble) expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void FastAbs() {
            Assert.AreEqual(FixedDouble.MinValue, FixedDouble.FastAbs(FixedDouble.MinValue));
            var sources   = new[] {-1, 0, 1, int.MaxValue};
            var expecteds = new[] {1, 0, 1, int.MaxValue};
            for (int i = 0; i < sources.Length; ++i) {
                var actual   = FixedDouble.FastAbs((FixedDouble) sources[i]);
                var expected = (FixedDouble) expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Floor() {
            var sources   = new[] {-5.1m, -1, 0, 1, 5.1m};
            var expecteds = new[] {-6m, -1, 0, 1, 5m};
            for (int i = 0; i < sources.Length; ++i) {
                var actual   = (decimal) FixedDouble.Floor((FixedDouble) sources[i]);
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Ceiling() {
            var sources   = new[] {-5.1m, -1, 0, 1, 5.1m};
            var expecteds = new[] {-5m, -1, 0, 1, 6m};
            for (int i = 0; i < sources.Length; ++i) {
                var actual   = (decimal) FixedDouble.Ceiling((FixedDouble) sources[i]);
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }

            Assert.AreEqual(FixedDouble.MaxValue, FixedDouble.Ceiling(FixedDouble.MaxValue));
        }

        [Test]
        public void Round() {
            var sources   = new[] {-5.5m, -5.1m, -4.5m, -4.4m, -1, 0, 1, 4.5m, 4.6m, 5.4m, 5.5m};
            var expecteds = new[] {-6m, -5m, -4m, -4m, -1, 0, 1, 4m, 5m, 5m, 6m};
            for (int i = 0; i < sources.Length; ++i) {
                var actual   = (decimal) FixedDouble.Round((FixedDouble) sources[i]);
                var expected = expecteds[i];
                Assert.AreEqual(expected, actual);
            }

            Assert.AreEqual(FixedDouble.MaxValue, FixedDouble.Round(FixedDouble.MaxValue));
        }


        [Test]
        public void Sqrt() {
            for (int i = 0; i < m_testCases.Length; ++i) {
                var f = FixedDouble.FromRaw(m_testCases[i]);
                if (FixedDouble.Sign(f) < 0) {
                    Assert.Throws<ArgumentOutOfRangeException>(() => FixedDouble.Sqrt(f));
                }
                else {
                    var expected = Math.Sqrt((double) f);
                    var actual   = (double) FixedDouble.Sqrt(f);
                    var delta    = (decimal) Math.Abs(expected - actual);
                    Assert.True(delta <= FixedDouble.Precision);
                }
            }
        }

        [Test]
        public void Log2() {
            double maxDelta = (double) (FixedDouble.Precision * 4);

            for (int j = 0; j < m_testCases.Length; ++j) {
                var b = FixedDouble.FromRaw(m_testCases[j]);

                if (b <= FixedDouble.Zero) {
                    Assert.Throws<ArgumentOutOfRangeException>(() => FixedDouble.Log2(b));
                }
                else {
                    var expected = Math.Log((double) b) / Math.Log(2);
                    var actual   = (double) FixedDouble.Log2(b);
                    var delta    = Math.Abs(expected - actual);

                    Assert.True(delta <= maxDelta, string.Format("Ln({0}) = expected {1} but got {2}", b, expected, actual));
                }
            }
        }

        [Test]
        public void Ln() {
            double maxDelta = 0.00000001;

            for (int j = 0; j < m_testCases.Length; ++j) {
                var b = FixedDouble.FromRaw(m_testCases[j]);

                if (b <= FixedDouble.Zero) {
                    Assert.Throws<ArgumentOutOfRangeException>(() => FixedDouble.Ln(b));
                }
                else {
                    var expected = Math.Log((double) b);
                    var actual   = (double) FixedDouble.Ln(b);
                    var delta    = Math.Abs(expected - actual);

                    Assert.True(delta <= maxDelta, string.Format("Ln({0}) = expected {1} but got {2}", b, expected, actual));
                }
            }
        }

        [Test]
        public void Pow2() {
            double maxDelta = 0.0000001;
            for (int i = 0; i < m_testCases.Length; ++i) {
                var e = FixedDouble.FromRaw(m_testCases[i]);

                var expected = Math.Min(Math.Pow(2, (double) e), (double) FixedDouble.MaxValue);
                var actual   = (double) FixedDouble.Pow2(e);
                var delta    = Math.Abs(expected - actual);

                Assert.True(delta <= maxDelta, string.Format("Pow2({0}) = expected {1} but got {2}", e, expected, actual));
            }
        }

        [Test]
        public void Pow() {
            for (int i = 0; i < m_testCases.Length; ++i) {
                var b = FixedDouble.FromRaw(m_testCases[i]);

                for (int j = 0; j < m_testCases.Length; ++j) {
                    var e = FixedDouble.FromRaw(m_testCases[j]);

                    if (b == FixedDouble.Zero && e < FixedDouble.Zero) {
                        Assert.Throws<DivideByZeroException>(() => FixedDouble.Pow(b, e));
                    }
                    else if (b < FixedDouble.Zero && e != FixedDouble.Zero) {
                        Assert.Throws<ArgumentOutOfRangeException>(() => FixedDouble.Pow(b, e));
                    }
                    else {
                        var expected = e == FixedDouble.Zero ? 1 : b == FixedDouble.Zero ? 0 : Math.Min(Math.Pow((double) b, (double) e), (double) FixedDouble.MaxValue);

                        // Absolute precision deteriorates with large result values, take this into account
                        // Similarly, large exponents reduce precision, even if result is small.
                        double maxDelta = Math.Abs((double) e) > 100000000 ? 0.5 : expected > 100000000 ? 10 : expected > 1000 ? 0.5 : 0.00001;

                        var actual = (double) FixedDouble.Pow(b, e);
                        var delta  = Math.Abs(expected - actual);

                        Assert.True(delta <= maxDelta, string.Format("Pow({0}, {1}) = expected {2} but got {3}", b, e, expected, actual));
                    }
                }
            }
        }

        [Test]
        public void Modulus() {
            var deltas = new List<decimal>();
            foreach (var operand1 in m_testCases) {
                foreach (var operand2 in m_testCases) {
                    var f1 = FixedDouble.FromRaw(operand1);
                    var f2 = FixedDouble.FromRaw(operand2);

                    if (operand2 == 0) {
                        Assert.Throws<DivideByZeroException>(() => Ignore(f1 / f2));
                    }
                    else {
                        var d1       = (decimal) f1;
                        var d2       = (decimal) f2;
                        var actual   = (decimal) (f1 % f2);
                        var expected = d1 % d2;
                        var delta    = Math.Abs(expected - actual);
                        deltas.Add(delta);
                        Assert.True(delta <= 60 * FixedDouble.Precision, string.Format("{0} % {1} = expected {2} but got {3}", f1, f2, expected, actual));
                    }
                }
            }
        }

        [Test]
        public void Negation() {
            foreach (var operand1 in m_testCases) {
                var f = FixedDouble.FromRaw(operand1);
                if (f == FixedDouble.MinValue) {
                    Assert.AreEqual(-f, FixedDouble.MaxValue);
                }
                else {
                    var expected = -((decimal) f);
                    var actual   = (decimal) (-f);
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [Test]
        public void EqualsTests() {
            foreach (var op1 in m_testCases) {
                foreach (var op2 in m_testCases) {
                    var d1 = (decimal) op1;
                    var d2 = (decimal) op2;
                    Assert.True(op1.Equals(op2) == d1.Equals(d2));
                }
            }
        }

        [Test]
        public void EqualityAndInequalityOperators() {
            var sources = m_testCases.Select(FixedDouble.FromRaw).ToList();
            foreach (var op1 in sources) {
                foreach (var op2 in sources) {
                    var d1 = (double) op1;
                    var d2 = (double) op2;
                    Assert.True((op1 == op2) == (d1 == d2));
                    Assert.True((op1 != op2) == (d1 != d2));
                    Assert.False((op1 == op2) && (op1 != op2));
                }
            }
        }

        [Test]
        public void CompareTo() {
            var nums        = m_testCases.Select(FixedDouble.FromRaw).ToArray();
            var numsDecimal = nums.Select(t => (decimal) t).ToArray();
            Array.Sort(nums);
            Array.Sort(numsDecimal);
            Assert.True(nums.Select(t => (decimal) t).SequenceEqual(numsDecimal));
        }
    }
}