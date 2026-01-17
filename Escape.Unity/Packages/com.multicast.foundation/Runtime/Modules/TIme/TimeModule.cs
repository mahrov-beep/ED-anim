namespace Multicast.Modules.TIme {
    using Cheats;
    using Cysharp.Threading.Tasks;
    using Install;

    public class TimeModule : IScriptableModule {
        public string name => "Time";

        public bool IsPlatformSupported(string platform) => true;

        public void Setup(ScriptableModule.ModuleSetup module) {
            module.Provides<ITimeService>();
        }

        public void PreInstall() {
        }

        public async UniTask Install(ScriptableModule.Resolver resolver) {
            var cheatProperties = await resolver.Get<ICheatGamePropertiesRegistry>();

            await resolver.Register<ITimeService>().ToAsync<TimeService>();

            cheatProperties.Register(TimeGameProperties.DaysOffset);
            cheatProperties.Register(TimeGameProperties.HoursOffset);
        }

        public void PostInstall() {
        }
    }
}