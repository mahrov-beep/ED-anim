namespace Multicast.ExpressionParser {
    using System;
    using System.Collections.Generic;
    using CodeWriter.ExpressionParser;
    using JetBrains.Annotations;

    [Serializable]
    public abstract class FormulaBaseList<TValue, TParser>
        where TParser : ExpressionParser<TValue>
        where TValue : struct {
        public List<string> expressions = new List<string>();

        [NonSerialized] private List<TValue?> values = new List<TValue?>();
        [NonSerialized] private ExpressionContext<TValue> context;
        [NonSerialized] private TParser parser;
        [NonSerialized] private Dictionary<string, Expression<TValue>> compiledExpressions = new Dictionary<string, Expression<TValue>>();
        [NonSerialized] private TValue currentN;

        [PublicAPI]
        public bool IsValid => this.expressions.Count != 0;

        [PublicAPI]
        public List<string> Expressions => this.expressions;

        protected FormulaBaseList(TParser parser) {
            this.parser = parser;
        }

        protected FormulaBaseList(TParser parser, List<string> expressions) : this(parser) {
            this.expressions = expressions;

            this.EvaluateAndCacheAll();
        }

        private void EvaluateAndCacheAll() {
            for (var i = 0; i < this.expressions.Count; i++) {
                this.EvaluateAndCache(i);
            }
        }

        [PublicAPI]
        public TValue this[int index] {
            get {
                if (index < 0) {
                    throw new ArgumentOutOfRangeException(nameof(index), "index must be positive");
                }

                if (index < this.values.Count) {
                    var value = this.values[index];
                    if (value.HasValue) {
                        return value.Value;
                    }
                }

                if (!this.IsValid) {
                    throw new InvalidOperationException("Has no formula or formula invalid");
                }

                return this.EvaluateAndCache(index);
            }
        }

        private TValue EvaluateAndCache(int index) {
            var count = index + 1;
            while (this.values.Count < count) {
                this.values.Add(null);
            }

            if (this.values[index] == null) {
                this.currentN = this.Cast(index);

                if (this.context == null) {
                    this.context = new ExpressionContext<TValue>();
                    this.context.RegisterVariable("n", () => this.currentN);
                }

                var exprIndex = Math.Min(index, this.expressions.Count - 1);
                var expression = this.expressions[exprIndex];

                if (!this.compiledExpressions.TryGetValue(expression, out var compiledExpression)) {
                    compiledExpression = this.parser.Compile(expression, this.context, cache: true);
                    this.compiledExpressions.Add(expression, compiledExpression);
                }

                this.values[index] = compiledExpression.Invoke();
            }

            return this.values[index].Value;
        }

        protected abstract TValue Cast(int value);
    }
}