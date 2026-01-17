namespace Multicast.ExpressionParser {
    using System;
    using CodeWriter.ExpressionParser;

    [Serializable]
    public class FormulaInt : FormulaBase<int, ExpressionParser<int>> {
        public static readonly FormulaInt Zero = new FormulaInt("0");

        public FormulaInt() : base(ExpressionParserInt.Instance) {
        }

        public FormulaInt(string expression) : base(ExpressionParserInt.Instance, expression) {
        }
    }
}