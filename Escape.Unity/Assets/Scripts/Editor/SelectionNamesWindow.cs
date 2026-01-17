using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SelectionNamesWindow : EditorWindow {
    [MenuItem("Tools/Selection Names")]
    public static void Open() {
        var window = GetWindow<SelectionNamesWindow>();
        window.titleContent = new GUIContent("Selection Names");
        window.ShowPopup();
    }

    private string format = "{0}";
    private string names;

    private void OnEnable() {
        Selection.selectionChanged += this.OnSelectionChanged;

        this.RefreshNames();
    }

    private void OnDisable() {
        Selection.selectionChanged -= this.OnSelectionChanged;
    }

    private void OnFocus() {
        this.RefreshNames();
    }

    private void OnSelectionChanged() {
        this.RefreshNames();
    }

    private void RefreshNames() {
        this.names = Selection.objects.Aggregate("", (sum, it) => sum + string.Format(this.format, it.name) + Environment.NewLine);
        this.Repaint();
    }

    private void OnGUI() {
        EditorGUI.BeginChangeCheck();

        this.format = EditorGUILayout.TextField("Format", this.format, GUILayout.ExpandWidth(true));

        if (EditorGUI.EndChangeCheck()) {
            this.RefreshNames();
        }

        GUILayout.TextArea(this.names, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }
}