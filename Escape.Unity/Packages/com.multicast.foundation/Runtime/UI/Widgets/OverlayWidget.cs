namespace Multicast.UI.Widgets {
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Unity;
    using Views;

    public class OverlayWidget : StatefulWidget {
        public AnimationController OpacityController { get; }
        public WidgetViewReference View              { get; }

        public Widget Child         { get; set; }
        public bool   DisableCamera { get; set; }

        public OverlayWidget(AnimationController opacityController, WidgetViewReference view) {
            this.OpacityController = opacityController;
            this.View              = view;
        }

        public override State CreateState() => new OverlayState();
    }

    public class OverlayState : ViewState<OverlayWidget>, IOverlayState {
        private readonly StateHolder childState;

        public OverlayState() {
            this.childState = this.CreateChild(_ => this.Widget.Child ?? new Empty());
        }

        public override void InitState() {
            base.InitState();

            Atom.Reaction(
                this.StateLifetime,
                () => this.Widget.DisableCamera && this.Widget.OpacityController.IsCompleted,
                this.DisableCamera
            );
        }

        public override void Dispose() {
            base.Dispose();

            this.DisableCamera(false);
        }

        public override WidgetViewReference View => this.Widget.View;

        public IState            Child   => this.childState.Value;
        public IAnimation<float> Opacity => this.Widget.OpacityController;

        private void DisableCamera(bool disable) {
            if (disable) {
                MainCameraControl.AddDisabler(this);
            }
            else {
                MainCameraControl.RemoveDisabler(this);
            }
        }
    }
}