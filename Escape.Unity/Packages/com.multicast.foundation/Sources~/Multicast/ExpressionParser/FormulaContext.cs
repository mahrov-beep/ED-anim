namespace Multicast.ExpressionParser {
    using System;
    using System.Collections.Generic;
    using CodeWriter.ExpressionParser;
    using JetBrains.Annotations;

    public class FormulaContextCore<TValue> : ExpressionContext<TValue>, IDisposable
        where TValue : struct {
        [NonSerialized]
        private readonly Dictionary<Formula<TValue>, Expression<TValue>> compiledExpressions = new Dictionary<Formula<TValue>, Expression<TValue>>();

        public FormulaContextCore(Action<IDisposable> lifetimeRegistrator, ExpressionContext<TValue> parent = null,
            Func<string, Expression<TValue>> unregisteredVariableResolver = null)
            : base(parent, unregisteredVariableResolver) {
            lifetimeRegistrator(this);
        }

        void IDisposable.Dispose() {
            this.compiledExpressions.Clear();
        }

        internal Expression<TValue> GetCompiled([NotNull] Formula<TValue> formula) {
            if (formula == null) {
                throw new ArgumentNullException(nameof(formula));
            }

            if (this.compiledExpressions.TryGetValue(formula, out var compiled)) {
                return compiled;
            }

            compiled = formula.Compile(this);
            this.compiledExpressions.Add(formula, compiled);
            return compiled;
        }
    }
}