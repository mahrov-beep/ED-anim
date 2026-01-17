namespace Multicast {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
#if UNITY_EDITOR
    using Sirenix.Utilities;
#endif
    using UnityEngine;

    public abstract partial class AutoView<TState> {
#if UNITY_EDITOR
        [PropertyOrder(50)]
        [InfoBox("Notice for programmers: prefab contains designer variables", InfoMessageType.Warning, visibleIfMemberName: nameof(ShouldWarnForDesignerVariables))]
        [Title("Designers", "Temporary data that must be used only during development", TitleAlignment = TitleAlignments.Centered, HorizontalLine = true, Bold = true)]
        [TypeFilter(nameof(GetDesignerViewVariableTypes))]
        [OnValueChanged(nameof(FixDesignerViewEntries), InvokeOnInitialize = true, InvokeOnUndoRedo = true)]
        [OnValueChanged(nameof(ApplyVariablesNextFrame), includeChildren: true)]
        [TableList(AlwaysExpanded = true, ShowPaging = false)]
#endif
        [SerializeReference] private List<ViewVariable> designerVariables;

#if UNITY_EDITOR
        [PropertyOrder(51)]
        [TypeFilter(nameof(GetDesignerViewEventTypes))]
        [OnValueChanged(nameof(FixDesignerViewEntries), InvokeOnInitialize = true, InvokeOnUndoRedo = true)]
        [OnValueChanged(nameof(ApplyVariablesNextFrame), includeChildren: true)]
        [TableList(AlwaysExpanded = true, ShowPaging = false)]
#endif
        [SerializeReference] private List<ViewEvent> designerEvents;

#if UNITY_EDITOR
        private bool ShouldWarnForDesignerVariables() {
            return this.designerVariables.Count > 0 || this.designerEvents.Count > 0;
        }

        private void FixDesignerViewEntries() {
            this.designerVariables ??= new List<ViewVariable>();
            this.designerEvents    ??= new List<ViewEvent>();
            
            foreach (var designerVariable in this.designerVariables) {
                designerVariable.SetContext(this);
            }

            foreach (var designerEvent in this.designerEvents) {
                designerEvent.SetContext(this);
            }
        }

        private IEnumerable<Type> GetDesignerViewVariableTypes() => GetDesignerViewEntryTypes<ViewVariable>();
        private IEnumerable<Type> GetDesignerViewEventTypes()    => GetDesignerViewEntryTypes<ViewEvent>();

        private static IEnumerable<Type> GetDesignerViewEntryTypes<TEntry>() where TEntry : ViewEntry {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => typeof(TEntry).IsAssignableFrom(type) && !type.IsAbstract)
                .Where(type => type.GetCustomAttribute<ExposedViewEntryAttribute>() != null)
                .OrderBy(type => type.Name);
        }
#endif
    }
}