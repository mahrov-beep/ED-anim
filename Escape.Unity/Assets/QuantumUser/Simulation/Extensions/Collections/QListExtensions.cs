using System;
using System.Linq;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum {
  public static unsafe partial class QListExtensions {
    public static bool Contains<T>(this QList<T> qList, T item) where T : unmanaged {
      foreach (T t in qList)
        if (t.Equals(item))
          return true;

      return false;
    }

    public static bool Contains<T>(this QListPtr<T> qList, Frame f, T item) where T : unmanaged {
      return f.ResolveList(qList).Contains(item);
    }

    public static T Pop<T>(this QList<T> qList) where T : unmanaged {
      if (qList.Count == 0)
        return default;

      var lastIndex = qList.Count - 1;
      var value     = qList[lastIndex];

      qList.RemoveAt(lastIndex);

      return value;
    }

    public static T Pop<T>(this QListPtr<T> qListPtr, Frame f) where T : unmanaged {
      var list = f.ResolveList(qListPtr);
      return list.Pop();
    }

    public static T Random<T>(this QList<T> qList, Frame f, RNGSession* rng) where T : unmanaged {
      if (qList.Count == 0)
        return default;

      int randomIndex = rng->Next(0, qList.Count);
      return qList[randomIndex];
    }

    public static void RandomShuffle<T>(this QListPtr<T> qListPtr, Frame f, RNGSession* rng) where T : unmanaged {
      var list = f.ResolveList(qListPtr);

      for (int i = list.Count - 1; i > 0;  i--)
      {
        int j = rng->NextInclusive(0, i);
        (list[i], list[j]) = (list[j], list[i]);
      }
    }

    public static T Random<T>(this QListPtr<T> qList, Frame f, RNGSession* rng) where T : unmanaged {
      return f.ResolveList(qList).Random(f, rng);
    }

    public static T FirstOrDefault<T>(this QListPtr<T> ptr, Frame f) where T : unmanaged {
      return f.ResolveList(ptr).FirstOrDefault();
    }

    public static T First<T>(this QListPtr<T> ptr, Frame f) where T : unmanaged {
      var qList = f.ResolveList(ptr);
      return qList.First();
    }

    public static T* FirstPointer<T>(this QListPtr<T> ptr, Frame f) where T : unmanaged {
      var qList = f.ResolveList(ptr);
      return qList.GetPointer(0);
    }

    public static void CopyTo<T>(this QListPtr<T> fromPtr, Frame f, QListPtr<T> toPtr, bool clearToBeforeCopy = true) where T : unmanaged {
      var from = f.ResolveList(fromPtr);
      var to   = f.ResolveList(toPtr);

      if (clearToBeforeCopy)
        to.Clear();

      foreach (var unmanaged in from)
        to.Add(unmanaged);
    }

    public static int Count<T>(this QListPtr<T> qListPtr, Frame f) where T : unmanaged {
      return f.ResolveList(qListPtr).Count;
    }

    public static bool Any<T>(this QListPtr<T> qListPtr, Frame f) where T : unmanaged {
      if (qListPtr == default)
        return false;

      return f.ResolveList(qListPtr).Any();
    }

    public static bool Any<T>(this QListPtr<T> qListPtr, Frame f, Func<T, bool> predicate) where T : unmanaged {
      if (qListPtr == default)
        return false;

      return f.ResolveList(qListPtr).Any(predicate);
    }
  }
}