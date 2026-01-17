namespace Multicast.Numerics {
    using System;
    using System.Globalization;
    using System.Text;

    public static class BigString {
        public static readonly string[] Names = {
            string.Empty,
            "K",
            "M",
            "B",
            "T",
            "aa",
            "bb",
            "cc",
            "dd",
            "ee",
            "ff",
            "gg",
            "hh",
            "ii",
            "jj",
            "kk",
            "ll",
            "mm",
            "nn",
            "oo",
            "pp",
            "qq",
            "rr",
            "ss",
            "tt",
            "uu",
            "vv",
            "ww",
            "xx",
            "yy",
            "zz",
        };

        private static readonly int[] MultiplierExponents = {
            1,
            10,
            100
        };

        private static readonly string[] Formats = {
            "F0",
            "F1",
            "F2",
        };

        private static readonly int MaxExponent = Names.Length * 3;

        public static string ToString(BigDouble d) {
            if (d == BigDouble.Zero) {
                return "0";
            }

            if (d.exponent < 3) {
                var digits = Math.Min(2, 2 - (int)d.exponent);
                return Math.Round(d.numerator * Math.Pow(10, d.exponent), digits).ToString(CultureInfo.InvariantCulture);
            }

            var expMod = (int)d.exponent % 3;
            var expLeft = d.exponent - expMod;
            var text = Math.Round(d.numerator * MultiplierExponents[expMod], 2 - expMod)
                .ToString(Formats[2 - expMod]);

            if (expLeft == 0L) {
                return text;
            }

            var str = (expLeft >= MaxExponent) ? GenerateCurrency(expLeft) : Names[expLeft / 3];
            return text + str;
        }

        public static string GenerateCurrency(long exp) {
            return $"e{exp}";
        }
    }
}