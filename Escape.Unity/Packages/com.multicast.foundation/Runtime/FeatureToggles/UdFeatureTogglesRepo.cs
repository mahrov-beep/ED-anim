namespace Multicast.FeatureToggles {
    using UserData;

    public class UdFeatureTogglesRepo : UdRepo<UdFeatureToggleData>, IFeatureToggleRepo {
        private readonly UdValue<bool> isOldUser;

        public UdFeatureTogglesRepo(UdArgs args) : base(args, a => new UdFeatureToggleData(a)) {
            this.isOldUser = this.Child("is_old_user");
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

        public UdFeatureToggleData GetOrCreateFeature(string featureKey, out bool created) {
            return this.Lookup.GetOrCreate(featureKey, out created);
        }

        public void RemoveFeature(string featureKey) {
            this.Lookup.Remove(featureKey);
        }
    }

    public class UdFeatureToggleData : UdObject, IFeatureToggleData {
        private readonly UdValue<string> variant;
        private readonly UdValue<int>    timesChanged;

        public UdFeatureToggleData(UdArgs args) : base(args) {
            this.variant      = this.Child("variant");
            this.timesChanged = this.Child("times_changed");
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