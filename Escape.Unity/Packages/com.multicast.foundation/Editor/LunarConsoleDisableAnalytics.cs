namespace Multicast {
    using UnityEditor;

    public static class LunarConsoleDisableAnalytics {
        private static readonly BuildTargetGroup[] TargetGroups = {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
            BuildTargetGroup.Standalone,
        };

        private static bool Checked {
            get => SessionState.GetBool("LunarConsoleDisableAnalytics", false);
            set => SessionState.SetBool("LunarConsoleDisableAnalytics", value);
        }

        [InitializeOnLoadMethod]
        private static void AutoDisable() {
            if (Checked) {
                return;
            }

            Checked = true;

            if (!AssetDatabase.IsValidFolder("Assets/LunarConsole")) {
                return;
            }

            foreach (var targetGroup in TargetGroups) {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
                if (!defines.Contains("LUNAR_CONSOLE_ANALYTICS_DISABLED")) {
                    defines += ";LUNAR_CONSOLE_ANALYTICS_DISABLED";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}