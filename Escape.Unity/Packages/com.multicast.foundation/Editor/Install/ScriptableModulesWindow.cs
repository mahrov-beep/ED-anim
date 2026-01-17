namespace Multicast.Install {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Boot;
    using Sirenix.OdinInspector;
    using Utilities;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public class ScriptableModulesWindow : OdinMenuEditorWindow {
        private List<ScriptableModule> modules;

        private static OdinMenuStyle enabledModuleStyle;
        private static OdinMenuStyle disabledModuleStyle;

        [NonSerialized] private ScriptableModule          selectedModule;
        [NonSerialized] private ScriptableModuleInstaller selectedModuleInstaller;

        [MenuItem("Window/MULTICAST GAMES/Modules")]
        private static void OpenWindow() {
            var window = GetWindow<ScriptableModulesWindow>();
            window.titleContent = new GUIContent("Modules");
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree() {
            var tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;

            var modulesCache = EditorAddressablesCache<ScriptableModule>.Instance;
            this.modules = EditorAddressablesUtils
                .EnumeratePathsByLabel(AppConstants.AddressableLabels.MODULES)
                .Select(it => modulesCache.Get(it))
                .ToList();

            foreach (var module in this.modules) {
                string path;

                if (module.GetType().GetAttribute<ScriptableModuleAttribute>() is var attr && attr != null) {
                    path = attr.Category + "/" + module.name;
                }
                else {
                    path = module.name;
                }

                if (module.GetType().GetCustomAttribute<ObsoleteAttribute>() is { } obsoleteAttribute) {
                    path += " :: [OBSOLETE]";
                }

                tree.AddObjectAtPath(path, module);
            }

            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle         = OdinMenuStyle.TreeViewStyle;
            tree.SortMenuItemsByName();

            UpdateStyles(tree);

            return tree;
        }

        private void RefreshSelectedModuleIfNeeded() {
            var selected = this.MenuTree?.Selection?.FirstOrDefault()?.Value as ScriptableModule;
            if (this.selectedModule == selected) {
                return;
            }

            this.selectedModule = selected;

            if (this.selectedModule == null) {
                this.selectedModuleInstaller = null;
                return;
            }

            this.selectedModuleInstaller = ScriptableModuleInstaller.Create(
                new[] {this.selectedModule},
                BootLoader.EditorPlatformOverride,
                new Progress<float>(), new ServicesContainer());

            this.selectedModuleInstaller.Setup();
        }

        private static void UpdateStyles(OdinMenuTree tree) {
            if (enabledModuleStyle == null) {
                enabledModuleStyle = tree.DefaultMenuStyle.Clone();
            }

            if (disabledModuleStyle == null) {
                disabledModuleStyle = tree.DefaultMenuStyle.Clone();
                disabledModuleStyle.DefaultLabelStyle = new GUIStyle(SirenixGUIStyles.Label) {
                    normal = {
                        textColor = Color.gray,
                    },
                    hover = {
                        textColor = Color.gray,
                    },
                };
            }

            var platform = BootLoader.EditorPlatformOverride;

            foreach (var item in tree.MenuItems) {
                if (item.Value is IScriptableModule module) {
                    item.Style = module.IsPlatformSupported(platform)
                        ? enabledModuleStyle
                        : disabledModuleStyle;
                }
            }
        }

        protected override void DrawMenu() {
            this.RefreshSelectedModuleIfNeeded();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Space(5);
                DrawPlatformDropdown(this.MenuTree);
                GUILayout.Space(5);
            }
            GUILayout.EndHorizontal();

            base.DrawMenu();

            static void DrawPlatformDropdown(OdinMenuTree tree) {
                var values   = ScriptableModule.AllPlatforms.ToArray();
                var index    = Array.IndexOf(values, BootLoader.EditorPlatformOverride);
                var newIndex = EditorGUILayout.Popup(index, values, EditorStyles.toolbarPopup);
                if (newIndex != index && newIndex != -1) {
                    BootLoader.EditorPlatformOverride = values[newIndex];
                    UpdateStyles(tree);
                }
            }
        }

        protected override void OnBeginDrawEditors() {
            if (this.MenuTree == null) {
                return;
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Space(5);

                if (GUILayout.Button("L", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) {
                    var overviewWindow = OdinEditorWindow.CreateOdinEditorWindowInstanceForObject(
                        new OverviewPage {
                            Modules = this.modules
                                .OrderBy(it => ScriptableModulePriority.GetPriority(it))
                                .Select(it => new ModuleOverview {Module = it})
                                .ToList(),
                        });

                    overviewWindow.titleContent = new GUIContent("Modules Overview");
                    overviewWindow.ShowAuxWindow();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add Module", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) {
                    this.ShowAddModuleDialog();
                }

                GUILayout.Space(5);
            }
            GUILayout.EndHorizontal();

            if (this.selectedModule is { } m && m != null &&
                m.GetType().GetAttribute<ObsoleteAttribute>() is { } obsoleteAttribute) {
                EditorGUILayout.HelpBox(obsoleteAttribute.Message, obsoleteAttribute.IsError ? MessageType.Error : MessageType.Warning);
            }
        }

        protected override void OnEndDrawEditors() {
            base.OnEndDrawEditors();

            GUILayout.FlexibleSpace();

            if (this.selectedModuleInstaller != null) {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Provides for selected platform");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(GUI.skin.box);

                var anyTypeProvided = false;

                foreach (var providedTypes in this.selectedModuleInstaller.moduleToProvidedType.Values) {
                    foreach (var providedType in providedTypes) {
                        anyTypeProvided = true;

                        if (typeof(Delegate).IsAssignableFrom(providedType)) {
                            continue;
                        }

                        GUILayout.Label(providedType.Name);
                    }
                }

                if (!anyTypeProvided) {
                    GUILayout.Label("Nothing", EditorStyles.miniLabel);
                }

                GUILayout.EndVertical();

                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (this.selectedModule != null) {
                    var rect = EditorGUILayout.GetControlRect();

                    var buttonRect = new Rect(rect) {
                        width = 40,
                    };

                    var labelRect = new Rect(rect) {
                        xMin = buttonRect.xMax,
                    };

                    if (GUI.Button(buttonRect, "Open", EditorStyles.toolbarButton)) {
                        var monoScript = MonoScript.FromScriptableObject(this.selectedModule);
                        AssetDatabase.OpenAsset(monoScript);
                    }

                    GUI.Label(labelRect, this.selectedModule.GetType().FullName);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void ShowAddModuleDialog() {
            var path = "Assets/";

            if (this.modules.Count > 0) {
                var module     = this.modules[0];
                var modulePath = AssetDatabase.GetAssetPath(module);
                path = Path.GetDirectoryName(modulePath);
            }

            void HandleModuleCreated(ScriptableObject obj) {
                this.TrySelectMenuItemWithObject(obj);
            }

            ScriptableObjectCreator.ShowDialog<ScriptableModule>(path, HandleModuleCreated);
        }

        [Serializable]
        private class OverviewPage {
            [ShowInInspector]
            [TableList(
                IsReadOnly     = true,
                ShowPaging     = false,
                AlwaysExpanded = true
            )]
            public List<ModuleOverview> Modules { get; set; }
        }

        [Serializable]
        private class ModuleOverview {
            public ScriptableModule Module { get; set; }

            [ShowInInspector, DisplayAsString]
            public string Namespace => this.Module.GetType().Namespace;

            [ShowInInspector, EnableGUI, DisplayAsString]
            public string Name => this.Module.name;

            [ShowInInspector, EnableGUI, DisplayAsString, TableColumnWidth(60, false)]
            public int Priority => ScriptableModulePriority.GetPriority(this.Module);
        }
    }
}