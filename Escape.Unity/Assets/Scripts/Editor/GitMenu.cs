namespace Scripts.Editor {
    using System;
    using Multicast.Build.PostBuildSteps;
    using Multicast.Build.PreBuildSteps;
    using UnityEditor;
    using UnityEngine;

    public static class GitMenu {
        [MenuItem("GIT/Check and Fix All Issues")]
        public static void CheckAndFixAllIssues() {
            try {
                AddressablesImporterRun.Execute();

                UnitTestsRun.Execute();
                OdinValidateProject.Execute();

                AddressablesClearCachedHash.Execute();

                EditorUtility.DisplayDialog("Git - Prepare for Merge", "Готово!", "Закрыть");
            }
            catch (Exception ex) {
                Debug.LogException(ex);

                EditorUtility.DisplayDialog("Git - Prepare for Merge", "Не удалось подготовить проект. Подробности в консоли", "Ok");
            }
            finally {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
    }
}