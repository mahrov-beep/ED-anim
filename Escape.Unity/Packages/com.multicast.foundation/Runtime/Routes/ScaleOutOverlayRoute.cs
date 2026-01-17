namespace Multicast.Routes {
    using DG.Tweening;
    using DG.Tweening.Core.Easing;
    using UI.Widgets;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class ScaleOutOverlayRoute : PageRouteBuilder {
        private static readonly EaseFunction BackOutEase = EaseManager.ToEaseFunction(Ease.OutBack);

        public ScaleOutOverlayRoute(
            WidgetViewReference overlayView,
            RouteSettings settings,
            PageBuilder pageBuilder,
            bool disableCamera = false
        ) : base(settings, pageBuilder, BuildSlideOverlayTransition(overlayView, disableCamera), 0.25f, 0.15f) {
        }

        private static PageTransitionsBuilder BuildSlideOverlayTransition(WidgetViewReference overlayView, bool disableCamera) {
            return (context, animation, secondaryAnimation, child) => new OverlayWidget(animation, overlayView) {
                DisableCamera = disableCamera,
                Child = new CompositeTransition {
                    Opacity = animation,
                    Scale = new CurvedAnimation(animation, curve: v => BackOutEase(v, 1f, 3.5f, 0), reverseCurve: v => v)
                        .Drive(new Vector3Tween(Vector3.one * 0.8f, Vector3.one)),
                    Child = child,
                },
            };
        }
    }
}