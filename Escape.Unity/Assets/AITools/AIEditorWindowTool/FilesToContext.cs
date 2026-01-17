using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace AITools.AIEditorWindowTool
{
    public class FilesToContext : AIEditorWindow
    {
        private const string PREFS_KEY = "AITools.AIEditorWindowTool.FilesToContext.SelectedScripts";
        private ContextScriptsToolbar scriptsToolbar;
        private Vector2 scrollPos;

        [MenuItem("GPTGenerated/" + nameof(FilesToContext))]
        public static void ShowWindow()
        {
            var wnd = GetWindow<FilesToContext>();
            wnd.titleContent = new GUIContent("FilesToContext");
            wnd.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (scriptsToolbar == null)
                scriptsToolbar = new ContextScriptsToolbar(PREFS_KEY);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnGUI()
        {
            base.OnGUI();
            scriptsToolbar.Draw();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("CopyToBuffer"))
            {
                EditorGUIUtility.systemCopyBuffer = scriptsToolbar.GetCombinedContextText();
            }

            if (GUILayout.Button("ResetSelected"))
            {
                scriptsToolbar.Clear();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}