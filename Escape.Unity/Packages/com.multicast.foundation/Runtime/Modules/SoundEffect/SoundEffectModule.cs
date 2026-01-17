namespace Multicast.Modules.SoundEffect {
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Collections;
    using global::Sound;
    using Install;
    using Sound;
    using SoundEffects;
    using UnityEngine;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class SoundEffectModule : ScriptableModule, IScriptableModuleWithPriority {
        [SerializeField] private string soundAddressableGroup;

        private AddressableCache<SoundAsset> cache;
        private UniTask                      cachePreloadTask;

        public int Priority { get; } = ScriptableModulePriority.LATE;

        public override void Setup(ModuleSetup module) {
            module.Provides<ISoundEffectService>();
        }

        public override void PreInstall() {
            base.PreInstall();

            this.cache            = new AddressableCache<SoundAsset>();
            this.cachePreloadTask = this.cache.Preload(this.soundAddressableGroup, loadOnDemand: true);
        }

        public override async UniTask Install(Resolver resolver) {
            await this.cachePreloadTask;

            var soundService = await resolver.Register<ISoundEffectService>().ToAsync<SoundEffectService, ICache<SoundAsset>>(this.cache);

            soundService.Initialize();
        }
    }
}