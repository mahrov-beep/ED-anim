namespace Multicast.DirtyDataEditor {
    using System.IO;
    using System.Linq;
    using UnityEditor;

    public static class DirtyDataMenu {
        [MenuItem("DDE/Import from GoogleSheet", true, priority = 10)]
        public static bool CanImportFromGoogleSheet() {
            return DirtyDataEditorSettings.Instance != null &&
                   DirtyDataEditorSettings.Instance.enabled;
        }

        [MenuItem("DDE/Import from GoogleSheet", priority = 10)]
        public static void ImportFromGoogleSheet() {
            DirtyDataEditorSettings.Instance.ImportConfigsFromGoogleSheets();
        }

        [MenuItem("DDE/Revert all to Default", true, priority = 150)]
        public static bool CanRevertToDefault() {
            return DirtyDataEditorSettings.Instance != null &&
                   DirtyDataEditorSettings.Instance.enabled &&
                   DirtyDataEditorSettings.Instance.CanRevertToDefault();
        }

        [MenuItem("DDE/Revert all to Default", priority = 150)]
        public static void RevertToDefault() {
            DirtyDataEditorSettings.Instance.RevertToDefault();
        }

        [MenuItem("DDE/Select Settings", priority = 140)]
        public static void SelectSettingsAsset() {
            Selection.activeObject = DirtyDataEditorSettings.Instance;
            EditorGUIUtility.PingObject(DirtyDataEditorSettings.Instance);
        }
    }
}