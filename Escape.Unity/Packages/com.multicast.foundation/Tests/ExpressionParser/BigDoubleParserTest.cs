namespace Multicast.ExpressionParser {
    using CodeWriter.ExpressionParser;
    using Numerics;
    using NUnit.Framework;

    public class BigDoubleParserTest {
        [Test]
        // Constants
        [TestCase("1", ExpectedResult     = 1)]
        [TestCase("123", ExpectedResult   = 123)]
        [TestCase("12.0", ExpectedResult  = 12)]
        [TestCase("12.34", ExpectedResult = 12.34f)]
        // Expressions
        [TestCase("(1)", ExpectedResult           = 1)]
        [TestCase("(-1)", ExpectedResult          = -1)]
        [TestCase("-(-1)", ExpectedResult         = 1)]
        [TestCase("1 + 2", ExpectedResult         = 3)]
        [TestCase("1+2", ExpectedResult           = 3)]
        [TestCase(" 1  +   2    ", ExpectedResult = 3)]
        [TestCase("6 + 2", ExpectedResult         = 8)]
        [TestCase("6 - 2", ExpectedResult         = 4)]
        [TestCase("6 * 2", ExpectedResult         = 12)]
        [TestCase("6 / 2", ExpectedResult         = 3)]
        [TestCase("6 % 2", ExpectedResult         = 0)]
        [TestCase("6 ^ 2", ExpectedResult         = 36)]
        [TestCase("1 + 2 + 3 + 4", ExpectedResult = 10)]
        [TestCase("1 + 2 * 3", ExpectedResult     = 7)]
        [TestCase("1 + (2 * 3)", ExpectedResult   = 7)]
        [TestCase("(1 + (2 * 3))", ExpectedResult = 7)]
        [TestCase("(1 + 2) * 3", ExpectedResult   = 9)]
        [TestCase("5 + 4 * 3 ^ 2", ExpectedResult = 41)]
        [TestCase("2 ^ 3 * 4 + 5", ExpectedResult = 37)]
        [TestCase("5 * 4 + 3 ^ 2", ExpectedResult = 29)]
        // modulo
        [TestCase("0 % 5", ExpectedResult      = 0)]
        [TestCase("1 % 5", ExpectedResult      = 1)]
        [TestCase("2 % 5", ExpectedResult      = 2)]
        [TestCase("3 % 5", ExpectedResult      = 3)]
        [TestCase("4 % 5", ExpectedResult      = 4)]
        [TestCase("5 % 5", ExpectedResult      = 0)]
        [TestCase("6 % 5", ExpectedResult      = 1)]
        [TestCase("10 % 5", ExpectedResult     = 0)]
        [TestCase("99 % 5", ExpectedResult     = 4)]
        [TestCase("10005 % 10", ExpectedResult = 5)]
        // NOT
        [TestCase("NOT(0)", ExpectedResult  = 1)]
        [TestCase("NOT(1)", ExpectedResult  = 0)]
        [TestCase("NOT(-5)", ExpectedResult = 0)]
        [TestCase("NOT(5)", ExpectedResult  = 0)]
        // AND
        [TestCase("0 AND 0", ExpectedResult = 0)]
        [TestCase("0 AND 1", ExpectedResult = 0)]
        [TestCase("1 AND 0", ExpectedResult = 0)]
        [TestCase("1 AND 1", ExpectedResult = 1)]
        // OR
        [TestCase("0 OR 0", ExpectedResult = 0)]
        [TestCase("0 OR 1", ExpectedResult = 1)]
        [TestCase("1 OR 0", ExpectedResult = 1)]
        [TestCase("1 OR 1", ExpectedResult = 1)]
        // Compare
        [TestCase("1 = 1", ExpectedResult  = 1)]
        [TestCase("1 = 2", ExpectedResult  = 0)]
        [TestCase("1 != 1", ExpectedResult = 0)]
        [TestCase("1 != 2", ExpectedResult = 1)]
        [TestCase("1 < 2", ExpectedResult  = 1)]
        [TestCase("1 <= 2", ExpectedResult = 1)]
        [TestCase("1 > 2", ExpectedResult  = 0)]
        [TestCase("1 >= 2", ExpectedResult = 0)]
        [TestCase("2 < 2", ExpectedResult  = 0)]
        [TestCase("2 <= 2", ExpectedResult = 1)]
        [TestCase("2 > 2", ExpectedResult  = 0)]
        [TestCase("2 >= 2", ExpectedResult = 1)]
        [TestCase("3 < 2", ExpectedResult  = 0)]
        [TestCase("3 <= 2", ExpectedResult = 0)]
        [TestCase("3 > 2", ExpectedResult  = 1)]
        [TestCase("3 >= 2", ExpectedResult = 1)]
        // Logical
        [TestCase("1 >= 0 AND 2 < 3", ExpectedResult  = 1)]
        [TestCase("0 >= 2 OR -4 < -5", ExpectedResult = 0)]
        [TestCase("1 > 0 AND 5", ExpectedResult       = 5)]
        [TestCase("1 > 0 OR 5", ExpectedResult        = 1)]
        [TestCase("1 > 2 AND 5", ExpectedResult       = 0)]
        [TestCase("1 > 2 OR 5", ExpectedResult        = 5)]
        [TestCase("1 AND 0 OR 1", ExpectedResult      = 1)]
        [TestCase("1 AND 0 OR 0", ExpectedResult      = 0)]
        [TestCase("1 AND (0 OR 1)", ExpectedResult    = 1)]
        // LOG
        [TestCase("LOG(10)", ExpectedResult              = 1)]
        [TestCase("LOG(10, 10)", ExpectedResult          = 1)]
        [TestCase("LOG(10000, 10)", ExpectedResult       = 4)]
        [TestCase("LOG(8, 2)", ExpectedResult            = 3)]
        [TestCase("LOG(1728, 12)", ExpectedResult        = 3)]
        [TestCase("LOG(0.000144, 0.012)", ExpectedResult = 2)]
        public float Parse(string input) => Execute(input, null).ToFloatUnsafe();

        private static BigDouble Execute(string input, ExpressionContext<BigDouble> context) {
            return ExpressionParserBigDouble.Instance.Compile(input, context, false).Invoke();
        }
    }
}