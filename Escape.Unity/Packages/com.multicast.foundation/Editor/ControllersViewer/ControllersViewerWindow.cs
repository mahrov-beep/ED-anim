namespace Multicast.ControllersViewer {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public class ControllersViewerWindow : OdinEditorWindow {
        private readonly Dictionary<ControllerBase, ControllerInfo> controllerInfo = new();

        [SerializeField, HideInInspector]
        private float menuWidth = 180f;

        [SerializeField, HideInInspector]
        private Vector2 menuScroll;

        [SerializeField, HideInInspector]
        private string lastPinnedControllerFullName;

        [SerializeField, HideInInspector]
        private string lastSelectedControllerFullName;

        [SerializeField, HideInInspector]
        private string searchText;

        [CanBeNull] private ControllerBase pinnedController;
        [CanBeNull] private ControllerBase selectedController;
        [CanBeNull] private ControllerBase activeFlowController;

        private HashSet<ControllerBase> activeFlowControllersSet = new();

        [MenuItem("Window/MULTICAST GAMES/Controllers Viewer")]
        private static void OpenControllersViewerWindow() {
            var window = GetWindow<ControllersViewerWindow>();
            window.titleContent = new GUIContent("Controllers");
            window.Show();
        }

        protected override void OnEnable() {
            base.OnEnable();

            ControllerBase.ControllerChildrenChanged += this.OnControllerChildrenChanged;
            ControllerBase.ControllerStatusChanged   += this.OnControllerStatusChanged;
            ControllerBase.ActiveControllerChanged   += this.OnActiveControllerChanged;
        }

        protected override void OnDisable() {
            ControllerBase.ControllerChildrenChanged -= this.OnControllerChildrenChanged;
            ControllerBase.ControllerStatusChanged   -= this.OnControllerStatusChanged;
            ControllerBase.ActiveControllerChanged   -= this.OnActiveControllerChanged;

            base.OnDisable();
        }

        protected override object GetTarget() {
            return this.selectedController;
        }

        protected override void OnImGUI() {
            if (this.pinnedController is { Status: ControllerStatus.Disposed }) {
                this.pinnedController = null;
            }

            if (this.selectedController is { Status: ControllerStatus.Disposed }) {
                this.selectedController = null;
            }

            this.activeFlowController = ControllersShared.ActiveFlowController;

            this.activeFlowControllersSet.Clear();
            foreach (var controller in ControllersShared.ActiveFlowControllers) {
                this.activeFlowControllersSet.Add(controller);
            }

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayoutOptions.Width(this.menuWidth).ExpandHeight());

            SirenixEditorGUI.BeginHorizontalToolbar();
            this.DrawMenuToolbar();
            SirenixEditorGUI.EndHorizontalToolbar();

            var currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
            EditorGUI.DrawRect(currentLayoutRect, SirenixGUIStyles.MenuBackgroundColor);

            var resizeRect = new Rect(currentLayoutRect) {
                xMin = currentLayoutRect.xMax - 4f,
                xMax = currentLayoutRect.xMax + 4f,
            };

            EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
            this.menuWidth += SirenixEditorGUI.SlideRect(resizeRect).x;

            this.DrawMenu();

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayoutOptions.ExpandHeight());

            EditorGUI.DrawRect(GUIHelper.GetCurrentLayoutRect(), SirenixGUIStyles.DarkEditorBackground);
            EditorGUI.DrawRect(resizeRect.AlignCenter(1f), SirenixGUIStyles.BorderColor);

            SirenixEditorGUI.BeginHorizontalToolbar();
            this.DrawTargetToolbar();
            SirenixEditorGUI.EndHorizontalToolbar();

            EditorGUI.BeginDisabledGroup(this.selectedController is not { IsRunning: true });
            base.OnImGUI();
            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawMenuToolbar() {
            this.searchText = SirenixEditorGUI.ToolbarSearchField(this.searchText);
        }

        private void DrawTargetToolbar() {
            if (this.selectedController != null) {
                var selectedControllerName = this.selectedController.DebugName ?? this.GetControllerInfo(this.selectedController).FallbackName;
                SirenixEditorGUI.ToolbarTab(isActive: false, selectedControllerName);
            }
        }

        private void DrawMenu() {
            this.menuScroll = GUILayout.BeginScrollView(this.menuScroll);

            var controller = this.pinnedController ?? ControllersShared.RootController;

            if (controller != null) {
                this.DrawControllerMenu(controller, 0);
            }
            else {
                EditorGUILayout.HelpBox("No any controllers", MessageType.Info);
            }

            GUILayout.EndScrollView();
        }

        private void DrawControllerMenu([NotNull] ControllerBase controller, int indentLevel) {
            var info = this.GetControllerInfo(controller);

            var enabled = (controller.DebugName?.Contains(this.searchText) ?? false) ||
                          info.FallbackName.Contains(this.searchText, StringComparison.OrdinalIgnoreCase);

            using (new EditorGUI.DisabledGroupScope(!enabled)) {
                var rect = GUILayoutUtility.GetRect(0, 99999, 20, 24);
                var pinButtonRect = new Rect(rect) {
                    xMax = rect.xMax - 5,
                    xMin = rect.xMax - 25,
                };
                var statusRect = new Rect(rect) {
                    xMax = pinButtonRect.xMin,
                    xMin = pinButtonRect.xMin - 60,
                };
                var iconRect = new Rect(rect) {
                    xMin  = rect.xMin + indentLevel * 10 + 10,
                    width = rect.height,
                };
                var labelRect = new Rect(rect) {
                    xMin = iconRect.xMax,
                    xMax = statusRect.xMin - 4,
                };

                var isSelected = this.selectedController == controller;
                if (isSelected) {
                    GUI.Box(rect, "", EditorStyles.helpBox);
                }

                var bgColor = this.activeFlowController == controller ? new Color(1f, 0.7f, 0f, 0.3f)
                    : this.activeFlowControllersSet.Contains(controller) ? new Color(1f, 0.7f, 0f, 0.1f)
                    : Color.clear;

                GUIHelper.PushColor(bgColor);
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                GUIHelper.PopColor();

                var icon = this.activeFlowController == controller ? SdfIconType.CaretRightSquareFill
                    : this.activeFlowControllersSet.Contains(controller) ? SdfIconType.CaretRightSquare
                    : SdfIconType.App;

                var labelStyle = isSelected ? EditorStyles.boldLabel : EditorStyles.label;

                GUIHelper.PushColor(this.activeFlowControllersSet.Contains(controller) ? Color.white : Color.black);
                SdfIcons.DrawIcon(iconRect.Expand(-4), icon);
                GUIHelper.PopColor();

                GUIHelper.PushColor(isSelected ? new Color(0f, 0.8f, 0.9f) : Color.white);

                if (GUI.Button(labelRect, controller.DebugName ?? this.GetControllerInfo(controller).FallbackName, labelStyle)) {
                    this.SelectControllerNextFrame(controller);
                }

                GUIHelper.PopColor();

                (Color color, string status) statusArgs = controller.Status switch {
                    ControllerStatus.Created => (Color.gray, "CREATED"),
                    ControllerStatus.Activating => (Color.yellow, "ACTIVATING"),
                    ControllerStatus.Running => (new Color(0.8f, 1f, 0.6f, 0.8f), "RUNNING"),
                    ControllerStatus.RunForResult => (new Color(0f, 0.9f, 1f, 0.8f), "FOR RESULT"),
                    ControllerStatus.Disposing => (new Color(1f, 0.4f, 0.4f), "DISPOSING"),
                    ControllerStatus.Disposed => (Color.red, "DISPOSED"),
                    _ => (Color.white, controller.Status.ToString()),
                };

                GUI.Box(statusRect, "", EditorStyles.helpBox);
                GUIHelper.PushColor(statusArgs.color);
                GUI.Label(statusRect, statusArgs.status, Styles.CenteredBoldMiniLabel);
                GUIHelper.PopColor();

                //EditorGUI.HelpBox(statusRect, controller?.Status.ToString() ?? "", MessageType.None);

                if (this.pinnedController != controller) {
                    if (SirenixEditorGUI.SDFIconButton(pinButtonRect, SdfIconType.ArrowRightSquare, style: EditorStyles.label)) {
                        this.PinControllerNextFrame(controller);
                        this.SelectControllerNextFrame(controller);
                    }
                }
                else if (controller != ControllersShared.RootController) {
                    GUIHelper.PushColor(Color.red);
                    if (SirenixEditorGUI.SDFIconButton(pinButtonRect, SdfIconType.ArrowLeftSquare, style: EditorStyles.label)) {
                        this.PinControllerNextFrame(null);
                    }

                    GUIHelper.PopColor();
                }
            }

            this.RestoreControllersIfNeeded(controller);

            foreach (var child in controller.Children) {
                this.DrawControllerMenu(child, indentLevel + 1);
            }
        }

        private ControllerInfo GetControllerInfo(ControllerBase controller) {
            if (!this.controllerInfo.TryGetValue(controller, out var info)) {
                this.controllerInfo[controller] = info = new ControllerInfo {
                    FallbackName = controller.DebugName ?? ObjectNames.NicifyVariableName(controller.GetType().Name),
                    FullName     = controller.GetType().FullName,
                };
            }

            return info;
        }

        private void RestoreControllersIfNeeded(ControllerBase controller) {
            var fullName = this.GetControllerInfo(controller).FullName;

            if (this.selectedController == null && this.lastSelectedControllerFullName == fullName) {
                this.SelectControllerNextFrame(controller);
            }

            if (this.pinnedController == null && this.lastPinnedControllerFullName == fullName) {
                this.PinControllerNextFrame(controller);
            }
        }

        private void PinControllerNextFrame([CanBeNull] ControllerBase controller) {
            EditorApplication.delayCall += PinController;

            void PinController() {
                this.pinnedController             = controller;
                this.lastPinnedControllerFullName = controller != null ? this.GetControllerInfo(controller).FullName : null;

                this.Repaint();
            }
        }

        private void SelectControllerNextFrame([CanBeNull] ControllerBase controller) {
            EditorApplication.delayCall += SelectController;

            void SelectController() {
                this.selectedController             = controller;
                this.lastSelectedControllerFullName = controller != null ? this.GetControllerInfo(controller).FullName : null;

                this.Repaint();
            }
        }

        private void OnControllerChildrenChanged(ControllerBase obj) {
            this.Repaint();
        }

        private void OnControllerStatusChanged(ControllerBase obj) {
            this.Repaint();
        }

        private void OnActiveControllerChanged(ControllerBase obj) {
            this.Repaint();
        }

        private struct ControllerInfo {
            public string FallbackName;
            public string FullName;
        }

        private static class Styles {
            public static readonly GUIStyle CenteredBoldMiniLabel;

            static Styles() {
                CenteredBoldMiniLabel = new GUIStyle(EditorStyles.miniBoldLabel) {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize  = 9,
                    normal = {
                        textColor = Color.white,
                    },
                };
            }
        }
    }
}