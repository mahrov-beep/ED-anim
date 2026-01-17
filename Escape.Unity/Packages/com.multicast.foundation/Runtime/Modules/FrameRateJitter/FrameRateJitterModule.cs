namespace Multicast.Modules {
    using Cheats;
    using Cysharp.Threading.Tasks;
    using GameProperties;
    using Install;
    using Morpeh;
    using Scellecs.Morpeh;
    using UnityEngine;

    public class FrameRateJitterModule : IScriptableModule {
        public string name => "FrameRate Jitter";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            var worldRegistration = await resolver.Get<IWorldRegistration>();
            var cheatProperties   = await resolver.Get<ICheatGamePropertiesRegistry>();

            cheatProperties.Register(FrameRateJitterGameProperties.FpsJitterEnabled);
            worldRegistration.RegisterInstaller(systems => systems.AddExistingSystem<FrameRateJitterSystem>());
        }

        public void PostInstall() {
        }
    }

    internal class FrameRateJitterSystem : SystemBase {
        private readonly GamePropertiesModel properties;

        public FrameRateJitterSystem(GamePropertiesModel properties) {
            this.properties = properties;
        }

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            var enabled = this.properties.Get(FrameRateJitterGameProperties.FpsJitterEnabled);

            if (!enabled) {
                return;
            }

            Application.targetFrameRate = Random.Range(20, 60);
        }
    }
}