namespace Game.Services.Graphics {
    using System;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Graphics/Lighting Settings Config", fileName = "LightingSettingsConfig")]
    public class LightingSettingsConfig : ScriptableObject {
        public LightingTierSettings Off = new();
        public LightingTierSettings On  = new();

        public LightingTierSettings Get(GraphicsLightingMode mode) {
            return mode switch {
                GraphicsLightingMode.Off => this.Off,
                GraphicsLightingMode.On  => this.On,
                _                        => this.On,
            };
        }
    }

    [Serializable]
    public class LightingTierSettings {
        [Header("Unity")]
        public int pixelLightCount = 2;

        [Header("URP")]
        [Tooltip("URP AdditionalLightsRenderingMode: 0=Per-vertex, 1=Per-pixel (disable via supportsAdditionalLights)")]
        public int  additionalLightsRenderingMode = 1;
        public bool supportsAdditionalLights      = true;
        public int  additionalLightsPerObjectLimit = 4;
        public bool supportsMixedLighting         = true;
    }
}
