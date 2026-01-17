namespace Multicast.Utilities {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;

    public static class ScriptableObjectCreator {
        public static void ShowDialog<T>(string defaultDestinationPath, Action<ScriptableObject> onScriptableObjectCreated = null)
            where T : ScriptableObject {
            ShowDialog(defaultDestinationPath, typeof(T), o => onScriptableObjectCreated?.Invoke(o as T));
        }

        public static void ShowDialog(string defaultDestinationPath, Type type, Action<ScriptableObject> onScriptableObjectCreated = null) {
            var selector = new ScriptableObjectSelector(defaultDestinationPath, type, onScriptableObjectCreated);

            if (selector.SelectionTree.EnumerateTree().Count() == 1) {
                // If there is only one scriptable object to choose from in the selector, then 
                // we'll automatically select it and confirm the selection. 
                selector.SelectionTree.EnumerateTree().First().Select();
                selector.SelectionTree.Selection.ConfirmSelection();
            }
            else {
                // Else, we'll open up the selector in a popup and let the user choose.
                selector.ShowInPopup(200);
            }
        }

        // Here is the actual ScriptableObjectSelector which inherits from OdinSelector.
        // You can learn more about those in the documentation: http://sirenix.net/odininspector/documentation/sirenix/odininspector/editor/odinselector(t)
        // This one builds a menu-tree of all types that inherit from T, and when the selection is confirmed, it then prompts the user
        // with a dialog to save the newly created scriptable object.

        private class ScriptableObjectSelector : OdinSelector<Type> {
            private readonly Action<ScriptableObject> onScriptableObjectCreated;
            private readonly string                   defaultDestinationPath;
            private readonly Type                     type;

            public ScriptableObjectSelector(string defaultDestinationPath, Type type, Action<ScriptableObject> onScriptableObjectCreated = null) {
                this.onScriptableObjectCreated =  onScriptableObjectCreated;
                this.defaultDestinationPath    =  defaultDestinationPath;
                this.type                      =  type;
                this.SelectionConfirmed        += this.ShowSaveFileDialog;
            }

            protected override void BuildSelectionTree(OdinMenuTree tree) {
                var scriptableObjectTypes = AssemblyUtilities.GetTypes(AssemblyCategory.Scripts | AssemblyCategory.ImportedAssemblies)
                    .Where(x => x.IsClass && !x.IsAbstract && x.InheritsFrom(this.type));

                tree.Selection.SupportsMultiSelect = false;
                tree.Config.DrawSearchToolbar      = true;
                tree.AddRange(scriptableObjectTypes, x => x.GetNiceName())
                    .AddThumbnailIcons();
            }

            private void ShowSaveFileDialog(IEnumerable<Type> selection) {
                var dest = this.defaultDestinationPath.TrimEnd('/');

                if (!Directory.Exists(dest)) {
                    Directory.CreateDirectory(dest);
                    AssetDatabase.Refresh();
                }

                var actualType = selection.First();

                dest = EditorUtility.SaveFilePanel("Save object as", dest, "New " + actualType.GetNiceName(), "asset");

                if (!string.IsNullOrEmpty(dest) && PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), dest, out dest)) {
                    var obj = ScriptableObject.CreateInstance(actualType);
                    AssetDatabase.CreateAsset(obj, dest);
                    AssetDatabase.Refresh();

                    this.onScriptableObjectCreated?.Invoke(obj);
                }
            }
        }
    }
}