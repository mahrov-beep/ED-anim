namespace Multicast.ExpressionParser {
    using System;
    using System.Collections.Generic;
    using CodeWriter.ExpressionParser;

    [Serializable]
    public class FormulaFloatList : FormulaBaseList<float, ExpressionParser<float>> {
        public FormulaFloatList()
            : base(FloatExpressionParser.Instance) {
        }

        public FormulaFloatList(List<string> expressions)
            : base(FloatExpressionParser.Instance, expressions) {
        }

        protected override float Cast(int value) => value;
    }
}