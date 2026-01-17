namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEngine;
#if UNITY_EDITOR
    using Sirenix.Utilities.Editor;
    using UnityEditor;

#endif

    [CreateAssetMenu(menuName = "DDE/Settings")]
    public class DirtyDataEditorSettings : ScriptableObject {
        private static DirtyDataEditorSettings settingCached;

        [TitleGroup("Settings"), LabelText("DDE enabled for current project"), ToggleLeft]
        public bool enabled = true;

        [TitleGroup("Settings")]
        [SerializeField, ReadOnly, EnableGUI]
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
        public SheetId[] localizationGoogleSheetIds;

        [TitleGroup("Settings")]
        [SerializeField, ReadOnly, EnableGUI]
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, OnTitleBarGUI = nameof(OnDefaultGoogleSheetsTitleBarGUI))]
        private SheetId[] defaultGoogleSheetIds;

        [Title("", HorizontalLine = true)]
        [TableList(AlwaysExpanded = true)]
        public SheetData[] sheets = Array.Empty<SheetData>();

        [Serializable]
        public class SheetId {
            [HorizontalGroup, HideLabel, DisplayAsString, Required]
            public string description;

            [HorizontalGroup, HideLabel, Required]
            public string googleSheetId;
        }

        [Serializable]
        public class SheetData {
            [Required]
            public string name;

            [Required, LabelText("Google Sheet IDs")]
            [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
            public string[] googleSheetIds;
        }

        [Title("Folders")]
        [SerializeField, FolderPath] private string outputConfigsPath = default;
        [SerializeField, FolderPath] private string outputLocalizationPath = default;
        [SerializeField, FolderPath] private string credentialsPath        = default;
        [SerializeField, FolderPath] private string workPath               = default;

        public string CredentialsPath        => FixPath(this.credentialsPath);
        public string WorkPath               => FixPath(this.workPath);
        public string OutputConfigsPath      => FixPath(this.outputConfigsPath);
        public string OutputLocalizationPath => FixPath(this.outputLocalizationPath);

        private static string FixPath(string path) {
            return path.EndsWith("/")
                ? path.Substring(0, path.Length - 1)
                : path;
        }

        internal static Action<DirtyDataEditorSettings> ImportConfigsCallback;
        internal static Action<DirtyDataEditorSettings> ImportLocalizationCallback;

        [TitleGroup("Google Sheets")]
        [PropertyOrder(-110)]
        [Button(ButtonSizes.Large, Name = "Import Configs")]
        public void ImportConfigsFromGoogleSheets() {
            ImportConfigsCallback?.Invoke(this);
        }

        [TitleGroup("Google Sheets")]
        [PropertyOrder(-100)]
        [Button(ButtonSizes.Large, Name = "Import Localization")]
        public void ImportLocalizationFromGoogleSheets() {
            ImportLocalizationCallback?.Invoke(this);
        }

        public static DirtyDataEditorSettings Instance {
            get {
                if (settingCached != null) {
                    return settingCached;
                }

#if UNITY_EDITOR
                settingCached = AssetDatabase.FindAssets("t: " + typeof(DirtyDataEditorSettings).FullName)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<DirtyDataEditorSettings>)
                    .Single();
#endif
                return settingCached;
            }
        }

        public bool CanRevertToDefault() {
            foreach (var sheetData in this.sheets) {
                if (sheetData.name == "default" && !sheetData.googleSheetIds.SequenceEqual(this.defaultGoogleSheetIds.Select(it => it.googleSheetId))) {
                    return true;
                }
            }

            return false;
        }

        public void RevertToDefault() {
            foreach (var sheetData in this.sheets) {
                if (sheetData.name == "default") {
                    sheetData.googleSheetIds = this.defaultGoogleSheetIds.Select(it => it.googleSheetId).ToArray();
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        private void OnDefaultGoogleSheetsTitleBarGUI() {
#if UNITY_EDITOR
            GUIHelper.PushColor(Color.yellow);

            if (this.CanRevertToDefault() && SirenixEditorGUI.ToolbarButton("Revert to Default")) {
                this.RevertToDefault();
            }

            GUIHelper.PopColor();
#endif
        }
    }
}