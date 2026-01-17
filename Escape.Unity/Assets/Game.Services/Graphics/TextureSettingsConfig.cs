namespace Game.Services.Graphics {
    using System;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Graphics/Texture Settings Config", fileName = "TextureSettingsConfig")]
    public class TextureSettingsConfig : ScriptableObject {
        public TextureTierSettings Low    = new();
        public TextureTierSettings Medium = new();
        public TextureTierSettings High   = new();

        public TextureTierSettings Get(GraphicsTextureQuality quality) {
            return quality switch {
                GraphicsTextureQuality.Low    => this.Low,
                GraphicsTextureQuality.Medium => this.Medium,
                GraphicsTextureQuality.High   => this.High,
                _                             => this.Medium,
            };
        }
    }

    [Serializable]
    public class TextureTierSettings {
        [Tooltip("globalTextureMipmapLimit: 0=full res, 1=half, 2=quarter")]
        public int mipmapLimit = 1;

        [Tooltip("Max anisotropic level (0 disables anisotropic filtering globally).")]
        public int anisotropicLimit = 0;

        [Tooltip("Enable Unity texture streaming.")]
        public bool textureStreaming = false;

        [Tooltip("Max streaming reduction (higher = lower resolution is allowed). Unity default is 2.")]
        public int streamingMaxLevelReduction = 2;

        [Tooltip("Streaming memory budget in MB.")]
        public int streamingMemoryBudgetMB = 256;
    }
}
