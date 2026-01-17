namespace Multicast.Build {
    using UnityEditor;
    using UnityEditor.Build;

    public static class EditorBuildHooks {
        [InitializeOnLoadMethod]
        public static void Setup() {
            BuildPlayerWindow.RegisterBuildPlayerHandler(HandleBuildPlayer);
        }

        private static void HandleBuildPlayer(BuildPlayerOptions options) {
            throw new BuildFailedException("Use one of BuildConfiguration to make builds instead of builtin menu");
        }
    }
}