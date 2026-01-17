namespace Multicast.Boosts {
    using System;
    using Numerics;

    [Serializable]
    public struct BoostSummary {
        public BigDouble baseValue;
        public BigDouble additive;
        public BigDouble multiplicative;
        public BigDouble additivePercent;
        public BigDouble multiplicativePercent;
    }
}