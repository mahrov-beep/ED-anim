using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AITools.AIEditorWindowTool
{
    public class SkinColorizerHelper : AIEditorWindow
    {
        [System.Serializable]
        private class MaterialColorBackup
        {
            public Renderer renderer;
            public int index;
            public Color originalColor;
        }

        private Color color = Color.white;
        private List<MaterialColorBackup> backups = new();

        [MenuItem("GPTGenerated/" + nameof(SkinColorizerHelper))]
        public static void ShowWindow()
        {
            GetWindow<SkinColorizerHelper>("Skin Colorizer Helper");
        }

        public override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Label("Skin Colorizer", EditorStyles.boldLabel);
            color = EditorGUILayout.ColorField("Цвет", color);

            GameObject selectedGO = Selection.activeGameObject;
            bool validSelection = selectedGO != null && selectedGO.GetComponentsInChildren<Renderer>(true).Length > 0;

            EditorGUI.BeginDisabledGroup(!validSelection);
            if (GUILayout.Button("Применить цвет"))
            {
                ApplyColor(selectedGO);
            }
            if (GUILayout.Button("Сбросить цвет"))
            {
                ResetColor();
            }
            EditorGUI.EndDisabledGroup();

            if (!validSelection)
            {
                EditorGUILayout.HelpBox("Выберите GameObject с дочерними Renderer-компонентами.", MessageType.Info);
            }
        }

        private void ApplyColor(GameObject targetGO)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Apply Color");
            int group = Undo.GetCurrentGroup();

            backups.Clear();
            var renderers = targetGO.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials;
                Undo.RegisterCompleteObjectUndo(renderer, "Apply Color");

                for (int i = 0; i < materials.Length; i++)
                {
                    var mat = materials[i];
                    if (mat == null || !mat.HasProperty("_Color")) continue;
                    if (mat.shader.name == "Standard") continue;

                    backups.Add(new MaterialColorBackup
                    {
                        renderer = renderer,
                        index = i,
                        originalColor = mat.GetColor("_Color")
                    });

                    var block = new MaterialPropertyBlock();
                    block.SetColor("_Color", color);
                    renderer.SetPropertyBlock(block, i);
                }
            }

            Undo.CollapseUndoOperations(group);
        }

        private void ResetColor()
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Reset Color");
            int group = Undo.GetCurrentGroup();

            foreach (var backup in backups)
            {
                if (backup.renderer == null) continue;
                Undo.RegisterCompleteObjectUndo(backup.renderer, "Reset Color");

                var mat = backup.renderer.sharedMaterials[backup.index];
                if (mat == null || !mat.HasProperty("_Color")) continue;

                var block = new MaterialPropertyBlock();
                block.SetColor("_Color", backup.originalColor);
                backup.renderer.SetPropertyBlock(block, backup.index);
            }

            backups.Clear();
            Undo.CollapseUndoOperations(group);
        }
    }
}