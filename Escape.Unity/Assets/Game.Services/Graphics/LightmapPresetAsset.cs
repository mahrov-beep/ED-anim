namespace Game.Services.Graphics {
    using UnityEngine;
    using UnityEngine.Rendering;

    [CreateAssetMenu(menuName = "Graphics/Lightmap Preset", fileName = "LightmapPreset")]
    public class LightmapPresetAsset : ScriptableObject {
        public LightmapsMode mode = LightmapsMode.NonDirectional;

        [Header("Textures (optional per entry)")]
        public Texture2D[] colorMaps;
        public Texture2D[] dirMaps;
        public Texture2D[] shadowMasks;
    }
}
