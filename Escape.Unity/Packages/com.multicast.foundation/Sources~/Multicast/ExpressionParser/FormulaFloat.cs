namespace Multicast.ExpressionParser {
    using System;
    using CodeWriter.ExpressionParser;

    [Serializable]
    public class FormulaFloat : FormulaBase<float, ExpressionParser<float>> {
        public static readonly FormulaFloat Zero = new FormulaFloat("0");

        public FormulaFloat() : base(FloatExpressionParser.Instance) {
        }

        public FormulaFloat(string expression) : base(FloatExpressionParser.Instance, expression) {
        }
    }
}