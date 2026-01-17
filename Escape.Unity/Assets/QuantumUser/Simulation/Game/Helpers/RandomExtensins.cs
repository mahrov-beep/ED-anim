namespace Quantum {
using Collections;
using Photon.Deterministic;
public static class RandomExtensins {
  public static unsafe T Random<T>(this T[] array, RNGSession* rng) {
    Assert.Always(array is { Length: > 0 }, "array must be non-empty");
    var index = rng->Next(0, array.Length);
    return array[index];
  }

  public static unsafe T Random<T>(this QListPtr<T> ptr, Frame f, RNGSession* rng) where T : unmanaged {
    Assert.Always(ptr != default, "array pointer is not valid");
    var array = f.ResolveList(ptr);
    return array.Random(rng);
  }

  public static unsafe T Random<T>(this QList<T> array, RNGSession* rng) where T : unmanaged {
    Assert.Always(array.Count > 0, "array must be non-empty");
    var index = rng->Next(0, array.Count);
    return array[index];
  }

}
}