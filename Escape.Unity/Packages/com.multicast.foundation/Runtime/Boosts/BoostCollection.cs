namespace Multicast.Boosts {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using ExpressionParser;
    using Numerics;
    using UniMob;
    using UnityEngine;

    public class BoostCollection<TKey> where TKey : struct {
        private readonly Lifetime                    lifetime;
        private readonly Func<TKey, BoostOutputType> outputTypeSelector;
        private readonly FormulaBigDouble            valueFormula = new("value");
        private readonly Dictionary<TKey, Boost>     boosts       = new();

        public Lifetime Lifetime => this.lifetime;

        public BoostCollection(Lifetime lifetime, Func<TKey, BoostOutputType> outputTypeSelector = null) {
            this.lifetime           = lifetime;
            this.outputTypeSelector = outputTypeSelector ?? (_ => BoostOutputType.Value);
        }

        private void Register(Lifetime lt, TKey key, BoostData data) {
            this.GetOrCreateBoost(key).Register(lt, data);
        }

        [PublicAPI]
        public void RegisterBoost(Lifetime lt, TKey key, BigDoubleBoostDef boostDef, FormulaContext<BigDouble> ctx, BoostInfo info, Atom<bool> stateEnabled = null) {
            if (boostDef == null) {
                return;
            }

            if (ctx == null) {
                Debug.LogError($"Context is null for {key}");
                return;
            }

            var data = new BoostData {
                BoostDef = boostDef,
                Ctx      = ctx,
                Info     = info,
                Enabled  = stateEnabled,
            };

            this.Register(lt, key, data);
        }

        [PublicAPI] public void RegisterBaseValue(Lifetime lt, TKey key, BigDouble value, BoostInfo info) {
            if (value == BigDouble.Zero) {
                return;
            }

            this.RegisterBoost(lt, key, BoostType.Base, () => value, info);
        }

        [PublicAPI] public void RegisterAdditiveValue(Lifetime lt, TKey key, BigDouble value, BoostInfo info) {
            if (value == BigDouble.Zero) {
                return;
            }

            this.RegisterBoost(lt, key, BoostType.Additive, () => value, info);
        }

        [PublicAPI] public void RegisterAdditivePercentValue(Lifetime lt, TKey key, BigDouble value, BoostInfo info) {
            if (value == BigDouble.Zero) {
                return;
            }

            this.RegisterBoost(lt, key, BoostType.AdditivePercent, () => value, info);
        }

        [PublicAPI] public void RegisterBoost(Lifetime lt, TKey key, BoostType type, Func<BigDouble> func, BoostInfo info) {
            var ctx = new FormulaContext<BigDouble>(lt);
            ctx.RegisterVariable("value", () => func.Invoke());

            var boostDef = new BigDoubleBoostDef();
            switch (type) {
                case BoostType.Base:
                    boostDef.baseValue = this.valueFormula;
                    break;
                case BoostType.Additive:
                    boostDef.additive = this.valueFormula;
                    break;
                case BoostType.Multiplicative:
                    boostDef.multiplicative = this.valueFormula;
                    break;
                case BoostType.AdditivePercent:
                    boostDef.additivePercent = this.valueFormula;
                    break;
                case BoostType.MultiplicativePercent:
                    boostDef.multiplicativePercent = this.valueFormula;
                    break;
                default:
                    throw new InvalidOperationException("Unknown boost type");
            }

            this.RegisterBoost(lt, key, boostDef, ctx, info);
        }

        [PublicAPI] public Boost Get(TKey key) => this.GetOrCreateBoost(key);

        [PublicAPI] public bool HasAnyBoost(TKey key) => this.GetOrCreateBoost(key).Primary.HasAnyBoost;

        [PublicAPI] public BoostOutputType GetOutputType(TKey key) => this.GetOrCreateBoost(key).OutputType;

        [PublicAPI] public BigDouble GetValue(TKey key)  => this.GetOrCreateBoost(key).Primary.Value;
        [PublicAPI] public BigDouble AsPercent(TKey key) => this.GetOrCreateBoost(key).Primary.ValueAsPercent;
        [PublicAPI] public BigDouble AsNumber(TKey key)  => this.GetOrCreateBoost(key).Primary.ValueAsNumber;

        [PublicAPI] public BoostSummary GetSummary(TKey key) => this.GetOrCreateBoost(key).Primary.Summary;

        [PublicAPI] public BoostDetails CreateDetails(TKey key) => this.GetOrCreateBoost(key).Primary.Details;

        private Boost GetOrCreateBoost(TKey key) {
            if (this.boosts.TryGetValue(key, out var boost)) {
                return boost;
            }

            var outputType = this.outputTypeSelector.Invoke(key);
            boost = new Boost(this.lifetime, outputType);
            return this.boosts[key] = boost;
        }
    }
}