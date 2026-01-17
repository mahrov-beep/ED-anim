namespace Multicast.Routes {
    using UI.Widgets;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class SlideDownOverlayRoute : PageRouteBuilder {
        public SlideDownOverlayRoute(
            WidgetViewReference overlayView,
            RouteSettings settings,
            PageBuilder pageBuilder,
            bool disableCamera = false
        ) : base(settings, pageBuilder, BuildSlideOverlayTransition(overlayView, disableCamera), 0.15f, 0.15f) {
        }

        private static PageTransitionsBuilder BuildSlideOverlayTransition(WidgetViewReference overlayView, bool disableCamera) {
            return (context, animation, secondaryAnimation, child) => new OverlayWidget(animation, overlayView) {
                DisableCamera = disableCamera,
                Child = new CompositeTransition {
                    Opacity  = animation,
                    Position = animation.Drive(new Vector2Tween(Vector2.down * 0.2f, Vector2.zero)),
                    Child    = child,
                },
            };
        }
    }
}