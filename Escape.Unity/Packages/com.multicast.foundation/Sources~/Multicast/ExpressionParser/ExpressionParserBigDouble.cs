namespace Multicast.ExpressionParser {
    using Numerics;

    public class ExpressionParserBigDouble : CodeWriter.ExpressionParser.ExpressionParser<BigDouble> {
        private static readonly BigDouble Zero = 0;
        private static readonly BigDouble One  = 1;

        public static readonly CodeWriter.ExpressionParser.ExpressionParser<BigDouble> Instance = new ExpressionParserBigDouble();

        protected override BigDouble Parse(string input) => BigDouble.Parse(input);

        protected override BigDouble Negate(BigDouble v) => -v;

        protected override BigDouble Add(BigDouble a, BigDouble b) => a + b;
        protected override BigDouble Sub(BigDouble a, BigDouble b) => a - b;
        protected override BigDouble Mul(BigDouble a, BigDouble b) => a * b;
        protected override BigDouble Div(BigDouble a, BigDouble b) => a / b;
        protected override BigDouble Mod(BigDouble a, BigDouble b) => a - b * BigDouble.Floor(a / b);
        protected override BigDouble Pow(BigDouble a, BigDouble b) => BigDouble.Pow(a, b.ToDoubleUnsafe());

        protected override BigDouble Equal(BigDouble a, BigDouble b)    => a == b ? One : Zero;
        protected override BigDouble NotEqual(BigDouble a, BigDouble b) => a != b ? One : Zero;

        protected override BigDouble LessThan(BigDouble a, BigDouble b)           => a < b ? One : Zero;
        protected override BigDouble LessThanOrEqual(BigDouble a, BigDouble b)    => a <= b ? One : Zero;
        protected override BigDouble GreaterThan(BigDouble a, BigDouble b)        => a > b ? One : Zero;
        protected override BigDouble GreaterThanOrEqual(BigDouble a, BigDouble b) => a >= b ? One : Zero;

        protected override bool      IsTrue(BigDouble v)  => v != Zero;
        protected override BigDouble Round(BigDouble v)   => BigDouble.Round(v);
        protected override BigDouble Ceiling(BigDouble v) => BigDouble.Ceiling(v);
        protected override BigDouble Floor(BigDouble v)   => BigDouble.Floor(v);
        protected override BigDouble Log10(BigDouble v)   => BigDouble.Log10(v);

        protected override BigDouble Log(BigDouble v, BigDouble newBase) => BigDouble.Log(v, newBase);

        protected override BigDouble False { get; } = 0;
        protected override BigDouble True  { get; } = 1;
    }
}