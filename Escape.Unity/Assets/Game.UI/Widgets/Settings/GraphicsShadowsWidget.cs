namespace Game.UI.Widgets.Settings {
    using Services.Graphics;
    using UniMob.UI;

    public class GraphicsShadowsWidget : GraphicsOptionWidget<GraphicsShadowQuality> {
        public override State CreateState() => new GraphicsShadowsState();
    }

    public class GraphicsShadowsState : GraphicsOptionState<GraphicsShadowQuality> {
    }
}
