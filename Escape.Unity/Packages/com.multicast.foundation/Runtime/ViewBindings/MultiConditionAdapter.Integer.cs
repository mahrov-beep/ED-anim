namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MultiConditionAdapter_Integer {
        [Serializable]
        public abstract class IntegerCompareCondition : MultiConditionAdapter.Condition {
            [SerializeField, HideLabel, HorizontalGroup]
            protected ViewVariableInt right;

            [SerializeField, HideLabel, HorizontalGroup(140)]
            protected Comparer comparer;

            public override string Prefix => "Integer";
        }

        [Serializable]
        public sealed class Dynamic : IntegerCompareCondition {
            [SerializeField, HideLabel, HorizontalGroup]
            private ViewVariableInt left;

            public override bool IsTrue() {
                return Compare(this.right.Value, this.left.Value, this.comparer);
            }
        }

        [Serializable]
        public sealed class Constant : IntegerCompareCondition {
            [SerializeField, HideLabel, HorizontalGroup]
            private int left;

            public override bool IsTrue() {
                return Compare(this.right.Value, this.left, this.comparer);
            }
        }

        private static bool Compare(int a, int b, Comparer c) {
            return c switch {
                Comparer.Equal => a == b,
                Comparer.NotEqual => a != b,
                Comparer.GreaterOrEqual => a >= b,
                Comparer.LessOrEqual => a <= b,
                Comparer.Greater => a > b,
                Comparer.Less => a < b,
                _ => false,
            };
        }

        public enum Comparer {
            Equal,
            NotEqual,
            GreaterOrEqual,
            LessOrEqual,
            Greater,
            Less,
        }
    }
}