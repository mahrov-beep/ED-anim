namespace Game.Services.Graphics {
    using UnityEngine;

    [CreateAssetMenu(menuName = "Graphics/Graphics Settings Config", fileName = "GraphicsSettingsConfig")]
    public class GraphicsSettingsConfig : ScriptableObject {
        public ShadowSettingsConfig   Shadows;
        public LightingSettingsConfig Lighting;
        public TextureSettingsConfig  Textures;
    }
}
