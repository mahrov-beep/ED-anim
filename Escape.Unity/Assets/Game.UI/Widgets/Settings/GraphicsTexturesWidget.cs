namespace Game.UI.Widgets.Settings {
    using Services.Graphics;
    using UniMob.UI;

    public class GraphicsTexturesWidget : GraphicsOptionWidget<GraphicsTextureQuality> {
        public override State CreateState() => new GraphicsTexturesState();
    }

    public class GraphicsTexturesState : GraphicsOptionState<GraphicsTextureQuality> {
    }
}
