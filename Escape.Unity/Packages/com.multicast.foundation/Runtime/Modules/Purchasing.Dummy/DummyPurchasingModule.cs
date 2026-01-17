namespace Multicast.Modules.Purchasing.Dummy {
    using Cheats;
    using Cysharp.Threading.Tasks;
    using Install;
    using Multicast.Purchasing;
    using UnityEngine;

    public class DummyPurchasingModule : ScriptableModule {
        [SerializeField] private DummyPurchasing.Options options;

        public override void Setup(ModuleSetup module) {
            module.Provides<IPurchasing>();
        }

        public override async UniTask Install(Resolver resolver) {
            var cheatButtons = await resolver.Get<ICheatButtonsRegistry>();

            var dummyPurchasing = await resolver.Register<IPurchasing>()
                .ToAsync<DummyPurchasing, DummyPurchasing.Options>(this.options);

            dummyPurchasing.Initialize();

            cheatButtons.RegisterAction("Clear Purchases", () => dummyPurchasing.ClearPurchases());
        }
    }
}