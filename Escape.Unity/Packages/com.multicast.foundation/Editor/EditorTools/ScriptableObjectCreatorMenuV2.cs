namespace Multicast.EditorTools {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;

    public static class ScriptableObjectCreatorMenuV2 {
        [MenuItem("Assets/Create Scriptable Object... %N", priority = -10000)]
        private static void ShowDialog() {
            var path = "Assets";
            var obj  = Selection.activeObject;
            if (obj && AssetDatabase.Contains(obj)) {
                path = AssetDatabase.GetAssetPath(obj) ?? path;
                if (!Directory.Exists(path)) {
                    path = Path.GetDirectoryName(path) ?? path;
                }
            }

            path = path.TrimEnd('/');

            var selector = new GenericSelector<Type>("Create", false, type => type.Name);
            selector.SelectionTree.AddRange(CollectScriptableObjectTypes(), GetMenuPathForType);
            selector.SelectionTree.SortMenuItemsByName();
            selector.SelectionConfirmed += x => CreateAsset(x.LastOrDefault(), path);
            selector.ShowInPopup(new Rect(10, 10, 270, 200));
        }

        private static HashSet<Type> CollectScriptableObjectTypes() {
            return new HashSet<Type>(AssemblyUtilities.GetTypes(AssemblyCategory.Scripts | AssemblyCategory.ImportedAssemblies)
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    typeof(ScriptableObject).IsAssignableFrom(t) &&
                    !typeof(EditorWindow).IsAssignableFrom(t) &&
                    !typeof(Editor).IsAssignableFrom(t)
                ));
        }

        private static string GetMenuPathForType(Type t) {
            if (t.GetCustomAttribute<CreateAssetMenuAttribute>() is {menuName: var customMenuName} &&
                !string.IsNullOrEmpty(customMenuName)) {
                return customMenuName;
            }

            var path = "./" + t.Assembly.GetName().Name + "/";

            if (t.Namespace != null) {
                path = t.Namespace.Split('.').Aggregate(path, (current, it) => current + it.SplitPascalCase() + "/");
            }

            return path + t.Name.SplitPascalCase();
        }

        private static string GetFileNameForType(Type t) {
            if (t.GetCustomAttribute<CreateAssetMenuAttribute>() is {fileName: var customFileName} &&
                !string.IsNullOrEmpty(customFileName)) {
                return customFileName;
            }

            return t.Name + ".asset";
        }

        private static void CreateAsset(Type type, string directory) {
            var so = ScriptableObject.CreateInstance(type);

            var dest = Path.Combine(directory, GetFileNameForType(type));
            dest = AssetDatabase.GenerateUniqueAssetPath(dest);
            ProjectWindowUtil.CreateAsset(so, dest);
        }
    }
}