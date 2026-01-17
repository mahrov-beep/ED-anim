namespace Multicast.GameProperties {
    using Cysharp.Threading.Tasks;
    using Install;

    internal class GamePropertiesModule : IScriptableModule {
        public string name => "Game Properties";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
            module.Provides<GamePropertiesModel>();
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            await resolver.Register<GamePropertiesModel>().ToAsync<GamePropertiesModel>();
        }

        public void PreInstall() {
        }

        public void PostInstall() {
        }
    }
}