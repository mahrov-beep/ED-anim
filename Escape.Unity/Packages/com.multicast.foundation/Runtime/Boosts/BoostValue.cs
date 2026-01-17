namespace Multicast.Boosts {
    using System;
    using System.Collections.Generic;
    using ExpressionParser;
    using Numerics;
    using UniMob;
    using UnityEngine;

    public class BoostValue : ILifetimeScope {
        public static Func<bool> ShowNothingBoostsInDetails = null;

        private static readonly Func<BoostInfo, bool> AllFilter = _ => true;

        private readonly Boost                 parent;
        private readonly Func<BoostInfo, bool> filter;

        internal BoostValue(Boost parent, Func<BoostInfo, bool> filter = null) {
            this.parent = parent;
            this.filter = filter ?? AllFilter;
        }

        public Lifetime Lifetime => this.parent.Lifetime;

        [Atom] public bool HasAnyBoost {
            get {
                foreach (var it in this.parent.List) {
                    if (!this.filter.Invoke(it.Info)) {
                        continue;
                    }

                    return true;
                }

                return false;
            }
        }

        [Atom] public BigDouble Value {
            get {
                var baseValue             = this.BaseValue;
                var additive              = this.AdditiveValue;
                var multiplicative        = this.MultiplicativeValue;
                var additivePercent       = this.AdditivePercentValue;
                var multiplicativePercent = this.MultiplicativePercentValue;

                return this.parent.OutputType switch {
                    BoostOutputType.Percent => -1 + (1 + baseValue + additive) * multiplicative * additivePercent * multiplicativePercent,
                    BoostOutputType.Value => (baseValue + additive) * multiplicative * additivePercent * multiplicativePercent,
                    _ => throw new InvalidOperationException("Unknown boost output type"),
                };
            }
        }

        [Atom] public BigDouble ValueAsPercent {
            get {
                if (this.parent.OutputType != BoostOutputType.Percent) {
                    Debug.LogError("ValueAsPercent method must only be called on boost with OutputType.Percent");
                }

                return this.Value;
            }
        }

        [Atom] public BigDouble ValueAsNumber {
            get {
                if (this.parent.OutputType != BoostOutputType.Value) {
                    Debug.LogError("ValueAsNumber method must only be called on boost with OutputType.Value");
                }

                return this.Value;
            }
        }

        [Atom] public BigDouble BaseValue =>
            this.CalculateBoostValue(0, it => it.baseValue, (result, v) => BigDouble.Max(result, v));

        [Atom] public BigDouble AdditiveValue =>
            this.CalculateBoostValue(0, it => it.additive, (result, v) => result + v);

        [Atom] public BigDouble MultiplicativeValue =>
            this.CalculateBoostValue(1, it => it.multiplicative, (result, v) => result * v);

        [Atom] public BigDouble AdditivePercentValue =>
            this.CalculateBoostValue(1, it => it.additivePercent, (result, v) => result + v * 0.01f);

        [Atom] public BigDouble MultiplicativePercentValue =>
            this.CalculateBoostValue(1, it => it.multiplicativePercent, (result, v) => result * (1.0f + v * 0.01f));

        [Atom] public BoostSummary Summary {
            get {
                BoostSummary summary;
                summary.baseValue             = this.BaseValue;
                summary.additive              = this.AdditiveValue;
                summary.multiplicative        = this.MultiplicativeValue;
                summary.additivePercent       = this.AdditivePercentValue;
                summary.multiplicativePercent = this.MultiplicativePercentValue;
                return summary;
            }
        }

        [Atom] public BoostDetails Details {
            get {
                return new BoostDetails(this.Lifetime, Enumerate);

                IEnumerable<BoostDetails.Item> Enumerate() {
                    this.parent.Version.Get();

                    foreach (var it in this.parent.List) {
                        if (!this.filter.Invoke(it.Info)) {
                            continue;
                        }

                        if (it.Enabled != null && !it.Enabled.Value) {
                            continue;
                        }

                        if (TryCalcAndNotEqualTo(it, it.BoostDef.baseValue, 0, out var baseValue)) {
                            yield return BoostDetails.Item.Base(baseValue, it.Info);
                        }

                        if (TryCalcAndNotEqualTo(it, it.BoostDef.additive, 0, out var addValue)) {
                            yield return BoostDetails.Item.Additive(addValue, it.Info);
                        }

                        if (TryCalcAndNotEqualTo(it, it.BoostDef.multiplicative, 1, out var multValue)) {
                            yield return BoostDetails.Item.Multiplicative(multValue, it.Info);
                        }

                        if (TryCalcAndNotEqualTo(it, it.BoostDef.multiplicativePercent, 0, out var multPercent)) {
                            yield return BoostDetails.Item.MultiplicativePercent(multPercent * 0.01f, it.Info);
                        }

                        if (TryCalcAndNotEqualTo(it, it.BoostDef.additivePercent, 0, out var addPercent)) {
                            yield return BoostDetails.Item.AdditivePercent(addPercent * 0.01f, it.Info);
                        }
                    }
                }

                static bool TryCalcAndNotEqualTo(BoostData data, FormulaBigDouble formula, BigDouble cmp, out BigDouble value) {
                    if (formula == null || !formula.IsValid) {
                        value = default;
                        return false;
                    }

                    value = formula.Calc(data.Ctx);
                    return value != cmp || (ShowNothingBoostsInDetails?.Invoke() ?? false);
                }
            }
        }

        private BigDouble CalculateBoostValue(BigDouble initial,
            Func<BigDoubleBoostDef, FormulaBigDouble> selector,
            Func<BigDouble, BigDouble, BigDouble> aggregator) {
            var value = initial;

            this.parent.Version.Get();

            foreach (var it in this.parent.List) {
                if (!this.filter.Invoke(it.Info)) {
                    continue;
                }

                if (it.Enabled != null && !it.Enabled.Value) {
                    continue;
                }

                var formula = selector.Invoke(it.BoostDef);
                if (formula == null) {
                    continue;
                }

                value = aggregator.Invoke(value, formula.Calc(it.Ctx));
            }

            return value;
        }
    }
}