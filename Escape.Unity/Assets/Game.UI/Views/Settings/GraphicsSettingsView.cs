namespace Game.UI.Views.Settings {
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UnityEngine;
    using UnityEngine.UI;
    using Multicast;
    using UI.Widgets.Settings;

    public class GraphicsSettingsView : AutoView<IGraphicsSettingsState> {
        [SerializeField, Required] private RectTransform            presetsRoot;
        [SerializeField, Required] private GraphicsPresetButtonView presetButtonTemplate;
        [SerializeField]            private RectTransform            shadowsRoot;
        [SerializeField]            private GraphicsPresetButtonView shadowsButtonTemplate;
        [SerializeField]            private RectTransform            texturesRoot;
        [SerializeField]            private GraphicsPresetButtonView texturesButtonTemplate;
        [SerializeField]            private RectTransform            lightingRoot;
        [SerializeField]            private GraphicsPresetButtonView lightingButtonTemplate;
        [SerializeField, Required] private Button                   closeButton;
        [SerializeField]            private LayoutGroup             layoutGroup;
        [SerializeField]            private ScrollRect              scrollRect;

        [Header("Mouse Sensitivity (Standalone only)")]
        [SerializeField] private GameObject mouseSensitivityContainer;
        [SerializeField] private Slider     mouseSensitivitySlider;

        private readonly List<GraphicsPresetButtonView> presetButtons = new();
        private readonly List<GraphicsPresetButtonView> shadowButtons = new();
        private readonly List<GraphicsPresetButtonView> textureButtons = new();
        private readonly List<GraphicsPresetButtonView> lightingButtons = new();
        private int layoutRefreshFrames;
        private const int LayoutRefreshFrameCount = 2;

        protected override void Awake() {
            base.Awake();
            this.closeButton.onClick.AddListener(this.OnCloseClicked);
            if (this.mouseSensitivitySlider != null) {
                this.mouseSensitivitySlider.onValueChanged.AddListener(this.OnMouseSensitivityChanged);
            }
        }

        protected override void OnDestroy() {  
            base.OnDestroy();
            this.closeButton.onClick.RemoveListener(this.OnCloseClicked); 
            if (this.mouseSensitivitySlider != null) {
                this.mouseSensitivitySlider.onValueChanged.RemoveListener(this.OnMouseSensitivityChanged);
            }
        }

        private void OnMouseSensitivityChanged(float value) {
            this.State.SetMouseSensitivity(value);
        }

        protected override void Render() {
            base.Render();

            var presets = this.State.Presets;
            this.RenderGroup(presets, this.presetsRoot, this.presetButtonTemplate, this.presetButtons);

            this.RenderGroup(this.State.Shadows, this.shadowsRoot, this.shadowsButtonTemplate ?? this.presetButtonTemplate, this.shadowButtons);
            this.RenderGroup(this.State.Textures, this.texturesRoot, this.texturesButtonTemplate ?? this.presetButtonTemplate, this.textureButtons);
            this.RenderGroup(this.State.Lighting, this.lightingRoot, this.lightingButtonTemplate ?? this.presetButtonTemplate, this.lightingButtons);

            if (this.mouseSensitivityContainer != null) {
                var visible = this.State.IsMouseSensitivityVisible;
                this.mouseSensitivityContainer.SetActive(visible);

                if (visible && this.mouseSensitivitySlider != null) {
                    this.mouseSensitivitySlider.SetValueWithoutNotify(this.State.MouseSensitivity);
                    this.mouseSensitivitySlider.minValue = this.State.MouseSensitivityMin;
                    this.mouseSensitivitySlider.maxValue = this.State.MouseSensitivityMax;
                    this.mouseSensitivitySlider.SetValueWithoutNotify(this.State.MouseSensitivity);
                }
            }

            this.RefreshLayoutIfNeeded();
        }

        private void OnCloseClicked() {
            this.State.Close();
        }

        private void RenderGroup(
            List<IGraphicsOptionButtonState> options,
            RectTransform root,
            GraphicsPresetButtonView template,
            List<GraphicsPresetButtonView> buffer
        ) {
            if (root == null || template == null) {
                return;
            }

            var targetCount = options?.Count ?? 0;

            if (buffer.Count != targetCount && root == this.presetsRoot) {
                this.layoutRefreshFrames = LayoutRefreshFrameCount;
            }

            while (buffer.Count < targetCount) {
                var instance = Instantiate(template, root);
                instance.gameObject.SetActive(true);
                buffer.Add(instance);
                if (root == this.presetsRoot) {
                    this.layoutRefreshFrames = LayoutRefreshFrameCount;
                }
            }

            for (var i = 0; i < buffer.Count; i++) {
                var instance = buffer[i];
                var active   = i < targetCount;
                if (instance != null) {
                    instance.gameObject.SetActive(active);
                    if (active) {
                        instance.Render(options[i]);
                    }
                }
            }
        }

        private void RefreshLayoutIfNeeded() {
            if (this.layoutGroup == null) {
                return;
            }

            if (this.layoutRefreshFrames > 0) {
                this.layoutGroup.enabled = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.presetsRoot);
                this.layoutRefreshFrames--;

                if (this.layoutRefreshFrames == 0) {
                    this.layoutGroup.enabled = false;
                }
            }
        }
    }

    public interface IGraphicsSettingsState : IViewState {
        List<IGraphicsOptionButtonState> Presets  { get; }
        List<IGraphicsOptionButtonState> Shadows  { get; }
        List<IGraphicsOptionButtonState> Textures { get; }
        List<IGraphicsOptionButtonState> Lighting { get; }

        float MouseSensitivity    { get; }
        float MouseSensitivityMin { get; }
        float MouseSensitivityMax { get; }
        bool  IsMouseSensitivityVisible { get; }
        void  SetMouseSensitivity(float value);

        void Close();
    }
}
