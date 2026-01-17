namespace Multicast.Boot.Steps {
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UnityEngine;
    using Utilities;

    [RequireFieldsInit]
    internal struct LoadServicesSceneControllerArgs : IResultControllerArgs {
    }

    internal class LoadServicesSceneController : ResultController<LoadServicesSceneControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<LoadServicesSceneControllerArgs, LoadServicesSceneController>();
        }

        protected override async UniTask Execute(Context context) {
            await AddressablesUtils.LoadSceneAsync(AppConstants.AddressableAssets.SERVICES_SCENE);
        }
    }
}