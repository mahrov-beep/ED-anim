namespace Multicast.FeatureToggles {
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Routes;
    using UI.Widgets;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    internal class FeatureTogglesUI {
        private readonly FeatureTogglesModel features;
        private readonly IFeatureToggleRepo  repo;

        public FeatureTogglesUI(FeatureTogglesModel features, IFeatureToggleRepo repo) {
            this.features = features;
            this.repo     = repo;
        }

        public static bool ShowFeatureTogglesSelectorOnce {
            get => PlayerPrefs.GetInt("Multicast_ShowFeatureTogglsSelectorOnce", 1) != 0;
            set => PlayerPrefs.SetInt("Multicast_ShowFeatureTogglsSelectorOnce", value ? 1 : 0);
        }

        public async UniTask<(string featureKey, string variant)[]> ShowFeatureTogglesOverrideUI() {
            var items = this.features.EnumerateFeatures()
                .Select(it => new FeatureOverrideSelectorItem(this, it, this.features.EnumerateVariantsForFeature(it).ToList()))
                .ToList();

            await App.Current.GetNavigator(AppNavigatorType.System).Push(new SlideDownRoute(
                new RouteSettings("feature_toggles_selector", RouteModalType.Popup),
                (context, _, _) => new DebugListWidget("Feature Toggles") {
                    Items = {
                        items.Where(it => it.Variants.Count > 0).Select(BuildFeature),
                    },
                }
            )).PopTask;

            return items
                .Where(it => it.Overriden)
                .Select(it => (it.Feature, it.Variant))
                .ToArray();

            Widget BuildFeature(FeatureOverrideSelectorItem item) {
                var hasVariant  = !string.IsNullOrEmpty(item.Variant);
                var variantsStr = item.Variants.Aggregate((a, b) => a + b);

                return new DebugListItemWidget {
                    PrimaryText        = $"[{variantsStr}] {item.Feature}",
                    SecondaryText      = hasVariant ? item.Variant : "Default",
                    PrimaryTextColor   = hasVariant ? Color.red : Color.black,
                    SecondaryTextColor = hasVariant ? Color.black : new Color(0.9f, 0.9f, 0.9f),
                    OnClick            = () => item.NextVariant(),
                };
            }
        }

        private class FeatureOverrideSelectorItem : ILifetimeScope {
            public string       Feature  { get; }
            public List<string> Variants { get; }

            public FeatureOverrideSelectorItem(FeatureTogglesUI featuresUi, string feature, List<string> variants) {
                this.Feature  = feature;
                this.Variants = variants;
                this.Variant = featuresUi.repo.TryGetFeature(feature, out var variantData)
                    ? variantData.Variant
                    : string.Empty;
            }

            public Lifetime Lifetime { get; } = Lifetime.Eternal;

            [Atom] public string Variant   { get; set; }
            [Atom] public bool   Overriden { get; set; }

            public void NextVariant() {
                var nextIndex = 1 + this.Variants.IndexOf(this.Variant);

                this.Variant = nextIndex == this.Variants.Count
                    ? string.Empty
                    : this.Variants[nextIndex];
                this.Overriden = true;
            }
        }
    }
}