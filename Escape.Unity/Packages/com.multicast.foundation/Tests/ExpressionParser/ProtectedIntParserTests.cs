namespace GreenButtonGames.STF {
    using CodeWriter.ExpressionParser;
    using Multicast.ExpressionParser;
    using Multicast.Numerics;
    using NUnit.Framework;

    public class ProtectedIntParserTest {
        [Test]
        // Constants
        [TestCase("1", ExpectedResult   = 1)]
        [TestCase("123", ExpectedResult = 123)]
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
        //
        [TestCase("LOG(10)", ExpectedResult        = 1)]
        [TestCase("LOG(10, 10)", ExpectedResult    = 1)]
        [TestCase("LOG(10000, 10)", ExpectedResult = 4)]
        [TestCase("LOG(8, 2)", ExpectedResult      = 3)]
        [TestCase("LOG(1728, 12)", ExpectedResult  = 3)]
        public int Parse(string input) => Execute(input, null).Value;

        private static ProtectedInt Execute(string input, ExpressionContext<ProtectedInt> context) {
            return ExpressionParserProtectedInt.Instance.Compile(input, context, false).Invoke();
        }
    }
}