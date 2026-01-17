namespace Multicast.ExpressionParser {
    using System;
    using CodeWriter.ExpressionParser;
    using UniMob;

    public class FormulaContext<TValue> : FormulaContextCore<TValue>
        where TValue : struct {
        public FormulaContext(Lifetime lifetime, ExpressionContext<TValue> parent = null,
            Func<string, Expression<TValue>> unregisteredVariableResolver = null)
            : base(it => lifetime.Register(it), parent, unregisteredVariableResolver) {
        }
    }
}