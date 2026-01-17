namespace Multicast.Build {
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    public static class UnityCloudBuildHooks {
        private const string ENV_VAR_BUILD_CONFIG = "SBP_BUILD_CONFIG";

        private static BuildConfiguration configuration;
        private static BuildContext       buildContext;

        [UsedImplicitly]
        public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest) {
            var target = EditorUserBuildSettings.activeBuildTarget;

            var buildCode            = manifest.GetValue<int>("buildNumber");
            var bundleId             = manifest.GetValue<string>("bundleId");
            var gitCommitId          = manifest.GetValue<string>("scmCommitId");
            var gitBranch            = manifest.GetValue<string>("scmBranch");
            var cloudBuildTargetName = manifest.GetValue<string>("cloudBuildTargetName");

            var buildConfigPath = Environment.GetEnvironmentVariable(ENV_VAR_BUILD_CONFIG);

            configuration = BuildScript.LoadConfigurationAtPath(buildConfigPath);

            if (configuration == null) {
                throw new BuildFailedException("[UnityCloudBuildHooks] Failed to find build config with name");
            }

            buildContext = new BuildContext(target, buildCode);

            BuildScript.ExecutePreBuildSteps(configuration, buildContext);
        }

        [UsedImplicitly]
        public static void PostExport() {
            if (configuration == null) {
                Debug.LogError("[UnityCloudBuildHooks] PostExport configuration is null");
                return;
            }

            BuildScript.ExecutePostBuildSteps(configuration, buildContext);
        }
    }
}

#if !UNITY_CLOUD_BUILD
namespace UnityEngine.CloudBuild {
    public class BuildManifestObject {
        public T GetValue<T>(string key) => default;
    }
}
#endif