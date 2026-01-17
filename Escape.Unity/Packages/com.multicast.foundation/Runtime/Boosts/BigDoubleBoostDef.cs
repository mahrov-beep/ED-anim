namespace Multicast.Boosts {
    using System;
    using JetBrains.Annotations;
    using DirtyDataEditor;
    using ExpressionParser;

    [Serializable, DDEObject]
    public class BigDoubleBoostDef {
        public static readonly BigDoubleBoostDef Empty = new() {baseValue = FormulaBigDouble.Zero};

        [DDE("base", null)]        public FormulaBigDouble baseValue;
        [DDE("add", null)]         public FormulaBigDouble additive;
        [DDE("mul", null)]         public FormulaBigDouble multiplicative;
        [DDE("percent", null)]     public FormulaBigDouble multiplicativePercent;
        [DDE("add_percent", null)] public FormulaBigDouble additivePercent;

        [PublicAPI]
        public bool IsValid => this.Formula != null;

        [PublicAPI]
        [CanBeNull]
        public FormulaBigDouble Formula => this.baseValue ?? this.additive ?? this.multiplicative ?? this.multiplicativePercent ?? this.additivePercent;

        [PublicAPI]
        public bool IsPercentBoost => this.Formula == this.multiplicativePercent || this.Formula == this.additivePercent;

        [PublicAPI]
        public BoostType BoostType =>
            this.Formula == this.multiplicative ? BoostType.Multiplicative :
            this.Formula == this.multiplicativePercent ? BoostType.MultiplicativePercent :
            this.Formula == this.additivePercent ? BoostType.AdditivePercent :
            this.Formula == this.additive ? BoostType.Additive :
            BoostType.Base;
    }
}