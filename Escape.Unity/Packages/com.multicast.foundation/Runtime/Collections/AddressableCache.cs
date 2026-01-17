namespace Multicast.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Diagnostics;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using Utilities;
    using Debug = UnityEngine.Debug;
    using Object = UnityEngine.Object;

    public class AddressableCache<T> : IEnumerableCache<T> {
        private const string DEBUG_TIMER_CATEGORY = "addressable_cache";

        private readonly List<string>                          keyCache       = new();
        private readonly Dictionary<string, IResourceLocation> locationsCache = new();
        private readonly Dictionary<string, T>                 assetsCache    = new();

        [PublicAPI]
        public IEnumerable<string> EnumerateCachedPaths() => this.assetsCache.Keys;

        [PublicAPI]
        public async UniTask Preload(string key, bool loadOnDemand = false) {
            using var _ = DebugTimer.Create(DEBUG_TIMER_CATEGORY, key);

            var locations = AddressablesUtils.LoadResourceLocations<T>(key);

            foreach (var loc in locations) {
                this.locationsCache.Add(loc.PrimaryKey, loc);
                this.keyCache.Add(loc.PrimaryKey);
            }

            if (loadOnDemand) {
                if (locations.Count > 0) {
                    this.Preload(locations[0]);
                }
            }
            else {
                foreach (var location in locations) {
                    this.Preload(location);
                }
            }

            await UniTask.NextFrame();
        }

        public IEnumerable<string> EnumeratePaths() {
            return this.keyCache;
        }

        [PublicAPI]
        public T Get(string path) {
            if (this.TryGet(path, out var asset)) {
                return asset;
            }

            throw new InvalidOperationException($"Asset '{path}' not preloaded");
        }

        [PublicAPI]
        public bool TryGet(string path, out T asset) {
            if (this.assetsCache.TryGetValue(path, out asset)) {
                return true;
            }

            if (this.locationsCache.TryGetValue(path, out var loc)) {
                asset = this.Preload(loc);
                return true;
            }

            return false;
        }

        private T Preload(IResourceLocation location) {
            var operation = Addressables.LoadAssetAsync<T>(location);

            var asset = operation.WaitForCompletion();

            if (operation.Status != AsyncOperationStatus.Succeeded) {
                throw new InvalidOperationException("AddressableCache Preload failed");
            }

            this.assetsCache[location.PrimaryKey] = asset;

            return asset;
        }
    }
}