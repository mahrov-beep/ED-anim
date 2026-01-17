namespace Multicast.Unity {
    using UnityEngine;

    public class AssetBundlesClearCache : MonoBehaviour {
        [SerializeField] private int addressablesCacheVersion;

        private static int AssetBundlesCacheVersion {
            get => PlayerPrefs.GetInt("AssetBundlesCacheVersion", 0);
            set => PlayerPrefs.SetInt("AssetBundlesCacheVersion", value);
        }

        protected virtual void Awake() {
            if (AssetBundlesCacheVersion != this.addressablesCacheVersion) {
                AssetBundlesCacheVersion = this.addressablesCacheVersion;

                Caching.ClearCache();
            }
        }
    }
}