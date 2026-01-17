namespace Multicast.Build.PostBuildSteps {
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEngine;

    [Serializable]
    public class AddressablesClearCachedHash : PostBuildStep {
        public override void PostBuild(BuildContext context) => Execute();

        [MenuItem("Build/Build Step/Addressables Clear CachedHash")]
        public static void Execute() {
            var settings  = AddressableAssetSettingsDefaultObject.Settings;
            var hashField = GetField(settings, "m_CachedHash") ?? GetField(settings, "m_currentHash");

            hashField?.SetValue(settings, default(Hash128));

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }

        private static FieldInfo GetField(object obj, string fieldName) {
            return obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}