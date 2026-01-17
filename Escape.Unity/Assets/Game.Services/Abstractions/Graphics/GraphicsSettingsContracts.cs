namespace Game.Services.Graphics {
    public readonly struct GraphicsQualityOption {
        public GraphicsQualityOption(int index, string name) {
            this.Index = index;
            this.Name  = name;
        }

        public int    Index { get; }
        public string Name  { get; }

        public override string ToString() {
            return $"{this.Index}:{this.Name}";
        }
    }

    public interface IGraphicsSettingsModel {
        int SelectedQualityIndex    { get; set; }
        int RecommendedQualityIndex { get; set; }

        GraphicsShadowQuality ShadowQuality            { get; set; }
        GraphicsShadowQuality RecommendedShadowQuality { get; set; }

        GraphicsTextureQuality TextureQuality            { get; set; }
        GraphicsTextureQuality RecommendedTextureQuality { get; set; }

        GraphicsLightingMode LightingMode            { get; set; }
        GraphicsLightingMode RecommendedLightingMode { get; set; }
    }
}
