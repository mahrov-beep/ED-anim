namespace Multicast.ExpressionParser {
    using System;
    using Numerics;
    using CodeWriter.ExpressionParser;

    [Serializable]
    public class FormulaBigDouble : FormulaBase<BigDouble, ExpressionParser<BigDouble>> {
        public static readonly FormulaBigDouble Zero = new FormulaBigDouble("0");

        public FormulaBigDouble() : base(ExpressionParserBigDouble.Instance) {
        }

        public FormulaBigDouble(string expression) : base(ExpressionParserBigDouble.Instance, expression) {
        }
    }
}