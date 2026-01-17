namespace Multicast.Build.PreBuildSteps {
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Settings;

    [Serializable]
    public class MoveTmpSettingsToAddressables : PreBuildStep {
        public override void PreBuild(BuildContext context) => Execute();
        public override void Cleanup(BuildContext context)  => Cleanup();

        public const string TMP_SETTINGS_PATH_IN_RESOURCES    = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        public const string TMP_SETTINGS_PATH_IN_ADDRESSABLES = "Assets/TextMesh Pro/Addressables/TMP Settings.asset";

        [MenuItem("Build/Build Step/MoveTmpSettingsToAddressables - Execute")]
        public static void Execute() {
            if (!File.Exists(TMP_SETTINGS_PATH_IN_RESOURCES)) {
                return;
            }

            if (File.Exists(TMP_SETTINGS_PATH_IN_ADDRESSABLES)) {
                File.Delete(TMP_SETTINGS_PATH_IN_ADDRESSABLES);
            }

            var addressablesFolder = Path.GetDirectoryName(TMP_SETTINGS_PATH_IN_ADDRESSABLES);

            if (addressablesFolder != null && !Directory.Exists(addressablesFolder)) {
                Directory.CreateDirectory(addressablesFolder);
            }

            MoveUnityFile(TMP_SETTINGS_PATH_IN_RESOURCES, TMP_SETTINGS_PATH_IN_ADDRESSABLES);

            var guid     = AssetDatabase.AssetPathToGUID(TMP_SETTINGS_PATH_IN_ADDRESSABLES);
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry    = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);

            entry.address = "TMP_Settings";

            SaveAddressables();
        }

        [MenuItem("Build/Build Step/MoveTmpSettingsToAddressables - Cleanup")]
        public static void Cleanup() {
            if (!File.Exists(TMP_SETTINGS_PATH_IN_ADDRESSABLES)) {
                return;
            }

            if (File.Exists(TMP_SETTINGS_PATH_IN_RESOURCES)) {
                File.Delete(TMP_SETTINGS_PATH_IN_RESOURCES);
            }

            var resourcesFolder    = Path.GetDirectoryName(TMP_SETTINGS_PATH_IN_RESOURCES);
            var addressablesFolder = Path.GetDirectoryName(TMP_SETTINGS_PATH_IN_ADDRESSABLES);

            if (resourcesFolder != null && !Directory.Exists(resourcesFolder)) {
                Directory.CreateDirectory(resourcesFolder);
            }

            var guid     = AssetDatabase.AssetPathToGUID(TMP_SETTINGS_PATH_IN_ADDRESSABLES);
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            settings.RemoveAssetEntry(guid);

            SaveAddressables();

            MoveUnityFile(TMP_SETTINGS_PATH_IN_ADDRESSABLES, TMP_SETTINGS_PATH_IN_RESOURCES);

            if (addressablesFolder != null &&
                Directory.EnumerateFileSystemEntries(addressablesFolder).Any() is var addressablesFolderNotEmpty &&
                !addressablesFolderNotEmpty) {
                AssetDatabase.DeleteAsset(addressablesFolder);
            }
        }

        private static void MoveUnityFile(string src, string dst) {
            File.Move(src, dst);
            File.Move(src + ".meta", dst + ".meta");

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(dst, ImportAssetOptions.ForceUpdate);
        }

        private static void SaveAddressables() {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }
    }
}