// ReSharper disable EnforceIfStatementBraces
namespace AITools.AIEditorWindowTool {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using static UnityEditor.EditorGUIUtility;

    [System.Serializable]
    public class ContextScriptsToolbar {
        public int Count => contextScripts.Count;

        [SerializeField]
        private List<MonoScript> contextScripts = new();

        private Vector2 scrollPos;
        private bool    contextFilesFoldout;

        private readonly string prefsKey;

        public ContextScriptsToolbar(string prefsKey) {
            this.prefsKey = prefsKey;

            Load();
        }

        public void Add(MonoScript ms) {
            if (!ms) return;
            if (contextScripts.Contains(ms)) return;

            contextScripts.Add(ms);

            Save();
        }

        public void RemoveAt(int idx) {
            if (idx < 0) return;
            if (idx >= contextScripts.Count) return;

            contextScripts.RemoveAt(idx);

            Save();
        }

        public void Remove(MonoScript ms) {
            if (contextScripts.Remove(ms))
                Save();
        }

        public void Clear() {
            contextScripts.Clear();
            Save();
        }

        public void Save() {
            if (contextScripts.Count == 0) {
                EditorPrefs.DeleteKey(prefsKey);
                return;
            }

            var paths = contextScripts
                            .Where(ms => ms)
                            .Select(AssetDatabase.GetAssetPath)
                            .Where(p => !string.IsNullOrEmpty(p) && p.EndsWith(".cs"))
                            .ToArray();

            EditorPrefs.SetString(prefsKey, string.Join("|", paths));
        }

        public void Load() {
            contextScripts.Clear();

            if (!EditorPrefs.HasKey(prefsKey)) return;

            var joined = EditorPrefs.GetString(prefsKey);

            if (string.IsNullOrEmpty(joined)) return;

            foreach (var p in joined.Split('|')) {

                if (string.IsNullOrEmpty(p)) continue;

                var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(p);

                if (!ms) continue;
                if (contextScripts.Contains(ms)) continue;

                contextScripts.Add(ms);
            }
        }

        public string GetCombinedContextText() {
            var sb = new StringBuilder();
            foreach (var ms in contextScripts) {
                if (!ms) continue;

                var path = AssetDatabase.GetAssetPath(ms);

                if (string.IsNullOrEmpty(path)) continue;
                if (!File.Exists(path)) continue;

                sb.AppendLine($"File: {Path.GetFileName(path)}");
                sb.AppendLine(File.ReadAllText(path));
            }

            return sb.ToString();
        }

        public void Draw() {
            contextFilesFoldout = EditorGUILayout.Foldout(contextFilesFoldout, "Context Scripts (drag .cs files)", true);

            if (!contextFilesFoldout) return;
            if (Count > 0)
                if (GUILayout.Button("Reset List"))
                    Clear();

            HandleDragAndDrop();
            EditorGUILayout.Space(5);
            DrawListGUI();
            EditorGUILayout.Space(5);
        }

        void DrawMoveButtons(int index, out bool moveUp, out bool moveDown, out bool moveTop, out bool moveBottom) {
            moveUp = moveDown = moveTop = moveBottom = false;

            if (GUILayout.Button(new GUIContent("↑", "Move Up"), GUILayout.MaxWidth(25)) && index > 0)
                moveUp = true;

            if (GUILayout.Button(new GUIContent("↓", "Move Down"), GUILayout.MaxWidth(25)) && index < contextScripts.Count - 1)
                moveDown = true;

            if (GUILayout.Button(new GUIContent("⇑", "Move To Top"), GUILayout.MaxWidth(25)) && index > 0)
                moveTop = true;

            if (GUILayout.Button(new GUIContent("⇓", "Move To Bottom"), GUILayout.MaxWidth(25)) && index < contextScripts.Count - 1)
                moveBottom = true;
        }

        void MoveScript(int from, int to) {
            var item = contextScripts[from];
            contextScripts.RemoveAt(from);
            contextScripts.Insert(to, item);
        }

        void DrawListGUI() {
            if (contextScripts == null) return;

            var listHeight = Mathf.Clamp(contextScripts.Count * 22f + 20f, 60f, 400f);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(listHeight));

            int removeIndex = -1;
            int moveUp      = -1, moveDown = -1, moveTop = -1, moveBottom = -1;

            for (var i = 0; i < contextScripts.Count; i++) {
                EditorGUILayout.BeginHorizontal();

                contextScripts[i] = (MonoScript)EditorGUILayout.ObjectField(contextScripts[i], typeof(MonoScript), false);

                bool up, down, top, bottom;
                DrawMoveButtons(i, out up, out down, out top, out bottom);
                if (up) moveUp         = i;
                if (down) moveDown     = i;
                if (top) moveTop       = i;
                if (bottom) moveBottom = i;

                if (GUILayout.Button("Copy name", GUILayout.MaxWidth(75)))
                    systemCopyBuffer = contextScripts[i].name;

                if (GUILayout.Button("Copy file", GUILayout.MaxWidth(65)))
                    systemCopyBuffer = contextScripts[i].text;

                if (GUILayout.Button("Remove", GUILayout.MaxWidth(65)))
                    removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (moveUp > 0) {
                MoveScript(moveUp, moveUp - 1);
                Save();
            }

            if (moveDown >= 0 && moveDown < contextScripts.Count - 1) {
                MoveScript(moveDown, moveDown + 1);
                Save();
            }

            if (moveTop > 0) {
                MoveScript(moveTop, 0);
                Save();
            }

            if (moveBottom >= 0 && moveBottom < contextScripts.Count - 1) {
                MoveScript(moveBottom, contextScripts.Count - 1);
                Save();
            }

            if (removeIndex >= 0)
                RemoveAt(removeIndex);
        }

        void HandleDragAndDrop() {
            var dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag C# scripts here", EditorStyles.helpBox);

            var evt = Event.current;
            if (evt.type is not (EventType.DragUpdated or EventType.DragPerform))
                return;

            if (!dropArea.Contains(evt.mousePosition))
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type != EventType.DragPerform)
                return;

            DragAndDrop.AcceptDrag();

            foreach (var obj in DragAndDrop.objectReferences) {
                var ms = obj as MonoScript;

                if (!ms) {
                    var p = AssetDatabase.GetAssetPath(obj);

                    if (!string.IsNullOrEmpty(p) && p.EndsWith(".cs"))
                        ms = AssetDatabase.LoadAssetAtPath<MonoScript>(p);
                }

                if (ms) Add(ms);
            }

            Event.current.Use();
        }
    }
}