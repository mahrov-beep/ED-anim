namespace Multicast.Boosts {
    using ExpressionParser;
    using JetBrains.Annotations;
    using Numerics;
    using UniMob;

    internal class BoostData {
        public BigDoubleBoostDef         BoostDef;
        public FormulaContext<BigDouble> Ctx;
        public BoostInfo                 Info;

        [CanBeNull] public Atom<bool> Enabled;
    }
}