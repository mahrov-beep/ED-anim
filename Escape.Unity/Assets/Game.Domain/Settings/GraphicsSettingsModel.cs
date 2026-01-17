namespace Game.Domain.Settings {
    using Services.Graphics;
    using Multicast;
    using Services.Graphics;
    using UniMob;

    public class GraphicsSettingsModel : Model, IGraphicsSettingsModel {
        public GraphicsSettingsModel(Lifetime lifetime) : base(lifetime) {
        }

        [Atom] public int SelectedQualityIndex    { get; set; }
        [Atom] public int RecommendedQualityIndex { get; set; }

        [Atom] public GraphicsShadowQuality ShadowQuality             { get; set; }
        [Atom] public GraphicsShadowQuality RecommendedShadowQuality  { get; set; }

        [Atom] public GraphicsTextureQuality TextureQuality            { get; set; }
        [Atom] public GraphicsTextureQuality RecommendedTextureQuality { get; set; }

        [Atom] public GraphicsLightingMode LightingMode             { get; set; }
        [Atom] public GraphicsLightingMode RecommendedLightingMode  { get; set; }
    }
}
