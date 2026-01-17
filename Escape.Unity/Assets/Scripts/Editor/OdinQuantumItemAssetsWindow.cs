namespace Scripts.Editor {
    using System.Linq;
    using Quantum;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    public class OdinQuantumItemAssetsWindow : OdinMenuEditorWindow {
        [MenuItem("Game Assets/Quantum Assets Window")]
        private static void OpenWindow() {
            var window = GetWindow<OdinQuantumItemAssetsWindow>();
            window.titleContent = new GUIContent("Quantum Assets");
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree() {
            var tree = new OdinMenuTree();

            var assets = AssetDatabase.FindAssets("t: " + typeof(AssetObject))
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<AssetObject>(path))
                .ToList();

            foreach (var itemAsset in assets) {
                var path = GetItemPath(itemAsset);

                if (string.IsNullOrEmpty(path)) {
                    continue;
                }

                tree.AddObjectAtPath(path, itemAsset);
            }

            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle         = OdinMenuStyle.TreeViewStyle;
            tree.SortMenuItemsByName();

            return tree;
        }

        private static string GetItemPath(AssetObject asset) {
            return asset switch {
                ItemAsset itemAsset => $"Items/{itemAsset.Grouping}/{itemAsset.ItemKey}",
                AttackData attackData => $"Attacks/{attackData.name}",
                GameModeAsset gameModeAsset => $"GameModes/{gameModeAsset.gameModeKey}",
                _ => null,
            };
        }
    }
}