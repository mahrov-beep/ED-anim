namespace Multicast.ExpressionParser {
    using System;
    using System.Collections.Generic;
    using Numerics;
    using CodeWriter.ExpressionParser;

    [Serializable]
    public class FormulaBigDoubleList : FormulaBaseList<BigDouble, ExpressionParser<BigDouble>> {
        public FormulaBigDoubleList()
            : base(ExpressionParserBigDouble.Instance) {
        }

        public FormulaBigDoubleList(List<string> expressions)
            : base(ExpressionParserBigDouble.Instance, expressions) {
        }

        protected override BigDouble Cast(int value) => value;
    }
}