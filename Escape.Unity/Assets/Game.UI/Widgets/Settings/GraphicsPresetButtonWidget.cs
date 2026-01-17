namespace Game.UI.Widgets.Settings {
    using Services.Graphics;
    using Multicast;
    using UniMob.UI;
    using Views.Settings;
    using UnityEngine;

    public class GraphicsPresetButtonWidget : StatefulWidget {
        public GraphicsQualityOption Option { get; set; }
    }

    public class GraphicsPresetButtonState : ViewState<GraphicsPresetButtonWidget>, IGraphicsPresetButtonState {
        [Inject] private IGraphicsSettingsModel graphicsSettingsModel;
        [Inject] private GraphicsSettingsService graphicsSettingsService;

        public override WidgetViewReference View => default;

        public int QualityIndex => this.Widget.Option.Index;
        public string RawName => this.Widget.Option.Name;

        public string Title => string.IsNullOrWhiteSpace(this.Widget.Option.Name)
            ? $"Quality {this.Widget.Option.Index + 1}"
            : this.Widget.Option.Name;

        public bool IsSelected    => this.graphicsSettingsModel.SelectedQualityIndex == this.Widget.Option.Index;
        public bool IsRecommended => this.graphicsSettingsModel.RecommendedQualityIndex == this.Widget.Option.Index;

        public void Select() {
            this.graphicsSettingsService.ApplyQualityLevel(this.Widget.Option.Index);
        }
    }
}
