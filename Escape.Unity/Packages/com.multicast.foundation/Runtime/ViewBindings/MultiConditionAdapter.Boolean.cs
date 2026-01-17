namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MultiConditionAdapter_Boolean {
        [Serializable]
        public abstract class BoolCompareCondition : MultiConditionAdapter.Condition {
            [SerializeField, HideLabel, HorizontalGroup]
            protected ViewVariableBool right;

            public override string Prefix => "Boolean";
        }

        [Serializable]
        public sealed class Constant : BoolCompareCondition {
            [SerializeField, HideLabel, HorizontalGroup]
            private Comparer comparer;

            public override bool IsTrue() {
                var left = this.comparer == Comparer.IsTrue;

                return this.right.Value == left;
            }

            public enum Comparer {
                IsTrue,
                IsFalse,
            }
        }
    }
}