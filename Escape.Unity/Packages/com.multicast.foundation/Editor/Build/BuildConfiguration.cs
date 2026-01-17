namespace Multicast.Build {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Pool;

    public class BuildConfiguration : ScriptableObject {
        private const string BUILD_GROUP     = "Build";
        private const string BUILD_GROUP_HOR = "Build/Hor";
        private const string INSPECTOR_GROUP = "Inspector";
        private const string SETTINGS_GROUP  = "Settings";

        private BuildTarget ActiveBuildTarget => EditorUserBuildSettings.activeBuildTarget;

        private string BuildGroupName => $"Build (Active target = {this.ActiveBuildTarget})";

        [SerializeField, PropertyOrder(-50)]
        [BoxGroup(BUILD_GROUP, GroupName = "$" + nameof(BuildGroupName))]
        [HorizontalGroup(BUILD_GROUP_HOR, MarginLeft = 5)]
        [HideIf(nameof(isTemplate))]
        [LabelText("Options"), LabelWidth(50)]
        private BuildOptions options;

        [ShowInInspector, PropertyOrder(-45)]
        [BoxGroup(BUILD_GROUP)]
        [HorizontalGroup(BUILD_GROUP_HOR, 85, MarginLeft = 5)]
        [HideIf(nameof(isTemplate))]
        [LabelText("Version"), LabelWidth(45)]
        public int LocalBuildCode { get; set; } = 1;

        [ShowInInspector, PropertyOrder(-40)]
        [BoxGroup(BUILD_GROUP)]
        [HorizontalGroup(BUILD_GROUP_HOR, 40, MarginLeft = 5)]
        [ShowIf("@isTemplate == false && ActiveBuildTarget == BuildTarget.Android")]
        [LabelText("Aab"), LabelWidth(25)]
        private bool LocalBuildAndroidAab {
            get => EditorUserBuildSettings.buildAppBundle;
            set => EditorUserBuildSettings.buildAppBundle = value;
        }

        public bool IsTemplate => this.isTemplate;

        [Button(ButtonSizes.Medium)]
        [BoxGroup(BUILD_GROUP)]
        [HorizontalGroup(BUILD_GROUP_HOR, 60, MarginLeft = 5)]
        [HideIf(nameof(isTemplate))]
        private void Build() {
            BuildScript.BuildDefaultPlayer(this, this.options);
        }

        [Button(ButtonSizes.Medium)]
        [BoxGroup(BUILD_GROUP)]
        [HorizontalGroup(BUILD_GROUP_HOR, 100)]
        [HideIf(nameof(isTemplate))]
        private void BuildAndRun() {
            BuildScript.BuildDefaultPlayer(this, this.options | BuildOptions.AutoRunPlayer);
        }

        [ShowInInspector, PropertyOrder(-9)]
        [TabGroup(INSPECTOR_GROUP)]
        [LabelText("Define Symbols"), Space]
        [GUIColor(1.0f, 0.95f, 1.0f)]
        [ListDrawerSettings(
            ShowFoldout = false, DraggableItems = false, ShowPaging = false,
            HideAddButton = true, HideRemoveButton = true)]
        [DisableContextMenu(true, true)]
        private List<InspectorScriptingDefineSymbol> inspectorDefinesSymbols;

        [ShowInInspector]
        [TabGroup(INSPECTOR_GROUP)]
        [LabelText("Pre Build Steps"), Space]
        [GUIColor(1.0f, 0.95f, 0.95f)]
        [ListDrawerSettings(
            ShowFoldout = false, DraggableItems = false, ShowPaging = false,
            HideAddButton = true, HideRemoveButton = true)]
        [DisableContextMenu(true, true)]
        private List<InspectorStep> inspectorPreBuildSteps = new();

        [ShowInInspector]
        [TabGroup(INSPECTOR_GROUP)]
        [LabelText("Post Build Steps"), Space]
        [GUIColor(0.95f, 0.95f, 1.0f)]
        [ListDrawerSettings(
            ShowFoldout = false, DraggableItems = false, ShowPaging = false,
            HideAddButton = true, HideRemoveButton = true)]
        [DisableContextMenu(true, true)]
        private List<InspectorStep> inspectorPostBuildSteps = new();

        [SerializeField]
        [TabGroup(SETTINGS_GROUP)]
        [OnValueChanged(nameof(RebuildInspector), includeChildren: true)]
        private bool isTemplate;

        [SerializeField]
        [TabGroup(SETTINGS_GROUP)]
        [OnValueChanged(nameof(RebuildInspector), includeChildren: true)]
        private BuildConfiguration template;

        [SerializeField]
        [TabGroup(SETTINGS_GROUP)]
        [OnValueChanged(nameof(RebuildInspector), includeChildren: true)]
        private List<BuildScriptingDefineSymbol> scriptingDefineSymbols;

        [SerializeReference]
        [TabGroup(SETTINGS_GROUP)]
        [InlineProperty]
        [OnValueChanged(nameof(RebuildInspector), includeChildren: true)]
        private List<PreBuildStep> preBuildSteps = new();

        [SerializeReference]
        [TabGroup(SETTINGS_GROUP)]
        [InlineProperty]
        [OnValueChanged(nameof(RebuildInspector), includeChildren: true)]
        private List<PostBuildStep> postBuildSteps = new();

        public List<PreBuildStep> EnumeratePreBuildSteps() {
            var steps = new List<PreBuildStep>();

            foreach (var it in PopulateConfigurations(this)) {
                PopulateSteps(steps, it.preBuildSteps);
            }

            return OrderSteps(steps);
        }

        public List<PostBuildStep> EnumeratePostBuildSteps() {
            var steps = new List<PostBuildStep>();

            foreach (var it in PopulateConfigurations(this)) {
                PopulateSteps(steps, it.postBuildSteps);
            }

            return OrderSteps(steps);
        }

        public List<BuildScriptingDefineSymbol> EnumerateDefineSymbols() {
            var symbols = this.template != null ? this.template.EnumerateDefineSymbols() : new List<BuildScriptingDefineSymbol>();

            foreach (var item in this.scriptingDefineSymbols) {
                if (symbols.FindIndex(it => it.symbol == item.symbol) is var index && index != -1) {
                    symbols[index] = item;
                }
                else {
                    symbols.Add(item);
                }
            }

            return symbols;
        }

        [OnInspectorInit]
        private void OnInspectorInit() {
            this.RebuildInspector();
        }

        private void RebuildInspector() {
            this.inspectorPreBuildSteps  = GenerateInspector(this.EnumeratePreBuildSteps());
            this.inspectorPostBuildSteps = GenerateInspector(this.EnumeratePostBuildSteps());

            this.inspectorDefinesSymbols = this.EnumerateDefineSymbols().Select(it => new InspectorScriptingDefineSymbol {
                Symbol  = it.symbol,
                Enabled = it.enabled,
            }).ToList();
        }

        private static List<BuildConfiguration> PopulateConfigurations(BuildConfiguration root) {
            var configurations = new List<BuildConfiguration>();

            for (var source = root; source != null; source = source.template) {
                configurations.Add(source);
            }

            configurations.Reverse();

            return configurations;
        }

        private static void PopulateSteps<TBuildStep>(List<TBuildStep> list, List<TBuildStep> from)
            where TBuildStep : BuildStep {
            foreach (var step in from) {
                if (step.StepMode == BuildStepMode.Replace &&
                    list.FindIndex(it => it.GetType() == step.GetType()) is var index && index != -1) {
                    list[index] = step;
                }
                else {
                    list.Add(step);
                }
            }
        }

        private static List<TBuildStep> OrderSteps<TBuildStep>(List<TBuildStep> list)
            where TBuildStep : BuildStep {
            return list
                .Select((it, index) => (index, it))
                .OrderBy(p => p.it.StepOrder.GetValueOrDefault(p.index))
                .Select(it => it.it)
                .ToList();
        }

        private static List<InspectorStep> GenerateInspector<TBuildStep>(List<TBuildStep> steps)
            where TBuildStep : BuildStep {
            return steps.Where(it => it.IsBuildStepEnabled).Select((it, ind) => new InspectorStep {
                    Name      = $"{ind}: {it.Name}",
                    Inspector = it.GetInspector() ?? new NoInspector(),
                })
                .ToList();
        }

        private struct InspectorScriptingDefineSymbol {
            [ShowInInspector]
            [LabelWidth(10), LabelText("$" + nameof(SymbolLabelText)), DisplayAsString, EnableGUI]
            public string Symbol;

            [NonSerialized]
            public bool Enabled;

            private string SymbolLabelText => this.Enabled ? "+" : "-";
        }

        private struct InspectorStep {
            [NonSerialized]
            public string Name;

            [ShowInInspector]
            [BoxGroup("$" + nameof(Name))]
            [InlineProperty, HideLabel, HideReferenceObjectPicker, Indent(2)]
            public object Inspector;
        }

        private class NoInspector {
        }
    }
}