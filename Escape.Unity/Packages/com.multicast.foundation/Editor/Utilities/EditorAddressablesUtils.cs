namespace Multicast.Utilities {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEditor.AddressableAssets;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class EditorAddressablesUtils {
        [PublicAPI]
        public static IEnumerable<string> EnumeratePaths() {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null) {
                Debug.LogError("Addressables not configured");
                yield break;
            }

            foreach (var g in settings.groups) {
                foreach (var entry in g.entries) {
                    yield return entry.address;
                }
            }
        }

        [PublicAPI]
        public static IEnumerable<string> EnumeratePathsByLabel(string label) {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null) {
                Debug.LogError("Addressables not configured");
                yield break;
            }

            foreach (var g in settings.groups) {
                foreach (var entry in g.entries) {
                    if (entry.labels.Contains(label)) {
                        yield return entry.address;
                    }
                }
            }
        }

        [PublicAPI]
        public static Object LoadAddressable(string path) {
            return LoadAddressable<Object>(path);
        }

        public static T LoadAddressable<T>(string path) where T : Object {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null) {
                throw new InvalidOperationException("Addressables not configured");
            }

            var entry = settings.groups
                .SelectMany(g => g.entries)
                .FirstOrDefault(e => e.address == path && typeof(T).IsAssignableFrom(e.MainAssetType));

            return entry?.MainAsset as T;
        }
    }
}