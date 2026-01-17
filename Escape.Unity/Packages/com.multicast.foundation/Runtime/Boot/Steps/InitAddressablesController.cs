namespace Multicast.Boot.Steps {
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    [RequireFieldsInit]
    internal struct InitAddressablesControllerArgs : IResultControllerArgs {
    }

    internal class InitAddressablesController : ResultController<InitAddressablesControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<InitAddressablesControllerArgs, InitAddressablesController>();
        }

        protected override async UniTask Execute(Context context) {
            await Addressables.InitializeAsync();
        }
    }
}