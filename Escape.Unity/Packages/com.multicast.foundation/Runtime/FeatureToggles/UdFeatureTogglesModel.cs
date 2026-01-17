namespace Multicast.FeatureToggles {
    using Analytics;
    using Collections;
    using UniMob;

    public class UdFeatureTogglesModel : FeatureTogglesModel {
        private readonly UdFeatureTogglesRepo                     repo;
        private readonly LookupCollection<FeatureToggleDef> defs;
        private readonly IUserDataService                   userDataService;

        protected UdFeatureTogglesModel(
            Lifetime lifetime,
            AppSharedFormulaContext appSharedFormulaContext,
            UdFeatureTogglesRepo repo,
            LookupCollection<FeatureToggleDef> defs,
            IFeatureToggleVariantProvider variantProvider,
            IAnalytics analytics, IUserDataService userDataService)
            : base(lifetime, appSharedFormulaContext, repo, defs, variantProvider, analytics) {
            this.repo            = repo;
            this.defs            = defs;
            this.userDataService = userDataService;
        }

        internal void Configure() {
            using (this.userDataService.Root.BeginTransactionScope("Features.Configure")) {
                foreach (var featureDef in this.defs.Items) {
                    if (this.TryGetNewFeatureVariant(featureDef, out var variant)) {
                        var variantData = this.repo.GetOrCreateFeature(featureDef.key, out var variantCreated);

                        variantData.SetVariant(variant);

                        if (!variantCreated) {
                            variantData.IncrementTimesChanged();
                        }
                    }
                }

                this.repo.SetIsOldUser();
            }
        }

        internal void OverrideFeatureVariant(string feature, string variant) {
            this.userDataService.Root.BeginTransaction("Features.OverrideFeatureVariant");
            {
                if (string.IsNullOrEmpty(variant)) {
                    this.repo.RemoveFeature(feature);
                }
                else {
                    this.repo.GetOrCreateFeature(feature, out _).SetVariant(variant);
                }
            }
            this.userDataService.Root.CommitTransaction();
        }
    }
}