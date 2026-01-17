namespace Multicast.ExpressionParser {
    using System;
    using System.Collections.Generic;
    using CodeWriter.ExpressionParser;

    [Serializable]
    public class FormulaIntList : FormulaBaseList<int, ExpressionParser<int>> {
        public FormulaIntList()
            : base(ExpressionParserInt.Instance) {
        }

        public FormulaIntList(List<string> expressions)
            : base(ExpressionParserInt.Instance, expressions) {
        }

        protected override int Cast(int value) => value;
    }
}