namespace Multicast.Modules.LunarConsole {
    using Cheats;
    using Cysharp.Threading.Tasks;
    using Install;

    internal class LunarConsoleModule : IScriptableModule {
        public string name => "Lunar Console";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
            module.Provides<ICheatButtonsRegistry>();
            module.Provides<ICheatGamePropertiesRegistry>();
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            await resolver.Register<ICheatButtonsRegistry>().ToAsync<LunarCheatButtonsRegistry>();
            await resolver.Register<ICheatGamePropertiesRegistry>().ToAsync<LunarCheatGamePropertiesRegistry>();
        }

        public void PostInstall() {
        }
    }
}