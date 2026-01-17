namespace Multicast.Build.PreBuildSteps {
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class ReimportTmpSpriteAssets : PreBuildStep {
        public override void PreBuild(BuildContext context) => Execute();

        [MenuItem("Build/Build Step/Reimport TmpSpriteAssets")]
        public static void Execute() {
            ReimportTextMeshProSpriteAssets();
            ReimportTextMeshProSpriteAtlasSupportPluginAssets();
        }

        public static void ReimportTextMeshProSpriteAssets() {
            foreach (var guid in AssetDatabase.FindAssets("t:TMP_SpriteAsset")) {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                Debug.Log($"[ReimportTextMeshProSpriteAssets] Import TMP sprite asset: {path}");
            }
        }

        public static void ReimportTextMeshProSpriteAtlasSupportPluginAssets() {
            var paths = Directory.EnumerateFiles("Assets/", "*.tmpspriteatlas", SearchOption.AllDirectories);

            foreach (var tmpAtlasPath in paths) {
                var text          = File.ReadAllText(tmpAtlasPath);
                var atlasGuid     = JsonUtility.FromJson<TextMeshProSupportAssetData>(text).atlasGuid;
                var iconAtlasPath = AssetDatabase.GUIDToAssetPath(atlasGuid);

                AssetDatabase.ImportAsset(iconAtlasPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                Debug.Log($"[ReimportTextMeshProSpriteAtlasSupportPluginAssets] Import unity sprite atlas asset: {iconAtlasPath}");

                AssetDatabase.ImportAsset(tmpAtlasPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                Debug.Log($"[ReimportTextMeshProSpriteAtlasSupportPluginAssets] Import TMP sprite atlas asset: {tmpAtlasPath}");
            }
        }

        [Serializable]
        private class TextMeshProSupportAssetData {
            public string atlasGuid;
        }
    }
}