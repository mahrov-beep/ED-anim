namespace Multicast.Build.PreBuildSteps {
    using System;
    using UnityEditor;
    using UnityEditor.AddressableAssets.Settings;

    [Serializable]
    public class AddressablesBuildPlayerContent : PreBuildStep {
        public override void PreBuild(BuildContext context) => Execute();

        [MenuItem("Build/Build Step/Addressables Build")]
        public static void Execute() {
            AddressableAssetSettings.BuildPlayerContent();
        }
    }
}