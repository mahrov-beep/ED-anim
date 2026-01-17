namespace Multicast.Build {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class BuildScript {
        public static BuildConfiguration LoadConfigurationAtPath(string path) {
            return AssetDatabase.LoadAssetAtPath<BuildConfiguration>(path);
        }

        public static void BuildDefaultPlayer(BuildConfiguration configuration, BuildOptions buildOptions) {
            var options     = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions());
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var context     = new BuildContext(buildTarget, buildCode: configuration.LocalBuildCode);

            options.options = buildOptions;

            BuildDefaultPlayer(configuration, context, options);
        }

        public static void BuildDefaultPlayer(BuildConfiguration configuration, BuildContext context, BuildPlayerOptions options) {
            Debug.Log($"[BuildScript] BuildDefaultPlayer: configuration = {configuration.name}");

            BuildDefinesController.CaptureDefines(context.BuildTargetGroup);

            BuildScript.ExecutePreBuildSteps(configuration, context);

            try {
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                context.BuildFailed = true;
            }

            ExecutePostBuildSteps(configuration, context);

            BuildDefinesController.RevertDefinesIfAvailable();

            AssetDatabase.SaveAssets();
        }

        public static bool PopulateDefines(BuildConfiguration configuration, HashSet<string> list) {
            Debug.Log($"[BuildScript] PopulateDefines: configuration = {configuration.name}");

            var changed = false;

            foreach (var item in configuration.EnumerateDefineSymbols()) {
                if (item.enabled) {
                    changed |= list.Add(item.symbol);
                }
                else {
                    changed |= list.Remove(item.symbol);
                }
            }

            return changed;
        }

        public static void ExecutePreBuildSteps(BuildConfiguration configuration, BuildContext context) {
            Debug.Log($"[BuildScript] ExecutePreBuildSteps: configuration = {configuration.name}");

            var initialDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(context.BuildTargetGroup);

            foreach (var step in configuration.EnumeratePreBuildSteps()) {
                if (!step.IsBuildStepEnabled) {
                    Debug.Log($"[BuildScript] Skip pre build step: {step.Name}");
                    continue;
                }

                Debug.Log($"[BuildScript] Begin pre build step: {step.Name}");

                step.PreBuild(context);

                Debug.Log($"[BuildScript] Finish pre build step: {step.Name}");
            }

            var defines        = initialDefinesString.Split(';').ToHashSet();
            var definesChanged = BuildScript.PopulateDefines(configuration, defines);

            if (definesChanged) {
                var newDefinesString = defines.Aggregate((a, b) => a + ";" + b);
                Debug.Log($"[BuildScript] Set defines: {initialDefinesString}");

                PlayerSettings.SetScriptingDefineSymbolsForGroup(context.BuildTargetGroup, newDefinesString);
            }
        }

        public static void ExecutePostBuildSteps(BuildConfiguration configuration, BuildContext context) {
            Debug.Log($"[BuildScript] ExecutePostBuildSteps: configuration = {configuration.name}");

            foreach (var step in configuration.EnumeratePreBuildSteps()) {
                if (!step.IsBuildStepEnabled) {
                    continue;
                }

                step.Cleanup(context);
            }

            foreach (var step in configuration.EnumeratePostBuildSteps()) {
                if (!step.IsBuildStepEnabled) {
                    Debug.Log($"[BuildScript] Skip post build step: {step.Name}");
                    continue;
                }

                Debug.Log($"[BuildScript] Begin post build step: {step.Name}");

                step.PostBuild(context);

                Debug.Log($"[BuildScript] Finish post build step: {step.Name}");
            }
        }
    }
}