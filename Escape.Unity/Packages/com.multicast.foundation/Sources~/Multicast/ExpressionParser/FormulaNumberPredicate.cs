namespace Multicast.ExpressionParser {
    using System;
    using CodeWriter.ExpressionParser;
    using JetBrains.Annotations;
    using Numerics;

    [Serializable]
    public class FormulaNumberPredicate : Formula<BigDouble> {
        public string expression;

        [PublicAPI]
        public string Expression => this.expression;

        public FormulaNumberPredicate() {
        }

        public FormulaNumberPredicate(string expression) {
            this.expression = expression;
        }

        private void Validate() {
            ExpressionParserBigDouble.Instance.Compile(this.expression, ValidationContext, cache: false);
        }

        [PublicAPI]
        public bool Calc([NotNull] FormulaContextCore<BigDouble> context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            var compiled = context.GetCompiled(this);

            return compiled.Invoke() != BigDouble.Zero;
        }

        internal override Expression<BigDouble> Compile(FormulaContextCore<BigDouble> context) {
            return ExpressionParserBigDouble.Instance.Compile(this.expression, context, cache: true);
        }
    }
}