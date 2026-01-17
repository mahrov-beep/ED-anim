namespace Multicast.ExpressionParser {
    using System;
    using CodeWriter.ExpressionParser;
    using JetBrains.Annotations;

    [Serializable]
    public class FormulaPredicate : Formula<int> {
        public string expression;

        [PublicAPI]
        public string Expression => this.expression;

        public FormulaPredicate() {
        }

        public FormulaPredicate(string expression) {
            this.expression = expression;
        }

        private void Validate() {
            ExpressionParserInt.Instance.Compile(this.expression, ValidationContext, cache: false);
        }

        [PublicAPI]
        public bool Calc([NotNull] FormulaContextCore<int> context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            var compiled = context.GetCompiled(this);

            return compiled.Invoke() != 0;
        }

        internal override Expression<int> Compile(FormulaContextCore<int> context) {
            return ExpressionParserInt.Instance.Compile(this.expression, context, cache: true);
        }
    }
}