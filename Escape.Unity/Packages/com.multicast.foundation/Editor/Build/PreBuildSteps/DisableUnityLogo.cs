namespace Multicast.Build.PreBuildSteps {
    using System;
    using UnityEditor;

    [Serializable]
    public class DisableUnityLogo : PreBuildStep {
        public override void PreBuild(BuildContext context) => Execute();

        [MenuItem("Build/Build Step/Disable Unity Logo")]
        public static void Execute() {
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }
    }
}