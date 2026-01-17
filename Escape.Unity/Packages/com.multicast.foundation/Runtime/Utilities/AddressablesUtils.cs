namespace Multicast.Utilities {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;

    public static class AddressablesUtils {
        [PublicAPI]
        public static IList<IResourceLocation> LoadResourceLocations<T>(string key) {
            var operation = Addressables.LoadResourceLocationsAsync(key, typeof(T));

            var result = operation.WaitForCompletion();

            if (operation.Status != AsyncOperationStatus.Succeeded) {
                throw new InvalidOperationException("LoadResourceLocationsAsync failed");
            }

            return result;
        }

        [PublicAPI]
        public static async UniTask<SceneInstance> LoadSceneAsync(string key, LoadSceneMode mode = LoadSceneMode.Single) {
            var locations = await Addressables.LoadResourceLocationsAsync(key);

            if (locations.Count != 1) {
                throw new InvalidOperationException($"Failed to load scene '{key}'");
            }

            var operation = Addressables.LoadSceneAsync(locations[0], mode);

            var scene = await operation.Task;

            if (operation.Status != AsyncOperationStatus.Succeeded) {
                throw new InvalidOperationException("LoadSceneAsync failed");
            }

            return scene;
        }

        [PublicAPI]
        public static async UniTask UnloadSceneAsync(SceneInstance sceneInstance) {
            try {
                await Addressables.UnloadSceneAsync(sceneInstance);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
    }
}