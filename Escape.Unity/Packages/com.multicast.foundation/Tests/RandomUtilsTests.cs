namespace Multicast {
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    public class RandomUtilsTests {
        private static List<List<WeightedItem>> TestCases => new() {
            new List<WeightedItem> {
                new(weight: 0.0f),
                new(weight: 0.0f),
                new(weight: 0.1f, winner: true),
            },
            new List<WeightedItem> {
                new(weight: 0.0f),
                new(weight: 1.0f, winner: true),
                new(weight: 0.0f),
            },
            new List<WeightedItem> {
                new(weight: 5.0f, winner: true),
                new(weight: 0.0f),
                new(weight: 0.0f),
            },
            new List<WeightedItem> {
                new(weight: 0.0f, winner: true),
                new(weight: 0.0f),
                new(weight: 0.0f),
            },
        };

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void GetRandomElementWithIntervals(List<WeightedItem> items) {
            var actual = RandomUtils.GetRandomElementWithIntervals(items, it => it.weight);

            Assert.IsTrue(actual.winner);
        }

        [Serializable]
        public class WeightedItem {
            public float weight;
            public bool  winner;

            public WeightedItem(float weight, bool winner = false) =>
                (this.weight, this.winner) = (weight, winner);
        }
    }
}