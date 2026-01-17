namespace Multicast {
    using JetBrains.Annotations;
    using UnityEngine;

    public class PrefabCache {
        private readonly ICache<GameObject> source;

        public PrefabCache(ICache<GameObject> source) {
            this.source = source;
        }

        [PublicAPI]
        [MustUseReturnValue]
        public GameObject Get(string path) {
            return this.source.Get(path);
        }
    }
}