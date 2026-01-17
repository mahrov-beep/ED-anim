namespace Multicast.Modules.Purchasing.UnityIAP {
    using Cysharp.Threading.Tasks;
    using Install;
    using Multicast.Purchasing;
    using UnityEngine;

    public class UnityIapPurchasingModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
#if UNITY_PURCHASING
            module.Provides<IPurchasing>();
            module.Provides<IUnityIapValidationsRegistration>();
#endif
        }

        public override async UniTask Install(Resolver resolver) {
#if UNITY_PURCHASING
            var purchasing = await resolver.Register<IPurchasing>().ToAsync<UnityIapPurchasing>();
            resolver.Register<IUnityIapValidationsRegistration>().To(purchasing);

            purchasing.Initialize();
#else
            Debug.LogError($"Project does not contains UNITY_PURCHASING define. Add it or remove {this.name}");
#endif
        }
    }
}