namespace Multicast.Modules.Boosts {
    using Cheats;
    using Cysharp.Threading.Tasks;
    using GameProperties;
    using Install;
    using Multicast.Boosts;

    public class BoostsModule : IScriptableModule {
        public string name => "Boosts";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            var gameProperties  = await resolver.Get<GamePropertiesModel>();
            var cheatProperties = await resolver.Get<ICheatGamePropertiesRegistry>();

            cheatProperties.Register(AppGameProperties.Booleans.ShowNothingBoostsInDetails);

            BoostValue.ShowNothingBoostsInDetails = () => gameProperties.Get(AppGameProperties.Booleans.ShowNothingBoostsInDetails);
        }

        public void PostInstall() {
        }
    }
}