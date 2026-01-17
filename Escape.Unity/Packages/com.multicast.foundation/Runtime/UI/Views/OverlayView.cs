namespace Multicast.UI.Views {
    using UniMob.UI;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using UnityEngine.UI;

    public class OverlayView : AutoView<IOverlayState> {
        [SerializeField, Required] private Image bg = default;

        [SerializeField, Required] private ViewPanel child = default;

        private Color initialColor;

        protected override void Awake() {
            base.Awake();

            this.initialColor = this.bg.color;
        }

        protected override void Render() {
            base.Render();

            var color = this.initialColor;
            color.a *= this.State.Opacity.Value;

            this.bg.color = color;
            this.child.Render(this.State.Child);
        }
    }

    public interface IOverlayState : IViewState {
        IState Child { get; }

        IAnimation<float> Opacity { get; }
    }
}