namespace Game.Services.Graphics {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public partial class GraphicsSettingsService {
        private class TexturesSettingImpl : IGraphicsSetting<GraphicsTextureQuality> {
            private const string PrefSelected    = "GraphicsTextures.Selected";
            private const string PrefRecommended = "GraphicsTextures.Recommended";

            private readonly List<GraphicsOption<GraphicsTextureQuality>> options = new();
            private readonly IGraphicsSettingsModel                        model;
            private readonly TextureSettingsConfig                         config;
            private readonly IGraphicsSettingStorage                       storage;

            public TexturesSettingImpl(IGraphicsSettingsModel model, TextureSettingsConfig config, IGraphicsSettingStorage storage) {
                this.model   = model;
                this.config  = config;
                this.storage = storage;

                var recommended = this.DetectRecommended();
                this.storage.SaveEnum(PrefRecommended, recommended);
                var selected    = this.storage.ReadEnum(PrefSelected, recommended);

                this.model.RecommendedTextureQuality = recommended;
                this.model.TextureQuality            = selected;

                this.BuildOptions(recommended);
                this.ApplyUnityTextures(selected);
            }

            public string Key => "textures";

            public List<GraphicsOption<GraphicsTextureQuality>> Options => this.options;

            public GraphicsTextureQuality Current => this.model.TextureQuality;

            public GraphicsTextureQuality Recommended => this.model.RecommendedTextureQuality;

            public void Apply(GraphicsTextureQuality value, bool userInitiated = true) {
                var current = this.model.TextureQuality;
                if (current == value) {
                    if (!userInitiated) {
                        this.ApplyUnityTextures(value);
                    }
                    return;
                }

                this.ApplyUnityTextures(value);

                this.model.TextureQuality = value;
                if (userInitiated) {
                    this.storage.SaveEnum(PrefSelected, value);
                }
            }

            private void BuildOptions(GraphicsTextureQuality recommended) {
                this.options.Clear();
                this.options.Add(new GraphicsOption<GraphicsTextureQuality>(GraphicsTextureQuality.Low, "Low", recommended == GraphicsTextureQuality.Low));
                this.options.Add(new GraphicsOption<GraphicsTextureQuality>(GraphicsTextureQuality.Medium, "Medium", recommended == GraphicsTextureQuality.Medium));
                this.options.Add(new GraphicsOption<GraphicsTextureQuality>(GraphicsTextureQuality.High, "High", recommended == GraphicsTextureQuality.High));
            }

            private GraphicsTextureQuality DetectRecommended() {
                return GraphicsSettingsService.DetectRecommendedTier() switch {
                    RecommendedGraphicsTier.Low    => GraphicsTextureQuality.Low,
                    RecommendedGraphicsTier.Medium => GraphicsTextureQuality.Medium,
                    _                              => GraphicsTextureQuality.High
                };
            }

            private void ApplyUnityTextures(GraphicsTextureQuality value) {
                var tier = this.config?.Get(value);

                var mipLimit = tier?.mipmapLimit ?? value switch {
                    GraphicsTextureQuality.Low    => 2,
                    GraphicsTextureQuality.Medium => 1,
                    GraphicsTextureQuality.High   => 0,
                    _                             => 1
                };

                QualitySettings.globalTextureMipmapLimit = Mathf.Clamp(mipLimit, 0, 4);

                if (tier != null) {
                    QualitySettings.anisotropicFiltering = tier.anisotropicLimit <= 0
                        ? AnisotropicFiltering.Disable
                        : AnisotropicFiltering.ForceEnable;

                    QualitySettings.streamingMipmapsActive            = tier.textureStreaming;
                    QualitySettings.streamingMipmapsAddAllCameras     = tier.textureStreaming;
                    QualitySettings.streamingMipmapsMaxLevelReduction = Mathf.Clamp(tier.streamingMaxLevelReduction, 0, 10);
                    QualitySettings.streamingMipmapsMemoryBudget      = Mathf.Max(32, tier.streamingMemoryBudgetMB);

                    var maxAniso = Mathf.Max(1, tier.anisotropicLimit);
                    Texture.SetGlobalAnisotropicFilteringLimits(1, maxAniso);
                } else {
                    QualitySettings.anisotropicFiltering          = AnisotropicFiltering.Disable;
                    QualitySettings.streamingMipmapsActive        = false;
                    QualitySettings.streamingMipmapsAddAllCameras = false;
                    QualitySettings.streamingMipmapsMaxLevelReduction = 2;
                    QualitySettings.streamingMipmapsMemoryBudget      = 512;
                    Texture.SetGlobalAnisotropicFilteringLimits(1, 1);
                }
            }
        }
    }
}
