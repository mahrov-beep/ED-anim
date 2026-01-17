namespace Game.UI.Modules {
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Multicast.Analytics;
    using Multicast.Collections;
    using Multicast.FeatureToggles;
    using Multicast.Install;
    using Multicast.Server;
    using Shared.UserProfile.Commands.FeatureToggles;
    using Shared.UserProfile.Data;
    using UniMob;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class SdFeatureTogglesModule : FeatureTogglesModuleBase<SdFeatureTogglesModule.SdFeatureTogglesModel, SdFeatureToggleRepo> {
        protected override async UniTask<SdFeatureToggleRepo> GetFeaturesRepo(Resolver resolver) {
            await resolver.Get<IServerSettings>();

            return (await resolver.Get<SdUserProfile>()).FeatureToggles;
        }

        protected override async UniTask Configure(Resolver resolver, SdFeatureTogglesModel features, SdFeatureToggleRepo repo) {
            /*
            var newOverrides = features.GetNewOverrides();

            if (newOverrides.Length > 0) {
                await this.ApplyOverrides(resolver, features, newOverrides);
            }
            */
        }

        protected override async UniTask ApplyOverrides(Resolver resolver, SdFeatureTogglesModel features, (string feature, string variant)[] overrides) {
            /*
            await App.Server.ExecuteUserProfile(new UserProfileSetFeatureTogglesCommand {
                Toggles = overrides.Select(it => new UserProfileSetFeatureTogglesCommand.ToggleState {
                    FeatureToggleKey = it.feature,
                    Variant          = it.variant,
                }).ToArray(),
            }, ServerCallRetryStrategy.RetryWithUserDialog);
            */
        }

        public sealed class SdFeatureTogglesModel : FeatureTogglesModel {
            public SdFeatureTogglesModel(
                Lifetime lifetime,
                AppSharedFormulaContext appSharedFormulaContext,
                SdUserProfile profile,
                LookupCollection<FeatureToggleDef> defs,
                IFeatureToggleVariantProvider variantProvider,
                IAnalytics analytics)
                : base(lifetime, appSharedFormulaContext, profile.FeatureToggles, defs, variantProvider, analytics) {
            }
        }
    }
}