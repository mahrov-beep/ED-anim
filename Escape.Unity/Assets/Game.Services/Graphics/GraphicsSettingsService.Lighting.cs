namespace Game.Services.Graphics {
    using System.Collections.Generic;
    using UnityEngine;

    public partial class GraphicsSettingsService {
        private class LightingSettingImpl : IGraphicsSetting<GraphicsLightingMode> {
            private const string PrefSelected    = "GraphicsLighting.Selected";
            private const string PrefRecommended = "GraphicsLighting.Recommended";

            private readonly List<GraphicsOption<GraphicsLightingMode>> options = new();
            private readonly IGraphicsSettingsModel                      model;
            private readonly LightingSettingsConfig                      config;
            private readonly IGraphicsSettingStorage                     storage;

            public LightingSettingImpl(IGraphicsSettingsModel model, LightingSettingsConfig config, IGraphicsSettingStorage storage) {
                this.model           = model;
                this.config          = config;
                this.storage         = storage;

                var recommended = this.DetectRecommended();
                this.storage.SaveEnum(PrefRecommended, recommended);
                var selected    = this.storage.ReadEnum(PrefSelected, recommended);

                this.model.RecommendedLightingMode = recommended;
                this.model.LightingMode            = selected;

                this.BuildOptions(recommended);
                this.ApplyUnityLighting(selected);
            }

            public string Key => "lighting";

            public List<GraphicsOption<GraphicsLightingMode>> Options => this.options;

            public GraphicsLightingMode Current => this.model.LightingMode;

            public GraphicsLightingMode Recommended => this.model.RecommendedLightingMode;

            public void Apply(GraphicsLightingMode value, bool userInitiated = true) {
                var current = this.model.LightingMode;
                if (current == value) {
                    if (!userInitiated) {
                        this.ApplyUnityLighting(value);
                    }
                    return;
                }

                this.ApplyUnityLighting(value);

                this.model.LightingMode = value;
                if (userInitiated) {
                    this.storage.SaveEnum(PrefSelected, value);
                }
            }

            private void BuildOptions(GraphicsLightingMode recommended) {
                this.options.Clear();
                this.options.Add(new GraphicsOption<GraphicsLightingMode>(GraphicsLightingMode.Off, "Off", recommended == GraphicsLightingMode.Off));
                this.options.Add(new GraphicsOption<GraphicsLightingMode>(GraphicsLightingMode.On, "On", recommended == GraphicsLightingMode.On));
            }

            private GraphicsLightingMode DetectRecommended() {
                return GraphicsLightingMode.On;
            }

            private void ApplyUnityLighting(GraphicsLightingMode value) {
                var tier             = this.config?.Get(value);
                var pixelLightMin    = tier?.pixelLightCount ?? (value == GraphicsLightingMode.On ? 2 : 0);
                var enableAdditional = tier?.supportsAdditionalLights ?? value == GraphicsLightingMode.On;
                var mixedLighting    = tier?.supportsMixedLighting ?? enableAdditional;

                var pixelLightCount = Mathf.Max(0, pixelLightMin);
                QualitySettings.pixelLightCount = pixelLightCount;

                var renderingMode  = this.ResolveAdditionalLightsMode(enableAdditional, tier);
                var perObjectLimit = tier?.additionalLightsPerObjectLimit ?? (enableAdditional ? System.Math.Max(1, pixelLightCount) : 0);

                this.ApplyUrpLighting(enableAdditional, renderingMode, perObjectLimit, mixedLighting);
            }

            private void ApplyUrpLighting(bool enableAdditional, int renderingMode, int perObjectLimit, bool mixedLighting) {
                var urp = UrpReflectionAdapter.GetPipelineAsset();
                if (urp == null) {
                    return;
                }

                UrpReflectionAdapter.TrySetValue(urp, "supportsAdditionalLights", enableAdditional);
                UrpReflectionAdapter.TrySetEnum(urp, "additionalLightsRenderingMode", renderingMode); // enum AdditionalLightsRenderingMode
                UrpReflectionAdapter.TrySetValue(urp, "additionalLightsPerObjectLimit", perObjectLimit);
                UrpReflectionAdapter.TrySetValue(urp, "supportsMixedLighting", mixedLighting);
            }

            private int ResolveAdditionalLightsMode(bool enableAdditional, LightingTierSettings tier) {
                if (!enableAdditional) {
                    return 0;
                }

                var mode = tier?.additionalLightsRenderingMode ?? 1;
                return this.ClampAdditionalLightsMode(mode);
            }

            private int ClampAdditionalLightsMode(int mode) {
                if (mode < 0) {
                    return 0;
                }

                if (mode > 1) {
                    return 1;
                }

                return mode;
            }
        }
    }
}
