namespace Multicast.Build {
    using System;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    internal static class BuildDefinesController {
        private const string DEFINES_PREFS_KEY = "last_defines";

        [DidReloadScripts]
        private static void DidReloadScripts() {
            RevertDefinesIfAvailable();
        }

        public static void CaptureDefines(BuildTargetGroup buildTargetGroup) {
            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            SessionState.SetString(DEFINES_PREFS_KEY, EditorJsonUtility.ToJson(new DefineData {
                buildTargetGroup = buildTargetGroup,
                projectPath      = Application.dataPath,
                defines          = definesString,
            }));
        }

        public static void RevertDefinesIfAvailable() {
            var definesJson = SessionState.GetString(DEFINES_PREFS_KEY, null);

            if (string.IsNullOrEmpty(definesJson)) {
                return;
            }

            SessionState.EraseString(DEFINES_PREFS_KEY);

            var definesData = JsonUtility.FromJson<DefineData>(definesJson);

            if (definesData.projectPath != Application.dataPath) {
                return;
            }

            Debug.Log("Revert defines: " + definesData.defines);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(definesData.buildTargetGroup, definesData.defines);

            AssetDatabase.SaveAssets();
        }

        [Serializable]
        private struct DefineData {
            public BuildTargetGroup buildTargetGroup;
            public string           projectPath;
            public string           defines;
        }
    }
}