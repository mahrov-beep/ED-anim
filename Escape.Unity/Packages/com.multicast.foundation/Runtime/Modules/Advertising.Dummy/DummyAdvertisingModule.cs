namespace Multicast.Modules.Advertising.Dummy {
    using Cheats;
    using Cysharp.Threading.Tasks;
    using FeatureToggles;
    using Install;
    using Multicast.Advertising;
    using Scellecs.Morpeh;
    using Sirenix.OdinInspector;
    using UserTracking;

    public class DummyAdvertisingModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
            module.Provides<IAdvertising>();
        }

        public override async UniTask Install(Resolver resolver) {
            var trackingService = await resolver.Get<IUserTrackingService>();
            var cheatProperties = await resolver.Get<ICheatGamePropertiesRegistry>();
            var features        = await resolver.Get<FeatureTogglesModel>();

            cheatProperties.Register(AdGameProperties.AdNoRewarded);

            var advertising = await resolver.Register<IAdvertising>().ToAsync<DummyAdvertising>();

            advertising.Initialize();
        }
    }
}