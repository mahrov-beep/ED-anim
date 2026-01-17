namespace Multicast.Boot.Steps {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct GetPlatformControllerArgs : IResultControllerArgs<string> {
        public string EditorPlatformOverride;
    }

    public class GetPlatformController : ResultController<GetPlatformControllerArgs, string> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<GetPlatformControllerArgs, GetPlatformController>();
        }

        protected override async UniTask<string> Execute(Context context) {
            var platforms = this.FetchPlatforms().ToList();

            return platforms.Count switch {
                1 => platforms[0],
                0 => throw new Exception($"Boot failed: no platform define found"),
                _ => throw new Exception(GetMultiplePlatformDefinesErrorMessage(platforms)),
            };

            static string GetMultiplePlatformDefinesErrorMessage(IEnumerable<string> platforms) {
                return $"Boot failed: multiple platform defines found: {string.Join(", ", platforms)}" +
                       $"{Environment.NewLine}Remove all platform specific defines from ScriptingDefineSymbols" +
                       $"{Environment.NewLine}If you catch this exception, also check that there are no unnecessary defines like BOOTLOADER_ENABLE_DEBUG_LOGS, USER_DATA_SHOW_SELECTOR_UI and so on" +
                       $"";
            }
        }

        protected IEnumerable<string> FetchPlatforms() {
#if ANDROID_DEV
            yield return GamePlatform.ANDROID_DEV;
#endif
#if ANDROID_PROD
            yield return GamePlatform.ANDROID_PROD;
#endif
#if IOS_DEV
            yield return GamePlatform.IOS_DEV;
#endif
#if IOS_PROD
            yield return GamePlatform.IOS_PROD;
#endif
#if STEAM_DEV
            yield return GamePlatform.STEAM_DEV;
#endif
#if STEAM_PROD
            yield return GamePlatform.STEAM_PROD;
#endif
#if UNITY_EDITOR
            var overridenPlatform = this.Args.EditorPlatformOverride;

            if (string.IsNullOrWhiteSpace(overridenPlatform)) {
                Debug.LogError("Boot: EditorPlatformOverride is empty. Please select valid override in Modules window");
                yield return GamePlatform.EDITOR;
            }
            else {
                yield return overridenPlatform;
            }
#endif

            // Workaround for Addressables which are built without defines
            // ReSharper disable once RedundantJumpStatement
            yield break;
        }
    }
}