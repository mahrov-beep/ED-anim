namespace Game.Services.Graphics {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    [CreateAssetMenu(menuName = "Graphics/Lightmap Preset Config", fileName = "LightmapPresetConfig")]
    public class LightmapPresetConfig : ScriptableObject {
        public List<Entry> presets = new();

        [Serializable]
        public class Entry {
            public string key;
            public AssetReferenceT<LightmapPresetAsset> asset;
        }
    }
}
