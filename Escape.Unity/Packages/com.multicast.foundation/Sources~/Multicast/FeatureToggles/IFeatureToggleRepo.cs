namespace Multicast.FeatureToggles {
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IFeatureToggleRepo {
        bool IsOldUser { get; }

        bool TryGetFeature(string featureKey, out IFeatureToggleData feature);
    }

    [PublicAPI]
    public interface IFeatureToggleData {
        string Variant { get; }

        int TimesChanged { get; }
    }
}