namespace Multicast.FeatureToggles {
    using Cheats;
    using Cysharp.Threading.Tasks;
    using Install;

    public abstract class FeatureTogglesModuleBase<TModel, TRepo> : ScriptableModule, IScriptableModuleWithPriority
        where TModel : FeatureTogglesModel
        where TRepo : class, IFeatureToggleRepo {
        public int Priority => ScriptableModulePriority.LATE;

        public override void Setup(ModuleSetup module) {
            module.Provides<FeatureTogglesModel>();
        }

        protected abstract UniTask<TRepo> GetFeaturesRepo(Resolver resolver);

        protected abstract UniTask Configure(Resolver resolver, TModel features, TRepo repo);

        protected abstract UniTask ApplyOverrides(Resolver resolver, TModel features, (string feature, string variant)[] overrides);

        public override async UniTask Install(Resolver resolver) {
            var repo = await this.GetFeaturesRepo(resolver);

            var features = await resolver.Register<FeatureTogglesModel>().ToAsync<TModel>();

            await this.Configure(resolver, features, repo);

            var cheatButtons = await resolver.Get<ICheatButtonsRegistry>();

            cheatButtons.RegisterAction("Show FeatureToggles Selector After Restart",
                () => FeatureTogglesUI.ShowFeatureTogglesSelectorOnce = true);

#if UNITY_EDITOR || FEATURE_TOGGLES_SHOW_SELECTOR_UI
            if (FeatureTogglesUI.ShowFeatureTogglesSelectorOnce) {
                FeatureTogglesUI.ShowFeatureTogglesSelectorOnce = false;

                var featuresUi        = new FeatureTogglesUI(features, repo);
                var overridenFeatures = await featuresUi.ShowFeatureTogglesOverrideUI();

                await this.ApplyOverrides(resolver, features, overridenFeatures);
            }
#endif
        }
    }
}