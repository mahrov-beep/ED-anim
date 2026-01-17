namespace Multicast.ExpressionParser {
    using System;
    using CodeWriter.ExpressionParser;
    using JetBrains.Annotations;

    [Serializable]
    public abstract class FormulaBase<TValue, TParser> : Formula<TValue>
        where TParser : ExpressionParser<TValue>
        where TValue : struct {
        public string expression;

        [NonSerialized] private TParser parser;

        public bool IsValid => !string.IsNullOrEmpty(this.expression);

        [PublicAPI]
        public string Expression => this.expression;

        protected FormulaBase(TParser parser) {
            this.parser = parser;
        }

        protected FormulaBase(TParser parser, string expression) : this(parser) {
            this.expression = expression;
        }

        [PublicAPI]
        public TValue Calc([NotNull] FormulaContextCore<TValue> ctx) {
            if (ctx == null) {
                throw new ArgumentNullException(nameof(ctx));
            }

            return ctx.GetCompiled(this).Invoke();
        }

        private void Validate() {
            this.parser.Compile(this.expression, ValidationContext, cache: false);
        }

        internal sealed override Expression<TValue> Compile(FormulaContextCore<TValue> context) {
            if (!this.IsValid) {
                throw new InvalidOperationException("Has no formula or formula invalid");
            }

            return this.parser.Compile(this.expression, context, cache: true);
        }
    }
}