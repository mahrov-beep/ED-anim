namespace Multicast.ScriptingDefinesManagement {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEngine;

    public class ScriptingDefinesWindow : OdinEditorWindow, IHasCustomMenu {
        private static readonly DefineTargetInfo[] TargetInfos = {
            new DefineTargetInfo(DefineTargets.standalone, BuildTargetGroup.Standalone),
            new DefineTargetInfo(DefineTargets.android, BuildTargetGroup.Android),
            new DefineTargetInfo(DefineTargets.ios, BuildTargetGroup.iOS),
        };

        [MenuItem("Tools/Scripting Defines")]
        public static void Open() {
            var window = GetWindow<ScriptingDefinesWindow>();
            window.titleContent = new GUIContent("Defines");
            window.Show();
        }

        [SerializeField]
        [ListDrawerSettings(
            ShowFoldout      = false,
            ShowPaging       = false,
            DraggableItems   = false,
            HideAddButton    = true,
            HideRemoveButton = true)]
        // ReSharper disable once NotAccessedField.Local
        private List<DefineEntry> defines = new List<DefineEntry>();

        protected override void OnEnable() {
            base.OnEnable();

            this.ReloadDefinesList();
        }

        protected override void OnImGUI() {
            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling)) {
                base.OnImGUI();
            }
        }


        [Button]
        [PropertySpace(20)]
        private void AddDefine(string define) {
            SetDefines(define, TargetInfos.Aggregate((DefineTargets) 0, (current, info) => current | info.Target));
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Reload Defines"), false, CompilationPipeline.RequestScriptCompilation);
        }

        private void ReloadDefinesList() {
            this.defines = Enumerable
                .Empty<string>()
                .Union(TargetInfos.SelectMany(info => info.PlayerDefines))
                .Union(GetSuggestedDefines())
                .OrderBy(define => define)
                .Select(define => new DefineEntry {
                    define = define,
                    targets = TargetInfos
                        .Where(info => info.PlayerDefines.Contains(define))
                        .Aggregate((DefineTargets) 0, (current, info) => current | info.Target),
                })
                .ToList();
        }

        private static void SetDefines(string define, DefineTargets targets) {
            try {
                EditorApplication.LockReloadAssemblies();

                foreach (var info in TargetInfos) {
                    info.SetPlayerDefine(define, targets.HasFlag(info.Target));
                }
            }
            finally {
                EditorApplication.UnlockReloadAssemblies();
                CompilationPipeline.RequestScriptCompilation();
            }
        }

        private class DefineTargetInfo {
            private string[] playerDefinesCached;

            public DefineTargetInfo(DefineTargets target, BuildTargetGroup targetGroup) {
                this.Target      = target;
                this.TargetGroup = targetGroup;
            }

            public DefineTargets    Target      { get; }
            public BuildTargetGroup TargetGroup { get; }

            public string[] PlayerDefines {
                get {
                    if (this.playerDefinesCached == null) {
                        this.playerDefinesCached = PlayerSettings
                            .GetScriptingDefineSymbolsForGroup(this.TargetGroup)
                            .Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
                    }

                    return this.playerDefinesCached;
                }
            }

            public void SetPlayerDefine(string define, bool enabled) {
                var list = this.PlayerDefines.ToList();

                if (enabled) {
                    list.Add(define);
                }
                else {
                    list.Remove(define);
                }

                var definesString = list
                    .Distinct()
                    .OrderBy(it => it)
                    .Aggregate((a, b) => a + ";" + b);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(this.TargetGroup, definesString);
            }
        }

        private static List<string> GetSuggestedDefines() {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(asm => asm.GetCustomAttributes(typeof(ScriptingDefineSuggestionAttribute), false))
                .Select(attr => attr is ScriptingDefineSuggestionAttribute suggestion ? suggestion.Define : null)
                .Where(define => define != null)
                .ToList();
        }

        [Serializable]
        private class DefineEntry {
            [HideInInspector]
            public string define;

            [LabelText("@" + nameof(define))]
            [EnumToggleButtons]
            [OnValueChanged(nameof(OnTargetsChanged))]
            [InlineButton(nameof(SetAll), "+", ShowIf = nameof(CanSetAll))]
            public DefineTargets targets;

            private void OnTargetsChanged() {
                SetDefines(this.define, this.targets);
            }

            private bool CanSetAll() {
                return this.targets != DefineTargetsUtil.All;
            }

            private void SetAll() {
                SetDefines(this.define, DefineTargetsUtil.All);
            }
        }

        // ReSharper disable InconsistentNaming
        [Flags]
        private enum DefineTargets {
            standalone = 1 << 0,
            android    = 1 << 1,
            ios        = 1 << 2,
        }
        // ReSharper restore InconsistentNaming

        private static class DefineTargetsUtil {
            public static readonly DefineTargets All = DefineTargets.standalone | DefineTargets.android | DefineTargets.ios;
        }
    }
}