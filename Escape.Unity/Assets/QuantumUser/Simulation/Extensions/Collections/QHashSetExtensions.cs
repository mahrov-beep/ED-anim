using Quantum.Collections;
using System;

namespace Quantum {
  static unsafe partial class QHashSetExtensions {
    public static void RemoveAll<T, TState>(this QHashSet<T> qHashSet, TState state, Func<TState, T, bool> predicate)
            where T : unmanaged, IEquatable<T> {
      bool dirty;
      do {
        dirty = false;

        foreach (var it in qHashSet) {
          if (predicate(state, it)) {
            dirty = true;
            qHashSet.Remove(it);
            break;
          }
        }

      }
      while (dirty);
    }

    public static bool Contains<T>(this QHashSetPtr<T> hashSetPtr, Frame f, T item)
            where T : unmanaged, IEquatable<T> {
      var hashSet = f.ResolveHashSet(hashSetPtr);
      return hashSet.Contains(item);
    }

    public static bool Add<T>(this QHashSetPtr<T> hashSetPtr, Frame f, T item)
            where T : unmanaged, IEquatable<T> {
      var hashSet = f.ResolveHashSet(hashSetPtr);
      return hashSet.Add(item);
    }

    public static bool IsEmpty<T>(this QHashSetPtr<T> hashSetPtr, Frame f)
            where T : unmanaged, IEquatable<T> {
      var hashSet = f.ResolveHashSet(hashSetPtr);
      return hashSet.Count < 1;
    }
  }
}