namespace Multicast.Balance {
    using System;
    using System.Linq;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    public class BalanceWindow : OdinMenuEditorWindow {
        [MenuItem("Tools/Balance")]
        private static void OpenWindow() {
            var window = GetWindow<BalanceWindow>();
            window.titleContent = new GUIContent("Balance");
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree() {
            var tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;

            var pageTypes = TypeCache.GetTypesDerivedFrom<InternalBalancePage>()
                .Where(t => t.IsAbstract == false)
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .ToList();

            foreach (var pageType in pageTypes) {
                var obj = Activator.CreateInstance(pageType);
                var path = obj is InternalBalancePage balancePage
                    ? balancePage.Path
                    : pageType.Name.Replace("_", "/");

                tree.AddObjectAtPath(path, obj);
            }

            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle         = OdinMenuStyle.TreeViewStyle;
            tree.SortMenuItemsByName();

            tree.Selection.SelectionChanged += type => {
                if (type != SelectionChangedType.ItemAdded) {
                    return;
                }

                if (tree.Selection.SelectedValue is InternalBalancePage balancePage) {
                    balancePage.LoadAndInitialize(false);
                }
            };

            return tree;
        }

        protected override void OnBeginDrawEditors() {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Space(5);

                if (GUILayout.Button("Reload", EditorStyles.toolbarButton)) {
                    if (this.MenuTree.Selection.SelectedValue is InternalBalancePage balancePage) {
                        balancePage.LoadAndInitialize(true);
                    }
                }

                GUILayout.Space(5);

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            base.OnBeginDrawEditors();
        }
    }
}