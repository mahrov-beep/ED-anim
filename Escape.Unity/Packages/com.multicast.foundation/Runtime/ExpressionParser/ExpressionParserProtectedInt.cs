namespace Multicast.ExpressionParser {
    using System;
    using System.Globalization;
    using Numerics;

    public class ExpressionParserProtectedInt : CodeWriter.ExpressionParser.ExpressionParser<ProtectedInt> {
        private static readonly ProtectedInt MinusOne = new ProtectedInt(-1);
        private static readonly ProtectedInt Zero     = new ProtectedInt(0);
        private static readonly ProtectedInt One      = new ProtectedInt(1);

        public static readonly CodeWriter.ExpressionParser.ExpressionParser<ProtectedInt> Instance = new ExpressionParserProtectedInt();

        protected override ProtectedInt Parse(string input) =>
            new ProtectedInt(int.Parse(input, NumberStyles.Any, CultureInfo.InvariantCulture));

        protected override ProtectedInt Negate(ProtectedInt v)              => v * MinusOne;
        protected override ProtectedInt Add(ProtectedInt a, ProtectedInt b) => a + b;
        protected override ProtectedInt Sub(ProtectedInt a, ProtectedInt b) => a - b;
        protected override ProtectedInt Mul(ProtectedInt a, ProtectedInt b) => a * b;
        protected override ProtectedInt Div(ProtectedInt a, ProtectedInt b) => a / b;
        protected override ProtectedInt Mod(ProtectedInt a, ProtectedInt b) => new ProtectedInt(a.Value % b.Value);
        protected override ProtectedInt Pow(ProtectedInt a, ProtectedInt b) => new ProtectedInt((int) Math.Pow(a.Value, b.Value));

        protected override ProtectedInt Equal(ProtectedInt a, ProtectedInt b)    => a == b ? One : Zero;
        protected override ProtectedInt NotEqual(ProtectedInt a, ProtectedInt b) => a != b ? One : Zero;

        protected override ProtectedInt LessThan(ProtectedInt a, ProtectedInt b)           => a < b ? One : Zero;
        protected override ProtectedInt LessThanOrEqual(ProtectedInt a, ProtectedInt b)    => a <= b ? One : Zero;
        protected override ProtectedInt GreaterThan(ProtectedInt a, ProtectedInt b)        => a > b ? One : Zero;
        protected override ProtectedInt GreaterThanOrEqual(ProtectedInt a, ProtectedInt b) => a >= b ? One : Zero;

        protected override bool         IsTrue(ProtectedInt v)  => v != Zero;
        protected override ProtectedInt Round(ProtectedInt v)   => v;
        protected override ProtectedInt Ceiling(ProtectedInt v) => v;
        protected override ProtectedInt Floor(ProtectedInt v)   => v;
        protected override ProtectedInt Log10(ProtectedInt v)   => new ProtectedInt((int) Math.Log10(v.Value));

        protected override ProtectedInt Log(ProtectedInt v, ProtectedInt newBase)
            => new ProtectedInt((int) Math.Log(v.Value, newBase.Value));

        protected override ProtectedInt False { get; } = new ProtectedInt(0);
        protected override ProtectedInt True  { get; } = new ProtectedInt(1);
    }
}