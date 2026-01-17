namespace Multicast.Localization {
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    internal static class EditorLocalizationSetup {
        [InitializeOnLoadMethod]
        public static void Setup() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                return;
            }

            var cache = EditorAddressablesCache<LocalizationTable>.Instance;
            var paths = EditorAddressablesUtils
                .EnumeratePathsByLabel(AppConstants.AddressableLabels.LOCALIZATION)
                .ToArray();

            LocalizationService.Configure(cache, paths);
        }
    }
}