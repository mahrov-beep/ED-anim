using Quantum.Collections;

namespace Quantum {
public static class QDictionaryExtend {
  public static void SafeSetValue<T>(this ref QDictionary<EntityRef, T> map, EntityRef key, T value) where T : unmanaged {
    if (!map.ContainsKey(key)) {
      map.Add(key, value);
    }
    else {
      map[key] = value;
    }
  }
}
}
