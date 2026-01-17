namespace Multicast.Boot.Steps {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct SetupStackTraceControllerArgs : IResultControllerArgs {
    }

    public class SetupStackTraceController : ResultController<SetupStackTraceControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<SetupStackTraceControllerArgs, SetupStackTraceController>();
        }

        protected override async UniTask Execute(Context context) {
            if (!this.IsDebugLogsEnabled()) {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                Debug.unityLogger.filterLogType = LogType.Error;
            }
        }

        private bool IsDebugLogsEnabled() {
#if UNITY_EDITOR || BOOTLOADER_ENABLE_DEBUG_LOGS
            return true;
#else
            return false;
#endif
        }
    }
}