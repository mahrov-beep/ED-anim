namespace Multicast.FeatureToggles {
    using ServerData;

    public class SdFeatureToggleRepo : SdRepo<SdFeatureToggleData>, IFeatureToggleRepo {
        private readonly SdValue<bool> isOldUser;

        public SdFeatureToggleRepo(SdArgs args) : base(args, a => new SdFeatureToggleData(a)) {
            this.isOldUser = this.Child(1);
        }

        public bool IsOldUser => this.isOldUser.Value;

        public bool TryGetFeature(string featureKey, out IFeatureToggleData feature) {
            if (this.Lookup.TryGetValue(featureKey, out var data)) {
                feature = data;
                return true;
            }

            feature = null;
            return false;
        }


        public void SetIsOldUser() {
            this.isOldUser.Value = true;
        }

        public SdFeatureToggleData GetOrCreateFeature(string featureKey, out bool created) {
            return this.Lookup.GetOrCreate(featureKey, out created);
        }

        public void RemoveFeature(string featureKey) {
            this.Lookup.Remove(featureKey);
        }
    }

    public class SdFeatureToggleData : SdArrayObject, IFeatureToggleData {
        private readonly SdValue<string> variant;
        private readonly SdValue<int>    timesChanged;

        public SdFeatureToggleData(SdArgs args) : base(args) {
            this.variant      = this.Child(0);
            this.timesChanged = this.Child(1);
        }

        public string Variant => this.variant.Value;

        public int TimesChanged => this.timesChanged.Value;

        public void SetVariant(string value) {
            this.variant.Value = value;
        }

        public void IncrementTimesChanged() {
            this.timesChanged.Value++;
        }
    }
}