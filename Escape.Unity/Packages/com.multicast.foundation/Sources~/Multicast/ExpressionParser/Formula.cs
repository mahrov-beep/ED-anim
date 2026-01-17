namespace Multicast.ExpressionParser {
    using CodeWriter.ExpressionParser;

    public abstract class Formula<TValue>
        where TValue : struct {
        protected static readonly ExpressionContext<TValue> ValidationContext = new ExpressionContext<TValue>(unregisteredVariableResolver: _ => () => default);

        internal abstract Expression<TValue> Compile(FormulaContextCore<TValue> context);
    }
}