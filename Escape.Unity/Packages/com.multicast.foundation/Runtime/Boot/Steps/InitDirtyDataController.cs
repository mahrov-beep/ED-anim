namespace Multicast.Boot.Steps {
    using System;
    using Cysharp.Threading.Tasks;
    using DirtyDataEditor;
    using Multicast;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct InitDirtyDataControllerArgs : IResultControllerArgs {
    }

    public class InitDirtyDataController : ResultController<InitDirtyDataControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<InitDirtyDataControllerArgs, InitDirtyDataController>();
        }

        protected override async UniTask Execute(Context context) {
            DirtyDataTypeModel.Initialize();
            DirtyDataParsers.Initialize();
        }
    }
}