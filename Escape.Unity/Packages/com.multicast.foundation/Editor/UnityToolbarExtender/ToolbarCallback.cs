using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;

namespace Multicast.UnityToolbarExtender {
    public static class ToolbarCallback {
        private static readonly Type ToolbarType        = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static readonly Type GUIViewType        = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
        private static readonly Type IWindowBackendType = typeof(Editor).Assembly.GetType("UnityEditor.IWindowBackend");

        private static PropertyInfo windowBackend = GUIViewType.GetProperty("windowBackend",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo viewVisualTree = IWindowBackendType.GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static ScriptableObject currentToolbar;

        /// <summary>
        /// Callback for toolbar OnGUI method.
        /// </summary>
        public static Action OnToolbarGUI;
        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIRight;

        static ToolbarCallback() {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate() {
            // Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
            if (currentToolbar != null) {
                return;
            }

            // Find toolbar
            var toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);
            currentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;
            if (currentToolbar == null) {
                return;
            }

            var root    = currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            var rawRoot = root.GetValue(currentToolbar);
            var mRoot   = rawRoot as VisualElement;

            RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
            RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

            void RegisterCallback(string root, Action cb) {
                var toolbarZone = mRoot.Q(root);

                var parent = new VisualElement() {
                    style = {
                        flexGrow      = 1,
                        flexDirection = FlexDirection.Row,
                    }
                };
                var container = new IMGUIContainer();
                container.style.flexGrow =  1;
                container.onGUIHandler   += () => { cb?.Invoke(); };
                parent.Add(container);
                toolbarZone.Add(parent);
            }
        }

        private static void OnGUI() {
            var handler = OnToolbarGUI;
            handler?.Invoke();
        }
    }
}