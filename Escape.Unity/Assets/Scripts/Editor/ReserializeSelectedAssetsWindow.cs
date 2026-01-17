using System;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ReserializeSelectedAssetsWindow : EditorWindow {
    [MenuItem("Tools/Reserialize Selected Assets")]
    public static void Open() {
        var window = GetWindow<ReserializeSelectedAssetsWindow>();
        window.titleContent = new GUIContent("Reserialize Assets");
        window.ShowPopup();
    }

    private void OnEnable() {
        Selection.selectionChanged += this.OnSelectionChanged;

        this.Refresh();
    }

    private void OnDisable() {
        Selection.selectionChanged -= this.OnSelectionChanged;
    }

    private void OnFocus() {
        this.Refresh();
    }

    private void OnSelectionChanged() {
        this.Refresh();
    }

    private void Refresh() {
        this.Repaint();
    }

    [NonSerialized] private Vector2 scroll;

    private void OnGUI() {
        this.scroll = GUILayout.BeginScrollView(this.scroll);

        foreach (var selectedAsset in Selection.objects) {
            EditorGUILayout.ObjectField(selectedAsset, typeof(Object), false);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(10);

        GUIHelper.PushColor(Color.yellow);

        if (GUILayout.Button("Reserialize", GUILayout.Height(40))) {
            var paths = Selection.objects
                .Select(obj => AssetDatabase.GetAssetPath(obj))
                .Where(path => !string.IsNullOrEmpty(path))
                .ToList();

            AssetDatabase.ForceReserializeAssets(paths, ForceReserializeAssetsOptions.ReserializeAssets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        GUIHelper.PopColor();
    }
}