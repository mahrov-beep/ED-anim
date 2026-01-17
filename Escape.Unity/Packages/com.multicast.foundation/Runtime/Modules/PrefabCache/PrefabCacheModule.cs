namespace Multicast.Modules.PrefabCache {
    using Collections;
    using Cysharp.Threading.Tasks;
    using Install;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using PrefabCache = Multicast.PrefabCache;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class PrefabCacheModule : ScriptableModule, IScriptableModuleWithPriority {
        [SerializeField, Required] private string prefabsAddressableGroup = "Prefabs";
        [SerializeField]           private bool   loadOnDemand            = true;

        private AddressableCache<GameObject> cache;
        private UniTask                      cachePreloadTask;

        public int Priority => ScriptableModulePriority.LATE;

        public override void Setup(ModuleSetup module) {
            module.Provides<PrefabCache>();
        }

        public override void PreInstall() {
            base.PreInstall();

            this.cache            = new AddressableCache<GameObject>();
            this.cachePreloadTask = this.cache.Preload(this.prefabsAddressableGroup, this.loadOnDemand);
        }

        public override async UniTask Install(Resolver resolver) {
            await this.cachePreloadTask;

            resolver.Register<PrefabCache>().To(new PrefabCache(this.cache));
        }
    }
}