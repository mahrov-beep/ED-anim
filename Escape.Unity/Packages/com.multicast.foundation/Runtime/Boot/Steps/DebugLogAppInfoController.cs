namespace Multicast.Boot.Steps {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UnityEngine;
    using TextAsset = UnityEngine.TextAsset;

    [Serializable, RequireFieldsInit]
    internal struct DebugLogAppInfoControllerArgs : IResultControllerArgs {
    }

    internal class DebugLogAppInfoController : ResultController<DebugLogAppInfoControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<DebugLogAppInfoControllerArgs, DebugLogAppInfoController>();
        }

        protected override async UniTask Execute(Context context) {
            if (!Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                return;
            }

            var sb = new StringBuilder()
                .Append("Product = ").AppendLine(Application.productName)
                .Append("Platform = ").AppendLine(App.Platform)
                .Append("Version = ").AppendLine(Application.version)
                .Append("UnityVersion = ").AppendLine(Application.unityVersion)
                .Append("");

            if (Resources.Load("UnityCloudBuildManifest.json") is TextAsset manifestTextAsset &&
                manifestTextAsset != null &&
                Json.Deserialize(manifestTextAsset.text) is Dictionary<string, object> manifestDict) {
                foreach (var (key, value) in manifestDict) {
                    if (key == "buildStartTime" && long.TryParse($"{value}", out var buildStartTimeUnix)) {
                        var buildStartDate = DateTimeOffset.FromUnixTimeSeconds(buildStartTimeUnix).DateTime;
                        sb.Append("buildStartTime = ").Append(buildStartDate.ToShortDateString()).Append(" ").Append(buildStartDate.ToShortTimeString()).AppendLine();
                    }
                    else {
                        sb.Append(key).Append(" = ").Append(value).AppendLine();
                    }
                }
            }

            Debug.Log(sb);
        }
    }
}