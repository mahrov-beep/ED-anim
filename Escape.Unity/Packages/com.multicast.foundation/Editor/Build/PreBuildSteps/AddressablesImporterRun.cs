namespace Multicast.Build.PreBuildSteps {
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class AddressablesImporterRun : PreBuildStep {
        public override void PreBuild(BuildContext context) => Execute();

        [MenuItem("Build/Build Step/Addressables Importer - Execute")]
        public static void Execute() {
            var directories = new[] {"Assets/Content.Addressables"}
                .Where(path => AssetDatabase.IsValidFolder(path))
                .ToArray();

#if ADDRESSABLES_IMPORTER
            AddressableImporter.FolderImporter.ReimportFolders(directories, showConfirmDialog: false);
#else
            Debug.LogError("Addressables Importer plugin not exist in project");
#endif
        }
    }
}