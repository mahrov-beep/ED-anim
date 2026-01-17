namespace Multicast {
    using System;
    using Numerics;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MultiConditionAdapter_BigDouble {
        [Serializable]
        public abstract class BigDoubleCompareCondition : MultiConditionAdapter.Condition {
            [SerializeField, HideLabel, HorizontalGroup]
            protected ViewVariableBigDouble right;

            [SerializeField, HideLabel, HorizontalGroup(140)]
            protected Comparer comparer;

            public override string Prefix => "Big Double";
        }

        [Serializable]
        public sealed class Dynamic : BigDoubleCompareCondition {
            [SerializeField, HideLabel, HorizontalGroup]
            private ViewVariableBigDouble left;

            public override bool IsTrue() {
                return Compare(this.right.Value, this.left.Value, this.comparer);
            }
        }

        [Serializable]
        public sealed class Constant : BigDoubleCompareCondition {
            [SerializeField, HideLabel, HorizontalGroup]
            private BigDouble left;

            public override bool IsTrue() {
                return Compare(this.right.Value, this.left, this.comparer);
            }
        }

        private static bool Compare(BigDouble a, BigDouble b, Comparer c) {
            return c switch {
                Comparer.Equal => a == b,
                Comparer.NotEqual => a != b,
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