namespace Game.ECS.Systems.Unit {
    using System.Collections.Generic;
    using JetBrains.Annotations;
    public interface IDictionaryProvider<TKey, TValue> where TKey : notnull {
        public Dictionary<TKey, TValue> Dictionary { get; }

        public bool TryGet([NotNull] TKey key, out TValue value) {
            return Dictionary.TryGetValue(key, out value);
        }
    }

    public static class DictionaryProviderExt {
        public static bool TryGet<TKey, TValue>(
                        this IDictionaryProvider<TKey, TValue> p,
                        TKey key,
                        out TValue value) => p.TryGet(key, out value);
    }
}