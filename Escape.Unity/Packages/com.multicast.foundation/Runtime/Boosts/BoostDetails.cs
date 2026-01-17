namespace Multicast.Boosts {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Numerics;
    using UniMob;

    public class BoostDetails : ILifetimeScope {
        private readonly Func<IEnumerable<Item>> enumerateFunc;

        internal BoostDetails(Lifetime lifetime, Func<IEnumerable<Item>> enumerateFunc) {
            this.enumerateFunc = enumerateFunc;
            this.Lifetime      = lifetime;
        }

        [PublicAPI]
        public IEnumerable<Item> EnumerateAll() => this.enumerateFunc.Invoke();

        public Lifetime Lifetime { get; }

        [Serializable]
        public struct Item {
            public BoostType type;
            public BigDouble value;
            public BoostInfo info;

            public Item(BoostType type, BigDouble value, BoostInfo info) {
                this.type  = type;
                this.value = value;
                this.info  = info;
            }

            internal static Item Base(BigDouble v, BoostInfo i)                  => new(BoostType.Base, v, i);
            internal static Item Additive(BigDouble v, BoostInfo i)              => new(BoostType.Additive, v, i);
            internal static Item Multiplicative(BigDouble v, BoostInfo i)        => new(BoostType.Multiplicative, v, i);
            internal static Item AdditivePercent(BigDouble v, BoostInfo i)       => new(BoostType.AdditivePercent, v, i);
            internal static Item MultiplicativePercent(BigDouble v, BoostInfo i) => new(BoostType.MultiplicativePercent, v, i);
        }
    }
}