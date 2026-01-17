namespace Multicast.Modules.Firebase {
    using Cysharp.Threading.Tasks;
    using Install;
    using UnityEngine;

    public class FirebaseModule : ScriptableModule, IScriptableModuleWithPriority {
        public int Priority { get; } = ScriptableModulePriority.EARLY;

        public override void Setup(ModuleSetup module) {
#if FIREBASE_SDK
            module.Provides<SdkInitializationMarkers.Firebase>();
#endif
        }

        public override async UniTask Install(Resolver resolver) {
#if FIREBASE_SDK
            var dependencyStatus = await global::Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus != global::Firebase.DependencyStatus.Available) {
                Debug.LogErrorFormat("Could not resolve all Firebase dependencies: {0}", dependencyStatus);
                return;
            }

            var _ = global::Firebase.FirebaseApp.DefaultInstance;

            resolver.Register<SdkInitializationMarkers.Firebase>().To(new SdkInitializationMarkers.Firebase());
#else
            Debug.LogError($"Project does not contains FIREBASE_SDK define. Add it or remove {this.name}");
#endif
        }
    }
}