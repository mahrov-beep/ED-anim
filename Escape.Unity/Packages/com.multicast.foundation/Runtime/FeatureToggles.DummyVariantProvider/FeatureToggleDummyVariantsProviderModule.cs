namespace Multicast.FeatureToggles.DummyVariantProvider {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Install;
    using TriInspector;
    using UnityEngine;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class FeatureToggleDummyVariantsProviderModule : ScriptableModule {
        [SerializeField] private DummyProvider provider;

        public override void Setup(ModuleSetup module) {
            module.Provides<IFeatureToggleVariantProvider>();
        }

        public override UniTask Install(Resolver resolver) {
            resolver.Register<IFeatureToggleVariantProvider>().To(this.provider);
            return UniTask.CompletedTask;
        }

        [Serializable]
        private class DummyProvider : IFeatureToggleVariantProvider {
            [SerializeField]
            [TableList(AlwaysExpanded = true)]
            private List<Entry> entries = new List<Entry>();

            [Serializable]
            private struct Entry {
                public string feature;
                public string variants;
            }

            public bool TryGetVariantsString(string feature, out string variantsString) {
                foreach (var entry in this.entries) {
                    if (entry.feature == feature) {
                        variantsString = entry.variants;
                        return true;
                    }
                }

                variantsString = null;
                return false;
            }
        }
    }
}