namespace Game.Services.Graphics {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    public partial class GraphicsSettingsService {
        private class ShadowsSettingImpl : IGraphicsSetting<GraphicsShadowQuality> {
            private const string PrefSelected    = "GraphicsShadows.Selected";
            private const string PrefRecommended = "GraphicsShadows.Recommended";

            private LightmapData[] cachedLightmaps = Array.Empty<LightmapData>();
            private LightmapsMode  cachedLightmapsMode = LightmapsMode.NonDirectional;
            private bool           lightmapsCached;

            private readonly List<GraphicsOption<GraphicsShadowQuality>> options = new();
            private readonly IGraphicsSettingsModel                      model;
            private readonly ShadowSettingsConfig                        config;
            private readonly IGraphicsSettingStorage                     storage;

            public ShadowsSettingImpl(IGraphicsSettingsModel model, ShadowSettingsConfig config, IGraphicsSettingStorage storage) {
                this.model   = model;
                this.config  = config;
                this.storage = storage;

                var recommended = this.storage.ReadEnum(PrefRecommended, this.DetectRecommended());
                var selected    = this.storage.ReadEnum(PrefSelected, recommended);

                this.model.RecommendedShadowQuality = recommended;
                this.model.ShadowQuality            = selected;

                this.BuildOptions(recommended);
                this.ApplyUnityShadows(selected);
            }

            public string Key => "shadows";

            public List<GraphicsOption<GraphicsShadowQuality>> Options => this.options;

            public GraphicsShadowQuality Current => this.model.ShadowQuality;

            public GraphicsShadowQuality Recommended => this.model.RecommendedShadowQuality;

            public void Apply(GraphicsShadowQuality value, bool userInitiated = true) {
                var current = this.model.ShadowQuality;
                if (current == value) {
                    if (!userInitiated) {
                        this.ApplyUnityShadows(value);
                    }
                    return;
                }

                this.ApplyUnityShadows(value);

                this.model.ShadowQuality = value;
                if (userInitiated) {
                    this.storage.SaveEnum(PrefSelected, value);
                }
            }

            private void BuildOptions(GraphicsShadowQuality recommended) {
                this.options.Clear();
                this.options.Add(new GraphicsOption<GraphicsShadowQuality>(GraphicsShadowQuality.Off, "Off", recommended == GraphicsShadowQuality.Off));
                this.options.Add(new GraphicsOption<GraphicsShadowQuality>(GraphicsShadowQuality.Low, "Low", recommended == GraphicsShadowQuality.Low));
                this.options.Add(new GraphicsOption<GraphicsShadowQuality>(GraphicsShadowQuality.Medium, "Medium", recommended == GraphicsShadowQuality.Medium));
                this.options.Add(new GraphicsOption<GraphicsShadowQuality>(GraphicsShadowQuality.High, "High", recommended == GraphicsShadowQuality.High));
            }

            private GraphicsShadowQuality DetectRecommended() {
                var gpuMemory    = SystemInfo.graphicsMemorySize; // MB
                var systemMemory = SystemInfo.systemMemorySize;   // MB

                if (gpuMemory >= 6000 && systemMemory >= 12000) {
                    return GraphicsShadowQuality.High;
                }

                if (gpuMemory >= 3000 && systemMemory >= 6000) {
                    return GraphicsShadowQuality.Medium;
                }

                return GraphicsShadowQuality.Low;
            }

            private void ApplyUnityShadows(GraphicsShadowQuality value) {
                var tier = this.config != null ? this.config.Get(value) : null;

                QualitySettings.shadows          = value == GraphicsShadowQuality.Off ? ShadowQuality.Disable : ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.shadowCascades   = tier?.shadowCascadeCount ?? 1;
                QualitySettings.shadowDistance   = tier?.shadowDistance ?? 0f;

                this.ApplyUrpShadows(value, tier);
                this.ApplyBakedShadows(value, tier);
            }

            private void ApplyBakedShadows(GraphicsShadowQuality value, ShadowTierSettings tier) {
                var disableBaked = tier?.disableBakedShadows ?? false;
                if (value == GraphicsShadowQuality.Off && disableBaked) {
                    this.CacheLightmaps();
                    return;
                }

                this.RestoreCachedLightmaps();
            }

            public void ClearLightmapCache() {
                this.cachedLightmaps     = Array.Empty<LightmapData>();
                this.cachedLightmapsMode = LightmapsMode.NonDirectional;
                this.lightmapsCached     = false;
            }

            public void CacheLightmaps() {
                if (this.lightmapsCached) {
                    return;
                }

                this.cachedLightmaps     = LightmapSettings.lightmaps;
                this.cachedLightmapsMode = LightmapSettings.lightmapsMode;
                this.lightmapsCached     = true;
            }

            public void ClearLightmaps() {
                LightmapSettings.lightmaps     = Array.Empty<LightmapData>();
                LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
            }

            public void RestoreCachedLightmaps() {
                if (!this.lightmapsCached) {
                    return;
                }

                if (LightmapSettings.lightmaps != null && LightmapSettings.lightmaps.Length > 0) {
                    return;
                }

                LightmapSettings.lightmaps     = this.cachedLightmaps ?? Array.Empty<LightmapData>();
                LightmapSettings.lightmapsMode = this.cachedLightmapsMode;
            }

            private void ApplyUrpShadows(GraphicsShadowQuality value, ShadowTierSettings tier) {
                var urp = UrpReflectionAdapter.GetPipelineAsset();
                if (urp == null) {
                    return;
                }

                UrpReflectionAdapter.TrySetValue(urp, "supportsMainLightShadows", tier?.mainLightShadows ?? value != GraphicsShadowQuality.Off);
                UrpReflectionAdapter.TrySetValue(urp, "supportsAdditionalLightShadows", tier?.additionalLightShadows ?? value != GraphicsShadowQuality.Off);
                UrpReflectionAdapter.TrySetValue(urp, "supportsSoftShadows", tier?.softShadows ?? value >= GraphicsShadowQuality.Medium);

                UrpReflectionAdapter.TrySetValue(urp, "shadowDistance", tier?.shadowDistance ?? 0f);

                var res = tier?.mainLightResolution ?? this.MapShadowResolution(value);
                UrpReflectionAdapter.TrySetValue(urp, "mainLightShadowmapResolution", res);
                UrpReflectionAdapter.TrySetValue(urp, "additionalLightsShadowAtlasResolution", tier?.additionalLightResolution ?? res);

                UrpReflectionAdapter.TrySetValue(urp, "shadowCascadeCount", tier?.shadowCascadeCount ?? 1);
            }

            private int MapShadowResolution(GraphicsShadowQuality value) {
                return value switch {
                    GraphicsShadowQuality.Off    => 256,
                    GraphicsShadowQuality.Low    => 512,
                    GraphicsShadowQuality.Medium => 1024,
                    GraphicsShadowQuality.High   => 2048,
                    _                            => 512
                };
            }            
        }
    }
}
