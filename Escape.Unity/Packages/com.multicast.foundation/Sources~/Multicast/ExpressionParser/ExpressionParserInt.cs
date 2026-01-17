namespace Multicast.ExpressionParser {
    using System;
    using System.Globalization;

    public class ExpressionParserInt : CodeWriter.ExpressionParser.ExpressionParser<int> {
        public static readonly CodeWriter.ExpressionParser.ExpressionParser<int> Instance = new ExpressionParserInt();

        protected override int Parse(string input) =>
            int.Parse(input, NumberStyles.Any, CultureInfo.InvariantCulture);

        protected override int Negate(int v) => -v;
        protected override int Add(int a, int b) => a + b;
        protected override int Sub(int a, int b) => a - b;
        protected override int Mul(int a, int b) => a * b;
        protected override int Div(int a, int b) => a / b;
        protected override int Mod(int a, int b) => a % b;
        protected override int Pow(int a, int b) => (int)Math.Pow(a, b);

        protected override int Equal(int a, int b) => a == b ? 1 : 0;
        protected override int NotEqual(int a, int b) => a != b ? 1 : 0;

        protected override int LessThan(int a, int b) => a < b ? 1 : 0;
        protected override int LessThanOrEqual(int a, int b) => a <= b ? 1 : 0;
        protected override int GreaterThan(int a, int b) => a > b ? 1 : 0;
        protected override int GreaterThanOrEqual(int a, int b) => a >= b ? 1 : 0;

        protected override bool IsTrue(int v) => v != 0;
        protected override int Round(int v) => v;
        protected override int Ceiling(int v) => v;
        protected override int Floor(int v) => v;
        protected override int Log10(int v) => (int)Math.Log10(v);

        protected override int Log(int v, int newBase) => (int)Math.Log(v, newBase);

        protected override int False { get; } = 0;
        protected override int True { get; } = 1;
    }
}