namespace Multicast.EditorTools {
    using UnityEditor;

    public static class ReloadScriptsMenu {
        [MenuItem("Edit/Reload Scripts", priority = 300)]
        public static void ReloadScripts() {
            EditorUtility.RequestScriptReload();
        }
    }
}