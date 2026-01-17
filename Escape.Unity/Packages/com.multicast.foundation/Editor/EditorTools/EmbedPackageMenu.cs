namespace Multicast.EditorTools {
    using System.IO;
    using UnityEditor;
    using UnityEditor.PackageManager;

    public static class EmbedPackageMenu {
        [MenuItem("Assets/Embed Package", false, 30)]
        private static void EmbedPackageMenuItem() {
            var selection   = Selection.activeObject;
            var packageName = Path.GetFileName(AssetDatabase.GetAssetPath(selection));

            Client.Embed(packageName);

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Embed Package", true)]
        private static bool EmbedPackageValidation() {
            var selection = Selection.activeObject;

            if (selection == null) {
                return false;
            }

            var path   = AssetDatabase.GetAssetPath(selection);
            var folder = Path.GetDirectoryName(path);

            return folder == "Packages";
        }
    }
}