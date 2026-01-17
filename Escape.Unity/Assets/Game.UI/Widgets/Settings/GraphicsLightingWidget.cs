namespace Game.UI.Widgets.Settings {
    using Services.Graphics;
    using UniMob.UI;

    public class GraphicsLightingWidget : GraphicsOptionWidget<GraphicsLightingMode> {
        public override State CreateState() => new GraphicsLightingState();
    }

    public class GraphicsLightingState : GraphicsOptionState<GraphicsLightingMode> {
    }
}
