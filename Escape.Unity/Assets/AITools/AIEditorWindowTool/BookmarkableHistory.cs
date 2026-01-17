using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AITools.AIEditorWindowTool
{
    public class BookmarkableHistory : AIEditorWindow
    {
        private const string EditorPrefsKeyPath = "BookmarkablePinnedAssetPath";
        private const string EditorPrefsKeyPinnedCount = "BookmarkablePinnedAssetCount";

        [System.Serializable]
        public class HistoryData
        {
            public List<string> selectionHistoryPaths;
            public List<string> selectionHistoryPinnedPaths;
        }

        private enum FilterType
        {
            All,
            Prefabs,
            Assets,
        }

        private FilterType currentFilter = FilterType.All;

        private int dragSourceIndex = -1;
        private bool isDraggingItem = false;
        private bool dragFromPinnedGroup = false;

        private AnimBool settingAnimation;
        private bool settingExpanded;
        private AnimBool clearAnimation;
        private bool historyVisible = true;

        private static List<Object> selectionHistory = new();
        private static HashSet<Object> selectionHistorySet = new();
        private static List<Object> selectionHistoryPinned = new();

        private static bool muteRecording;
        private static int selectedIndex = -1;

        private bool _hasFocus;

        public static bool RecordHierarchy
        {
            get => EditorPrefs.GetBool("BookmarkableHistory_RecordHierarchy", true);
            set => EditorPrefs.SetBool("BookmarkableHistory_RecordHierarchy", value);
        }

        public static bool RecordProject
        {
            get => EditorPrefs.GetBool("BookmarkableHistory_RecordProject", true);
            set => EditorPrefs.SetBool("BookmarkableHistory_RecordProject", value);
        }

        public static bool WorkInBackground
        {
            get => EditorPrefs.GetBool("BookmarkableHistory_WorkInBackground", false);
            set => EditorPrefs.SetBool("BookmarkableHistory_WorkInBackground", value);
        }

        public static int MaxHistorySize
        {
            get => EditorPrefs.GetInt("BookmarkableHistory_MaxHistorySize", 50);
            set => EditorPrefs.SetInt("BookmarkableHistory_MaxHistorySize", value);
        }

        private static void SavePinnedToPrefs()
        {
            var oldCount = EditorPrefs.GetInt(EditorPrefsKeyPinnedCount, 0);
            for (int i = 0; i < oldCount; i++)
                EditorPrefs.DeleteKey(EditorPrefsKeyPath + i);

            for (var i = 0; i < selectionHistoryPinned.Count; i++)
            {
                var assetPath = AssetDatabase.GetAssetPath(selectionHistoryPinned[i]);
                EditorPrefs.SetString(EditorPrefsKeyPath + i, assetPath);
            }
            EditorPrefs.SetInt(EditorPrefsKeyPinnedCount, selectionHistoryPinned.Count);
        }

        private static void RestorePinnedFromPrefs()
        {
            selectionHistoryPinned.Clear();
            int count = EditorPrefs.GetInt(EditorPrefsKeyPinnedCount, 0);
            for (int i = 0; i < count; i++)
            {
                var assetPath = EditorPrefs.GetString(EditorPrefsKeyPath + i, "");
                var loadedAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (loadedAsset) selectionHistoryPinned.Add(loadedAsset);
            }
        }

        private static void OnSelectionChanged()
        {
            if (!WorkInBackground) return;
            if (muteRecording || !Selection.activeObject) return;
            AddToHistory();
        }

        private void OnSelectionChange()
        {
            Repaint();
            if (WorkInBackground) return;
            if (muteRecording || !Selection.activeObject) return;
            AddToHistory();
        }

        private void OnFocus()
        {
            var cleaned = new List<Object>();
            foreach (var obj in selectionHistory)
                if (obj != null) cleaned.Add(obj);
            selectionHistory = cleaned;
            selectionHistorySet = new HashSet<Object>(selectionHistory);
            _hasFocus = true;
        }

        private void OnLostFocus()
        {
            _hasFocus = false;
        }

        private void OnInspectorUpdate()
        {
            if (_hasFocus) Repaint();
        }

        private static void AddToHistory()
        {
            Object active = Selection.activeObject;
            if (active is DefaultAsset) return;
            if (EditorUtility.IsPersistent(active) && !RecordProject) return;
            if (!EditorUtility.IsPersistent(active) && !RecordHierarchy) return;
            if (!selectionHistorySet.Contains(active))
            {
                selectionHistory.Insert(0, active);
                selectionHistorySet.Add(active);
            }
            if (selectionHistory.Count > MaxHistorySize)
            {
                Object removed = selectionHistory[selectionHistory.Count - 1];
                selectionHistory.RemoveAt(selectionHistory.Count - 1);
                selectionHistorySet.Remove(removed);
            }
        }

        private static void ListenForNavigationInput(SceneView sceneView)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.isKey && Event.current.keyCode == KeyCode.LeftBracket)
            {
                SelectPrevious();
            }
            if (Event.current.type == EventType.KeyDown && Event.current.isKey && Event.current.keyCode == KeyCode.RightBracket)
            {
                SelectNext();
            }
        }

        private static void SetSelection(Object target, int index)
        {
            muteRecording = true;
            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
            muteRecording = false;
        }

        private static void SelectPrevious()
        {
            if (selectionHistory.Count == 0) return;
            selectedIndex--;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, selectionHistory.Count - 1);
            SetSelection(selectionHistory[selectedIndex], selectedIndex);
        }

        private static void SelectNext()
        {
            if (selectionHistory.Count == 0) return;
            selectedIndex++;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, selectionHistory.Count - 1);
            SetSelection(selectionHistory[selectedIndex], selectedIndex);
        }

        private Vector2 scrollPos;

        private void OnGUI()
        {
            base.OnGUI();

            _hasFocus = _hasFocus || Event.current.type == EventType.MouseMove;
            DrawHeader();
            if (EditorGUILayout.BeginFadeGroup(settingAnimation.faded))
            {
                EditorGUILayout.Space();
                DrawSettings();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFadeGroup();
            clearAnimation.target = !historyVisible;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox, GUILayout.MaxHeight(maxSize.y - 20f));
            EditorGUILayout.BeginFadeGroup(1f - clearAnimation.faded);
            DrawPinnedHistory();
            DrawHistory();
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndScrollView();
            if (clearAnimation.faded == 1f)
            {
                selectionHistory.Clear();
                selectionHistorySet.Clear();
            }
            if (selectionHistory.Count == 0) historyVisible = true;
        }

        private void DrawHeader()
        {
            bool isProSkin = EditorGUIUtility.isProSkin;
            var backIcon = EditorGUIUtility.IconContent((isProSkin ? "d_" : "") + "back@2x").image;
            var forwardIcon = EditorGUIUtility.IconContent((isProSkin ? "d_" : "") + "forward@2x").image;
            var trashIcon = EditorGUIUtility.IconContent((isProSkin ? "d_" : "") + "TreeEditor.Trash").image;
            var settingsIcon = EditorGUIUtility.IconContent((isProSkin ? "d_" : "") + "Settings").image;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(selectionHistory.Count == 0))
                {
                    using (new EditorGUI.DisabledScope(selectedIndex == selectionHistory.Count - 1))
                    {
                        if (GUILayout.Button(
                                new GUIContent(backIcon, "Select previous (Left bracket key)"),
                                EditorStyles.miniButtonLeft, GUILayout.Height(20f), GUILayout.Width(30f)))
                        {
                            SelectNext();
                        }
                    }
                    using (new EditorGUI.DisabledScope(selectedIndex == 0))
                    {
                        if (GUILayout.Button(
                                new GUIContent(forwardIcon, "Select next (Right bracket key)"),
                                EditorStyles.miniButtonRight, GUILayout.Height(20), GUILayout.Width(30f)))
                        {
                            SelectPrevious();
                        }
                    }
                    if (GUILayout.Button(
                            new GUIContent(trashIcon, "Clear history"),
                            EditorStyles.miniButton, GUILayout.Height(20f), GUILayout.Width(30f)))
                    {
                        historyVisible = false;
                    }
                }
                GUILayout.Space(6);
                DrawFilterButton("All", FilterType.All);
                DrawFilterButton("Prefab", FilterType.Prefabs);
                DrawFilterButton("Asset", FilterType.Assets);
                GUILayout.Space(6);
                if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Height(20f), GUILayout.Width(44f)))
                    ExportHistory();
                if (GUILayout.Button("Paste", EditorStyles.miniButton, GUILayout.Height(20f), GUILayout.Width(45f)))
                    ImportHistory();
                GUILayout.FlexibleSpace();
                settingExpanded = GUILayout.Toggle(
                    settingExpanded,
                    new GUIContent(settingsIcon, "Edit settings"),
                    EditorStyles.miniButtonMid,
                    GUILayout.Height(20f),
                    GUILayout.Width(26f)
                );
                settingAnimation.target = settingExpanded;
            }
        }

        private void DrawFilterButton(string label, FilterType filterType)
        {
            bool isSelected = currentFilter == filterType;
            Color originalColor = GUI.contentColor;
            if (isSelected) GUI.contentColor = Color.green;

            var btnWidth = 9f * label.Length;

            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Height(20f), GUILayout.Width(btnWidth)))
                currentFilter = filterType;

            GUI.contentColor = originalColor;
        }

        private void DrawSettings()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Record", EditorStyles.boldLabel, GUILayout.Width(100f));
                RecordHierarchy = EditorGUILayout.ToggleLeft("Hierarchy", RecordHierarchy, GUILayout.MaxWidth(80f));
                RecordProject = EditorGUILayout.ToggleLeft("Project window", RecordProject);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                WorkInBackground = EditorGUILayout.ToggleLeft("Record in background", WorkInBackground, GUILayout.MaxWidth(160));
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("History size", EditorStyles.boldLabel, GUILayout.Width(100f));
                MaxHistorySize = EditorGUILayout.IntField(MaxHistorySize, GUILayout.MaxWidth(40f));
            }
        }

        private bool FilterItem(Object item)
        {
            string path = AssetDatabase.GetAssetPath(item);
            if (currentFilter == FilterType.Prefabs)
                return !string.IsNullOrEmpty(path) &&
                    path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase);
            else if (currentFilter == FilterType.Assets)
                return EditorUtility.IsPersistent(item) &&
                    !string.IsNullOrEmpty(path) &&
                    !path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase);
            return true;
        }

        private void DrawHistory()
        {
            var filtered = new List<(int index, Object item)>();
            for (int i = 0; i < selectionHistory.Count; i++)
            {
                Object item = selectionHistory[i];
                if (item != null && !selectionHistoryPinned.Contains(item) && FilterItem(item))
                    filtered.Add((i, item));
            }
            DrawHistoryItems(filtered, "Pin", Pin, selectionHistory);
        }

        private void DrawPinnedHistory()
        {
            var filtered = new List<(int index, Object item)>();
            for (int i = 0; i < selectionHistoryPinned.Count; i++)
            {
                Object item = selectionHistoryPinned[i];
                if (item != null && FilterItem(item))
                    filtered.Add((i, item));
            }
            DrawHistoryItems(filtered, "Unpin", Unpin, selectionHistoryPinned);
        }

        // ---- MODIFIED AREA BEGIN ----
        private void DrawHistoryItems(List<(int index, Object item)> items, string actionButtonLabel, System.Action<Object> actionButtonAction, List<Object> baseList)
        {
            bool isProSkin = EditorGUIUtility.isProSkin;
            const float nonProSkinMultiplier1 = 1.7f;
            const float nonProSkinMultiplier2 = 1.66f;
            const float proSkinMultiplier1 = 1f;
            const float proSkinMultiplier2 = 1.05f;
            const float hoverProSkinMultiplier = 1.1f;
            const float hoverNonProSkinMultiplier = 1.5f;
            const float selectionProSkinMultiplier = 1.5f;
            const float buttonHeight = 17f;
            const float buttonWidth = 50f;
            const float outlineOffset = 1f;
            const float outlineAdjustment = 2f;

            Color prevColor = GUI.color;
            Color prevBgColor = GUI.backgroundColor;

            foreach (var tuple in items)
            {
                int underlyingIndex = tuple.index;
                Object historyItem = tuple.item;
                if (historyItem == null) continue;

                Rect rect = EditorGUILayout.GetControlRect(false, buttonHeight);

                float colorMultiplier = (underlyingIndex % 2 == 0)
                    ? (isProSkin ? proSkinMultiplier1 : nonProSkinMultiplier1)
                    : (isProSkin ? proSkinMultiplier2 : nonProSkinMultiplier2);

                GUI.color = Color.grey * colorMultiplier;
                if (rect.Contains(Event.current.mousePosition) || Selection.activeObject == historyItem)
                    GUI.color = isProSkin ? Color.grey * hoverProSkinMultiplier : Color.grey * hoverNonProSkinMultiplier;
                if (Selection.activeObject == historyItem)
                {
                    var outline = rect;
                    outline.x -= outlineOffset;
                    outline.y -= outlineOffset;
                    outline.width += outlineAdjustment;
                    outline.height += outlineAdjustment;
                    var outlineColor = isProSkin ? Color.gray * selectionProSkinMultiplier : Color.gray;
                    EditorGUI.DrawRect(outline, outlineColor);
                }
                EditorGUI.DrawRect(rect, GUI.color);

                GUI.color = prevColor;
                GUI.backgroundColor = prevBgColor;

                // SETUP BUTTON POSITIONS (RIGHT-ALIGNED FOR ACTION BUTTONS)
                float actionButtonsWidth = 0f;
                float buttonSpacing = 2f;
                float assetBtnW = buttonWidth;
                float pinBtnW = buttonWidth;
                float totalRightWidth = 0f;

                string assetPath = AssetDatabase.GetAssetPath(historyItem);
                bool hasAssetButton = !string.IsNullOrEmpty(assetPath) &&
                    (assetPath.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase) || assetPath.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase));
                if (hasAssetButton)
                    totalRightWidth += assetBtnW + buttonSpacing;
                totalRightWidth += pinBtnW;

                float rightEdge = rect.xMax - 2f;
                float pinBtnX = rightEdge - pinBtnW;
                float assetBtnX = pinBtnX;
                if (hasAssetButton)
                {
                    assetBtnX = pinBtnX - assetBtnW - buttonSpacing;
                }

                Rect pinBtnRect = new Rect(pinBtnX, rect.y + 1, pinBtnW, buttonHeight - 2);
                Rect assetBtnRect = new Rect(assetBtnX, rect.y + 1, assetBtnW, buttonHeight - 2);

                float iconWidth = 16f;
                float labelPad = 6f;

                float labelLeft = rect.x + labelPad;
                float labelMaxRight = (hasAssetButton ? assetBtnX : pinBtnX) - buttonSpacing;
                float labelWidth = labelMaxRight - labelLeft;
                if (labelWidth < iconWidth + 10f)
                    labelWidth = iconWidth + 10f;
                Rect labelRect = new Rect(labelLeft, rect.y, labelWidth, buttonHeight);

                // Asset "Load"/"Open" button
                if (hasAssetButton)
                {
                    if (assetPath.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (GUI.Button(assetBtnRect, "Load"))
                        {
                            EditorSceneManager.OpenScene(assetPath);
                        }
                    }
                    else if (assetPath.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (GUI.Button(assetBtnRect, "Open"))
                        {
                            OpenPrefab(historyItem);
                        }
                    }
                }

                if (GUI.Button(pinBtnRect, actionButtonLabel))
                {
                    actionButtonAction(historyItem);
                }

                // Draw label + icon (selection object name, left area)
                var buttonImage = EditorGUIUtility.ObjectContent(historyItem, historyItem.GetType()).image;
                var buttonContent = new GUIContent(" " + historyItem.name, buttonImage);

                if (GUI.Button(labelRect, buttonContent, EditorStyles.label))
                {
                    SetSelection(historyItem, underlyingIndex);
                }

                // Drag logic (work only if click labelRect, not button)
                if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
                    dragSourceIndex = underlyingIndex;
                if (Event.current.type == EventType.MouseDrag && dragSourceIndex == underlyingIndex && !isDraggingItem)
                {
                    isDraggingItem = true;
                    dragFromPinnedGroup = (baseList == selectionHistoryPinned);
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.StartDrag("HistoryDrag");
                    Event.current.Use();
                }
                if (isDraggingItem && rect.Contains(Event.current.mousePosition) &&
                    ((baseList == selectionHistoryPinned && dragFromPinnedGroup) || (baseList == selectionHistory && !dragFromPinnedGroup)))
                {
                    if (Event.current.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                        Event.current.Use();
                    }
                    if (Event.current.type == EventType.DragPerform)
                    {
                        int targetIndex = underlyingIndex;
                        if (targetIndex != dragSourceIndex)
                        {
                            ReorderList(baseList, dragSourceIndex, targetIndex);
                            SavePinnedToPrefsIfNeeded(baseList);
                        }
                        isDraggingItem = false;
                        dragSourceIndex = -1;
                        DragAndDrop.AcceptDrag();
                        Event.current.Use();
                    }
                }

                if (Event.current.type == EventType.MouseUp)
                {
                    isDraggingItem = false;
                    dragSourceIndex = -1;
                }
            }
        }
        // ---- MODIFIED AREA END ----

        private void SavePinnedToPrefsIfNeeded(List<Object> list)
        {
            if (list == selectionHistoryPinned)
                SavePinnedToPrefs();
        }

        private void ReorderList(List<Object> list, int from, int to)
        {
            if (from < 0 || from >= list.Count || to < 0 || to >= list.Count) return;
            Object item = list[from];
            list.RemoveAt(from);
            if (from < to) to--;
            list.Insert(to, item);

            if (list == selectionHistoryPinned)
            {
                SavePinnedToPrefs();
            }
            Repaint();
        }

        private static void OpenPrefab(Object obj)
        {
            AssetDatabase.OpenAsset(obj);
        }

        private void Pin(Object obj)
        {
            if (!selectionHistoryPinned.Contains(obj))
            {
                selectionHistoryPinned.Add(obj);
                SavePinnedToPrefs();
            }
        }

        private void Unpin(Object obj)
        {
            if (selectionHistoryPinned.Contains(obj))
            {
                selectionHistoryPinned.Remove(obj);
                SavePinnedToPrefs();
            }
        }

        private void ExportHistory()
        {
            HistoryData data = new HistoryData();
            data.selectionHistoryPaths = GetPathsFromList(selectionHistory);
            data.selectionHistoryPinnedPaths = GetPathsFromList(selectionHistoryPinned);
            string json = JsonUtility.ToJson(data);
            EditorGUIUtility.systemCopyBuffer = json;
        }

        private void ImportHistory()
        {
            string json = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(json))
                return;
            HistoryData data = null;
            try
            {
                data = JsonUtility.FromJson<HistoryData>(json);
            }
            catch
            {
                return;
            }
            if (data == null)
                return;
            selectionHistory.Clear();
            selectionHistorySet.Clear();
            selectionHistoryPinned.Clear();
            selectionHistory.AddRange(LoadAssetsFromPaths(data.selectionHistoryPaths));
            selectionHistorySet.UnionWith(selectionHistory);
            selectionHistoryPinned.AddRange(LoadAssetsFromPaths(data.selectionHistoryPinnedPaths));
            SavePinnedToPrefs();
            Repaint();
        }

        private static List<string> GetPathsFromList(List<Object> list)
        {
            List<string> paths = new();
            foreach (var item in list)
            {
                if (item == null) continue;
                string path = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(path))
                    paths.Add(path);
            }
            return paths;
        }

        private static List<Object> LoadAssetsFromPaths(List<string> paths)
        {
            List<Object> assets = new();
            if (paths != null)
            {
                foreach (var path in paths)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (asset != null)
                        assets.Add(asset);
                }
            }
            return assets;
        }

        [MenuItem("GPTGenerated/" + nameof(BookmarkableHistory))]
        public static void ShowWindow()
        {
            var window = GetWindow<BookmarkableHistory>();
            window.autoRepaintOnSceneChange = true;
            window.titleContent.text = "Bookmarkable History";
            window.wantsMouseMove = true;
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui += ListenForNavigationInput;
            RestorePinnedFromPrefs();
            settingAnimation = new AnimBool(false);
            settingAnimation.valueChanged.AddListener(Repaint);
            settingAnimation.speed = 4f;
            clearAnimation = new AnimBool(false);
            clearAnimation.valueChanged.AddListener(Repaint);
            clearAnimation.speed = settingAnimation.speed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Selection.selectionChanged -= OnSelectionChanged;
            SceneView.duringSceneGui -= ListenForNavigationInput;
            SavePinnedToPrefs();
        }
    }
}