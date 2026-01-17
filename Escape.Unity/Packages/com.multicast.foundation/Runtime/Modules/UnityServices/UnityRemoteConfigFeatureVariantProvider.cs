#if UNITY_REMOTE_CONFIG
namespace Multicast.Modules.UnityServices {
    using FeatureToggles;
    using global::Unity.Services.RemoteConfig;

    internal class UnityRemoteConfigFeatureVariantProvider : IFeatureVariantProvider {
        public bool TryGetVariantsString(string feature, out string variantsString) {
            if (!RemoteConfigService.Instance.appConfig.HasKey(feature)) {
                variantsString = default;
                return false;
            }

            variantsString = RemoteConfigService.Instance.appConfig.GetString(feature);
            return true;
        }
    }
}
#endif