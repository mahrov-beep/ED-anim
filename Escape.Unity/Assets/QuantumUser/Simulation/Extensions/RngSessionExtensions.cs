namespace Quantum {
  using System;
  using System.Collections.Generic;
  using JetBrains.Annotations;
  using Photon.Deterministic;
  using UnityEngine.Pool;

  public static class RngSessionExtensions {
    [PublicAPI]
    [MustUseReturnValue]
    public static bool Roll01(this ref RNGSession session, FP probability01) {
      return probability01 > session.Next(FP._0, FP._1);
    }

    [PublicAPI]
    [MustUseReturnValue]
    public static T GetRandomElementWithIntervals<T>(this ref RNGSession session, List<T> array, Func<T, FP> weightGetter) {
      return array[session.GetRandomElementIndexWithIntervals(array, weightGetter)];
    }

    [PublicAPI]
    [MustUseReturnValue]
    public static T GetRandomElementWithIntervals<T>(this ref RNGSession session, T[] array, Func<T, FP> weightGetter) {
      return array[session.GetRandomElementIndexWithIntervals(array, weightGetter)];
    }

    [PublicAPI]
    [MustUseReturnValue]
    public static int GetRandomElementIndexWithIntervals<T>(this ref RNGSession session, List<T> array, Func<T, FP> weightGetter) {
      if (array == null || array.Count == 0) {
        throw new ArgumentException("array of elements must not empty");
      }

      FP sum = FP._0;

      using (ListPool<FP>.Get(out var list)) {
        list.Add(FP._0);

        foreach (var elem in array) {
          sum += weightGetter(elem);
          list.Add(sum);
        }

        var randomValue = session.Next(FP._0, sum);

        for (var i = 0; i < array.Count; i++) {
          if (randomValue >= list[i] && randomValue <= list[i + 1]) {
            return i;
          }
        }

        return 0;
      }
    }
    
    [PublicAPI]
    [MustUseReturnValue]
    public static int GetRandomElementIndexWithIntervals<T>(this ref RNGSession session, T[] array, Func<T, FP> weightGetter) {
      if (array == null || array.Length == 0) {
        throw new ArgumentException("array of elements must not empty");
      }

      FP sum = FP._0;

      using (ListPool<FP>.Get(out var list)) {
        list.Add(FP._0);

        foreach (var elem in array) {
          sum += weightGetter(elem);
          list.Add(sum);
        }

        var randomValue = session.Next(FP._0, sum);

        for (var i = 0; i < array.Length; i++) {
          if (randomValue >= list[i] && randomValue <= list[i + 1]) {
            return i;
          }
        }

        return 0;
      }
    }
  }
}