namespace Multicast {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Numerics;
    using UnityEngine.Pool;

    public static class RandomUtils {
        [PublicAPI]
        [MustUseReturnValue]
        public static bool Roll(BigDouble chance) {
            return chance > UnityEngine.Random.value;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static bool Roll(float chance) {
            return chance > UnityEngine.Random.value;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public static T GetRandomElementWithIntervals<T>(IList<T> array, Func<T, float> weightGetter) {
            float sum = 0;

            using (ListPool<float>.Get(out var list)) {
                list.Add(0.0f);

                foreach (var elem in array) {
                    sum += weightGetter(elem);
                    list.Add(sum);
                }

                var randomValue = UnityEngine.Random.Range(0.0f, sum);

                for (var i = 0; i < array.Count; i++) {
                    if (randomValue >= list[i] && randomValue <= list[i + 1]) {
                        return array[i];
                    }
                }

                return array[0];
            }
        }
    }
}