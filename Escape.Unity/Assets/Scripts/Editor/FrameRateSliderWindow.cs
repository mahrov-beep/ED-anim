using System;
using UnityEditor;
using UnityEngine;

public class FrameRateSliderWindow : EditorWindow {
    [MenuItem("Tools/Frame Rate Slider")]
    public static void Open() {
        var window = GetWindow<FrameRateSliderWindow>();
        window.titleContent = new GUIContent("Frame Rate");
        window.ShowPopup();
    }

    private void OnGUI() {
        EditorGUI.BeginDisabledGroup(disabled: Application.isPlaying == false);
        EditorGUI.BeginChangeCheck();

        var newFrameRate = EditorGUILayout.IntSlider(new GUIContent("Frame Rate"), Application.targetFrameRate, 5, 200);

        if (EditorGUI.EndChangeCheck()) {
            Application.targetFrameRate = newFrameRate;
        }

        EditorGUI.EndDisabledGroup();
    }
}