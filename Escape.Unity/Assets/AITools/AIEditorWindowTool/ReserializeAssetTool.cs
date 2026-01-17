using UnityEngine;
using UnityEditor;

namespace AITools.AIEditorWindowTool
{
    public class ReserializeAssetTool : AIEditorWindow
    {
        [MenuItem("GPTGenerated/" + nameof(ReserializeAssetTool))]
        public static void ShowWindow()
        {
            var window = GetWindow<ReserializeAssetTool>();
            window.titleContent = new GUIContent("ReserializeAssetTool");
            window.Show();
        }

        private void OnEnable()
        {
            base.OnEnable();
        }

        private void OnDisable()
        {
            base.OnDisable();
        }

        private void OnGUI()
        {
            base.OnGUI();

            if (GUILayout.Button("Reserialize Selected Assets"))
            {
                var selectedObjects = Selection.objects;
                foreach (var obj in selectedObjects)
                {
                    if (obj != null)
                    {
                        EditorUtility.SetDirty(obj);
                    }
                }
                AssetDatabase.SaveAssets();
                Debug.Log($"Reserialized {selectedObjects.Length} assets.");
            }
        }
    }
}