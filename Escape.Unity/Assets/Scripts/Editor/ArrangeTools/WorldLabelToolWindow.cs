namespace Scripts.Editor {
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    public class WorldLabelToolWindow : EditorWindow {
        private string labelText = "Label";
        private float yOffset = 1.5f;
        private int fontSize = 24;
        private Color color = Color.white;
        private float scale = 0.01f;
        private bool faceSceneCamera = true;

        [MenuItem("Tools/World Labeler...")]
        public static void ShowWindow() {
            GetWindow<WorldLabelToolWindow>("World Labeler");
        }

        private void OnGUI() {
            var selection = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            EditorGUILayout.LabelField("Selected objects", selection.Length.ToString());
            labelText = EditorGUILayout.TextField("Text", labelText);
            yOffset = EditorGUILayout.FloatField("Y Offset", yOffset);
            fontSize = EditorGUILayout.IntField("Font Size", fontSize);
            color = EditorGUILayout.ColorField("Color", color);
            scale = EditorGUILayout.FloatField("Scale", scale);
            faceSceneCamera = EditorGUILayout.ToggleLeft("Face Scene Camera", faceSceneCamera);
            EditorGUILayout.Space(8);
            using (new EditorGUI.DisabledScope(selection.Length == 0)) {
                if (GUILayout.Button("Apply")) {
                    Apply(selection);
                }
            }
        }

        private void Apply(Transform[] selection) {
            foreach (var t in selection) {
                if (t == null) continue;
                var label = FindExistingLabel(t);
                if (label.canvas == null || label.text == null) label = CreateLabel(t);
                Undo.RecordObject(label.text, "Update Label");
                label.text.text = labelText;
                label.text.fontSize = fontSize;
                label.text.color = color;
                Undo.RecordObject(label.canvas.transform, "Update Label Transform");
                label.canvas.transform.localPosition = new Vector3(0f, yOffset, 0f);
                label.canvas.transform.localScale = Vector3.one * Mathf.Max(0.0001f, scale);
                if (faceSceneCamera) FaceSceneCam(label.canvas.transform);
            }
            SceneView.RepaintAll();
        }

        private void FaceSceneCam(Transform tr) {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null || sv.camera == null) return;
            var cam = sv.camera.transform;
            var dir = tr.position - cam.position;
            if (dir.sqrMagnitude < 1e-6f) return;
            tr.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        private (Canvas canvas, Text text) CreateLabel(Transform parent) {
            var go = new GameObject(parent.name + "_Label");
            Undo.RegisterCreatedObjectUndo(go, "Create Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, yOffset, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * Mathf.Max(0.0001f, scale);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 60f);
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            var textGo = new GameObject("Text");
            Undo.RegisterCreatedObjectUndo(textGo, "Create Label Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.text = labelText;
            text.fontSize = fontSize;
            text.color = color;
            var textRt = text.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            return (canvas, text);
        }

        private (Canvas canvas, Text text) FindExistingLabel(Transform parent) {
            var canvas = parent.GetComponentsInChildren<Canvas>(true).FirstOrDefault(c => c.renderMode == RenderMode.WorldSpace);
            if (canvas == null) return default;
            var text = canvas.GetComponentInChildren<Text>(true);
            if (text == null) return default;
            return (canvas, text);
        }
    }
}


