namespace Multicast.Build.PreBuildSteps {
    using System;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Build.AnalyzeRules;

    [Serializable]
    public class FixDuplicatesInBundles : PreBuildStep {
        public override void PreBuild(BuildContext context) => Execute();
        public override void Cleanup(BuildContext context)  => Cleanup();

        [MenuItem("Build/Build Step/Addressables - FixDuplicatesInBundles - Execute")]
        public static void Execute() {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            var rule    = new CheckBundleDupeDependencies();
            var results = rule.RefreshAnalysis(settings);

            if (results.Count == 1 && results[0].resultName == "No issues found") {
                return;
            }

            rule.FixIssues(settings);

            const string duplicateAssetsGroup = "Duplicate Asset Isolation";

            var group = settings.FindGroup(duplicateAssetsGroup);
            if (group != null) {
                EditorUtility.SetDirty(group);
                AssetDatabase.SaveAssetIfDirty(group);
            }
        }

        [MenuItem("Build/Build Step/Addressables - FixDuplicatesInBundles - Cleanup")]
        public static void Cleanup() {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            const string duplicateAssetsGroup = "Duplicate Asset Isolation";

            var group = settings.FindGroup(duplicateAssetsGroup);
            if (group != null) {
                settings.RemoveGroup(group);
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }
    }
}