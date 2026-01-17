namespace Multicast.Build {
    using System.Linq;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    public class BuildWindow : OdinMenuEditorWindow {
        [MenuItem("Tools/Build")]
        private static void OpenWindow() {
            var window = GetWindow<BuildWindow>();
            window.titleContent = new GUIContent("Build");
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree() {
            var tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;

            var configurations = AssetDatabase.FindAssets("t: " + typeof(BuildConfiguration).FullName)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<BuildConfiguration>)
                .Where(it => it != null)
                .Where(it => !it.IsTemplate)
                .ToList();

            foreach (var configuration in configurations) {
                tree.AddObjectAtPath(configuration.name, configuration);
            }

            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle         = OdinMenuStyle.TreeViewStyle;
            tree.SortMenuItemsByName();

            return tree;
        }
    }
}