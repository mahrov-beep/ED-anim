namespace Multicast.Boot.Steps {
    using System;
    using System.Linq;
    using Collections;
    using Cysharp.Threading.Tasks;
    using Localization;
    using Multicast;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    internal struct ConfigureLocalizationControllerArgs : IResultControllerArgs {
    }

    internal class ConfigureLocalizationController : ResultController<ConfigureLocalizationControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<ConfigureLocalizationControllerArgs, ConfigureLocalizationController>();
        }

        protected override async UniTask Execute(Context context) {
            var localizationCache = new AddressableCache<LocalizationTable>();
            await localizationCache.Preload(AppConstants.AddressableLabels.LOCALIZATION);
            var paths = localizationCache.EnumerateCachedPaths().ToArray();
            LocalizationService.Configure(localizationCache, paths);
        }
    }
}