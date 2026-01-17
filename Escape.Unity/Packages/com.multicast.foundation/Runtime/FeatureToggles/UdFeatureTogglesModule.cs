namespace Multicast.FeatureToggles {
    using Cysharp.Threading.Tasks;

    public class UdFeatureTogglesModule : FeatureTogglesModuleBase<UdFeatureTogglesModel, UdFeatureTogglesRepo> {
        protected override UniTask Configure(Resolver resolver, UdFeatureTogglesModel features, UdFeatureTogglesRepo repo) {
            features.Configure();
            return UniTask.CompletedTask;
        }

        protected override UniTask ApplyOverrides(Resolver resolver, UdFeatureTogglesModel features, (string feature, string variant)[] overrides) {
            foreach (var (feature, variant) in overrides) {
                features.OverrideFeatureVariant(feature, variant);
            }

            return UniTask.CompletedTask;
        }

        protected override async UniTask<UdFeatureTogglesRepo> GetFeaturesRepo(Resolver resolver) {
            return await resolver.Get<UdFeatureTogglesRepo>();
        }
    }
}