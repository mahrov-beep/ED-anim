namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MultiConditionAdapter_Float {
        [Serializable]
        public abstract class FloatCompareCondition : MultiConditionAdapter.Condition {
            [SerializeField, HideLabel, HorizontalGroup]
            protected ViewVariableFloat right;

            [SerializeField, HideLabel, HorizontalGroup(140)]
            protected Comparer comparer;

            public override string Prefix => "Float";
        }

        [Serializable]
        public sealed class Dynamic : FloatCompareCondition {
            [SerializeField, HideLabel, HorizontalGroup]
            private ViewVariableFloat left;

            public override bool IsTrue() {
                return Compare(this.right.Value, this.left.Value, this.comparer);
            }
        }

        [Serializable]
        public sealed class Constant : FloatCompareCondition {
            [SerializeField, HideLabel, HorizontalGroup]
            private float left;

            public override bool IsTrue() {
                return Compare(this.right.Value, this.left, this.comparer);
            }
        }

        private static bool Compare(float a, float b, Comparer c) {
            return c switch {
                Comparer.Equal => Mathf.Approximately(a, b),
                Comparer.NotEqual => !Mathf.Approximately(a, b),
                Comparer.GreaterOrEqual => a >= b,
                Comparer.LessOrEqual => a <= b,
                _ => false,
            };
        }

        public enum Comparer {
            Equal,
            NotEqual,
            GreaterOrEqual,
            LessOrEqual,
        }
    }
}