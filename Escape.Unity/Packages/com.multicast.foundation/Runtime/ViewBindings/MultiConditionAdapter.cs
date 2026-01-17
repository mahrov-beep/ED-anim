namespace Multicast {
    using System;
    using System.Linq;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [AddComponentMenu("View Binding/Adapters/[Binding] Multi Condition Adapter")]
    public class MultiConditionAdapter : SingleResultAdapterBase<bool, ViewVariableBool> {
        [SerializeField, EnumToggleButtons]
        private MultiConditionMode mode = MultiConditionMode.All;

        [SerializeReference]
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
        private Condition[] conditions = Array.Empty<Condition>();

        [Serializable, HideReferenceObjectPicker]
        public abstract class Condition {
            [ShowInInspector, PropertyOrder(-10)]
            [HideLabel, DisplayAsString, HorizontalGroup(100)]
            public abstract string Prefix { get; }

            public abstract bool IsTrue();
        }

        private enum MultiConditionMode {
            All,
            Any,
        }

        protected override bool Adapt() {
            return this.mode switch {
                MultiConditionMode.All => this.conditions.All(it => it.IsTrue()),
                MultiConditionMode.Any => this.conditions.Any(it => it.IsTrue()),
                _ => false,
            };
        }
    }
}