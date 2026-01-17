namespace Multicast.FeatureToggles {
    public interface IFeatureToggleVariantProvider {
        bool TryGetVariantsString(string feature, out string variantsString);
    }
}