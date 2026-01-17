namespace Game.Services.Graphics {
    using System;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Graphics/Shadow Settings Config", fileName = "ShadowSettingsConfig")]
    public class ShadowSettingsConfig : ScriptableObject {
        public ShadowTierSettings Off    = new();
        public ShadowTierSettings Low    = new();
        public ShadowTierSettings Medium = new();
        public ShadowTierSettings High   = new();

        public ShadowTierSettings Get(GraphicsShadowQuality quality) {
            return quality switch {
                GraphicsShadowQuality.Off    => this.Off,
                GraphicsShadowQuality.Low    => this.Low,
                GraphicsShadowQuality.Medium => this.Medium,
                GraphicsShadowQuality.High   => this.High,
                _                            => this.Off,
            };
        }
    }

    [Serializable]
    public class ShadowTierSettings {
        [Header("Realtime Shadows")]
        public bool mainLightShadows         = true;
        public bool additionalLightShadows   = true;
        public bool softShadows              = true;
        public float shadowDistance          = 40f;
        public int shadowCascadeCount        = 2;
        public int mainLightResolution       = 1024;
        public int additionalLightResolution = 1024;

        [Header("Baked Shadows")]
        public bool disableBakedShadows = false;
    }
}
