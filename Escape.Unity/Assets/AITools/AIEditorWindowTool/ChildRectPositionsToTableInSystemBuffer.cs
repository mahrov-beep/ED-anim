using UnityEngine;
using UnityEditor;
using System.Text;

namespace AITools.AIEditorWindowTool {
    public class ChildRectPositionsToTableInSystemBuffer : AIEditorWindow {
        private string lastResult = "";

        [MenuItem("GPTGenerated/" + nameof(ChildRectPositionsToTableInSystemBuffer))]
        public static void ShowWindow() {
            GetWindow<ChildRectPositionsToTableInSystemBuffer>(nameof(ChildRectPositionsToTableInSystemBuffer));
        }

        protected override void OnEnable() {
            base.OnEnable();
        }

        protected override void OnDisable() {
            base.OnDisable();
        }

        public override void OnGUI() {
            base.OnGUI();

            var selectedRoot = Selection.activeGameObject;

            EditorGUILayout.LabelField("Use current Selection as parent (with GameModeItemView children):");
            EditorGUILayout.ObjectField(selectedRoot, typeof(GameObject), true);

            // Check for children
            var hasChildren = selectedRoot != null && selectedRoot.transform.childCount > 0;

            GUI.enabled = hasChildren;

            if (GUILayout.Button("Copy Child RectTransform Positions to Clipboard")) {
                CopyAnchoredPositionsToClipboard(selectedRoot);
            }

            GUI.enabled = true;

            if (!string.IsNullOrEmpty(lastResult)) {
                EditorGUILayout.LabelField("Preview:");
                EditorGUILayout.TextArea(lastResult, GUILayout.ExpandHeight(true), GUILayout.MinHeight(80));
            }
        }

        private void CopyAnchoredPositionsToClipboard(GameObject selectedRoot) {
            if (selectedRoot == null) {
                lastResult = "";
                return;
            }

            var sb    = new StringBuilder();
            var first = true;

            for (var i = 0; i < selectedRoot.transform.childCount; ++i) {
                var child         = selectedRoot.transform.GetChild(i);
                var rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform) {
                    var parentRect = rectTransform.parent as RectTransform;
                    var pos        = rectTransform.anchoredPosition;
                    if (parentRect) {
                        // anchoredPosition matches X,Y seen in Transform on RectTransform UI
                        // that's what's visible in Inspector for RectTransform under "Pos X/Y"
                        // No need for custom calculations, just display anchoredPosition
                    }

                    if (!first) {
                        sb.AppendLine();
                    }
                    sb.AppendFormat("{0}\t{1}", Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
                    first = false;
                }
            }
            lastResult = sb.ToString();

            if (!string.IsNullOrEmpty(lastResult)) {
                EditorGUIUtility.systemCopyBuffer = lastResult;
            }
        }
    }
}