namespace Multicast.Boot.Steps {
    using System;
    using Collections;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UniMob.UI;
    using UniMob.UI.Internal.ViewLoaders;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct UniMobUiAddressablesPreloadControllerArgs : IFlowControllerArgs {
        public string AddressablesLabel;
        public bool   LoadOnDemand;
    }

    public class UniMobUiAddressablesPreloadController : FlowController<UniMobUiAddressablesPreloadControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<UniMobUiAddressablesPreloadControllerArgs, UniMobUiAddressablesPreloadController>();
        }

        protected override async UniTask Activate(Context context) {
            var uiCache = new AddressableCache<GameObject>();
            await uiCache.Preload(this.Args.AddressablesLabel, loadOnDemand: this.Args.LoadOnDemand);

            var uiPreloadHandle = new UniMobAddressableLoaderAdapter(uiCache);
            AddressableViewLoaderInternal.RegisterAddressablesLoader(this.Lifetime, uiPreloadHandle);
        }

        private class UniMobAddressableLoaderAdapter : IUniMobAddressablesLoader {
            private readonly AddressableCache<GameObject> cache;

            public UniMobAddressableLoaderAdapter(AddressableCache<GameObject> cache) => this.cache = cache;

            public bool TryGetPrefab(string path, out GameObject prefab) => this.cache.TryGet(path, out prefab);
        }
    }
}