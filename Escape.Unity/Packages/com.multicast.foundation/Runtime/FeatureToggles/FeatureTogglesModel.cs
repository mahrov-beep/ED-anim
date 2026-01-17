namespace Multicast.FeatureToggles {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Analytics;
    using CodeWriter.ExpressionParser;
    using Collections;
    using ExpressionParser;
    using JetBrains.Annotations;
    using Numerics;
    using UniMob;
    using UnityEngine;

    public class FeatureTogglesModel : Model {
        private readonly IFeatureToggleRepo                 repo;
        private readonly LookupCollection<FeatureToggleDef> defs;

        private readonly IFeatureToggleVariantProvider variantProvider;
        private readonly IAnalytics              analytics;

        protected readonly FormulaContext<int> FormulaContext;

        protected FeatureTogglesModel(Lifetime lifetime, AppSharedFormulaContext appSharedFormulaContext,
            IFeatureToggleRepo repo,
            LookupCollection<FeatureToggleDef> defs, IFeatureToggleVariantProvider variantProvider, IAnalytics analytics)
            : base(lifetime) {
            this.repo            = repo;
            this.defs            = defs;
            this.variantProvider = variantProvider;
            this.analytics       = analytics;

            this.FormulaContext = new FormulaContext<int>(lifetime, appSharedFormulaContext);

            var formattedVariableNamesMapping = defs.Items.ToDictionary(it => $"F_{it.key}", it => it.key);

            appSharedFormulaContext.RegisterGlobalVariableResolver(ResolveFeatureVariable);

            Expression<int> ResolveFeatureVariable(string variableName) {
                if (formattedVariableNamesMapping.TryGetValue(variableName, out var key)) {
                    return () => this.GetValue(key);
                }

                return null;
            }
        }

        public (string feature, string variant)[] GetNewOverrides() {
            var result = new List<(string feature, string variant)>();

            foreach (var featureDef in this.defs.Items) {
                if (this.TryGetNewFeatureVariant(featureDef, out var variant)) {
                    result.Add((featureDef.key, variant));
                }
            }

            return result.ToArray();
        }

        protected bool TryGetNewFeatureVariant(FeatureToggleDef featureDef, out string newVariant) {
            newVariant = null;

            // no AB in remote-config
            if (!this.variantProvider.TryGetVariantsString(featureDef.key, out var variantsString)) {
                return false;
            }

            var oldUsersAllowed = variantsString.StartsWith("+");
            var overrideAllowed = variantsString.StartsWith("*");

            // AB already configured
            if (this.repo.TryGetFeature(featureDef.key, out var variantData) && !overrideAllowed) {
                return false;
            }

            // skip AB for old user
            if (this.repo.IsOldUser && !oldUsersAllowed && !overrideAllowed) {
                return false;
            }

            var variantList = featureDef.variants.Keys
                .Where(v => variantsString.Contains(v))
                .ToList();

            // no matched variants
            if (variantList.Count == 0) {
                return false;
            }

            var variant = variantList[UnityEngine.Random.Range(0, variantList.Count)];

            if (variantData.Variant == variant) {
                return false;
            }

            this.analytics.Send("ab_variant",
                new AnalyticsArg(featureDef.key, $"{variantsString}_{variant}_{variantData.TimesChanged}")
            );

            newVariant = variant;
            return true;
        }

        [PublicAPI]
        public IEnumerable<string> EnumerateFeatures() {
            return this.defs.Items.Select(it => it.key);
        }

        [PublicAPI]
        public IEnumerable<string> EnumerateVariantsForFeature(string feature) {
            if (this.defs.TryGet(feature, out var def)) {
                return def.variants.Keys;
            }

            return Array.Empty<string>();
        }

        [PublicAPI]
        public bool IsFeatureExist(FeatureToggleName name) {
            return this.defs.TryGet(name.Name, out _);
        }

        [PublicAPI]
        public bool IsDisabled(FeatureToggleName name) {
            return !this.IsEnabled(name);
        }

        [PublicAPI]
        public bool IsEnabled(FeatureToggleName name) {
            return this.GetValue(name) != BigDouble.Zero;
        }

        [PublicAPI]
        public string GetText(FeatureToggleName name) {
            if (!this.defs.TryGet(name.Name, out var def)) {
                Debug.LogError($"Feature '{name.Name}' not exists");
                return string.Empty;
            }

            if (this.TryGetFeatureVariant(def.key, out var variant) &&
                TryGetText(def.variants, variant, out var variantValue)) {
                return variantValue;
            }

            if (TryGetText(def.platforms, App.Platform, out var platformValue)) {
                return platformValue;
            }

            if (def.defaults?.text != null) {
                return def.defaults.text;
            }

            return string.Empty;
        }

        [PublicAPI]
        public int GetValue(FeatureToggleName name) {
            if (!this.defs.TryGet(name.Name, out var def)) {
                Debug.LogError($"Feature '{name.Name}' not exists");
                return 0;
            }

            if (this.TryGetFeatureVariant(def.key, out var variant) &&
                TryCalcValue(def.variants, variant, this.FormulaContext, out var variantValue)) {
                return variantValue;
            }

            if (TryCalcValue(def.platforms, App.Platform, this.FormulaContext, out var platformValue)) {
                return platformValue;
            }

            if (def.defaults?.formula != null) {
                return def.defaults.formula.Calc(this.FormulaContext);
            }

            return 0;
        }

        private bool TryGetFeatureVariant(string feature, out string variant) {
            if (this.repo.TryGetFeature(feature, out var data)) {
                variant = data.Variant;
                return true;
            }

            variant = null;
            return false;
        }

        private static bool TryCalcValue(Dictionary<string, FeatureToggleValueDef> dict, string variant,
            FormulaContext<int> ctx, out int value) {
            if (!dict.TryGetValue(variant, out var valueDef)) {
                value = 0;
                return false;
            }

            if (valueDef.formula == null) {
                value = 0;
                return false;
            }

            value = valueDef.formula.Calc(ctx);
            return true;
        }

        private static bool TryGetText(Dictionary<string, FeatureToggleValueDef> dict, string variant, out string text) {
            if (!dict.TryGetValue(variant, out var valueDef)) {
                text = string.Empty;
                return false;
            }

            if (valueDef.text == null) {
                text = string.Empty;
                return false;
            }

            text = valueDef.text;
            return true;
        }
    }
}