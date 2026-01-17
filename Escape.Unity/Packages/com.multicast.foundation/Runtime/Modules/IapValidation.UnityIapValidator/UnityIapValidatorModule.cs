namespace Multicast.Modules.IapValidation.UnityIapValidator {
    using Cysharp.Threading.Tasks;
    using Install;
    using Purchasing.UnityIAP;
    using UnityEngine;

    public class UnityIapValidatorModule : ScriptableModule {
        public override void Setup(ModuleSetup module) {
        }

        public override async UniTask Install(Resolver resolver) {
#if UNITY_PURCHASING
            var registration = await resolver.Get<IUnityIapValidationsRegistration>();
            var tangle       = await resolver.Get<UnityIapValidatorTangle>();

            registration.RegisterValidator(new UnityIapValidator(tangle));
#else
            Debug.LogError($"Project does not contains UNITY_PURCHASING define. Add it or remove {this.name}");
#endif
        }
    }
}