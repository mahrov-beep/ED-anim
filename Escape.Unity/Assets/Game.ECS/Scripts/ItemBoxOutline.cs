namespace Game.ECS.Systems.GameInventory {
    using EPOOutline;
    using Game.ECS.Scripts;
    using Multicast;
    using Quantum;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Outlinable))]
    public class ItemBoxOutline : QuantumViewComponent<ItemBoxContext> {       

        [SerializeField] private Outlinable outlinable;
        [SerializeField] private ItemBoxOutlineConfig config;        

        private bool isOutlined;

        public float ActivationRadius {
            get {
                if (Config != null) {
                    return Config.activationRadius;
                }

                return 0;
            }
        }

        private ItemBoxOutlineConfig Config => config != null
            ? config
            : (config = TryGetConfig());

        private void Awake() {
            EnsureOutlinable();
            ApplyConfig();
            ApplyState(false, true);
        }

        public void SetOutline(bool shouldBeVisible) {
            ApplyState(shouldBeVisible, false);
        }

        private void ApplyState(bool state, bool force) {
            if (outlinable == null && !EnsureOutlinable()) {
                return;
            }

            if (!force && isOutlined == state) {
                return;
            }

            isOutlined = state;
            outlinable.enabled = state;
        }

        private void ApplyConfig() {
            if (Config == null || outlinable == null) {
                return;
            }

            var useFrontBack = Config.renderStyle == RenderStyle.FrontBack;

            outlinable.RenderStyle = useFrontBack ? RenderStyle.FrontBack : RenderStyle.Single;
            ApplyProperties(outlinable.OutlineParameters, !useFrontBack, Config.color);
            ApplyProperties(outlinable.FrontParameters, useFrontBack && Config.front.enabled, Config.front.color);
            ApplyProperties(outlinable.BackParameters, useFrontBack && Config.back.enabled, Config.back.color);
        }

        private ItemBoxOutlineConfig TryGetConfig() {
            if (config != null) {
                return config;
            }

            config = App.Get<ItemBoxOutlineConfig>(); 

            return config;
        }

        private void ApplyProperties(Outlinable.OutlineProperties properties, bool enabled, Color color) {
            if (properties == null || Config == null) {
                return;
            }

            properties.Enabled = enabled;
            properties.Color = color;
            properties.DilateShift = Config.dilateShift;
            properties.BlurShift = Config.blurShift;
        }

        private bool EnsureOutlinable() {
            if (outlinable != null) {
                return true;
            }

            outlinable = GetComponent<Outlinable>();
            if (outlinable == null) {
                outlinable = GetComponentInChildren<Outlinable>(true);
            }

            return outlinable != null;
        }
    }
}
