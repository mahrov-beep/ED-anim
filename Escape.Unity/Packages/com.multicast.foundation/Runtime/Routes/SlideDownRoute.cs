namespace Multicast.Routes {
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class SlideDownRoute : PageRouteBuilder {
        public SlideDownRoute(
            RouteSettings settings,
            PageBuilder pageBuilder
        ) : base(settings, pageBuilder, BuildSlideOverlayTransition(), 0.15f, 0.15f) {
        }

        private static PageTransitionsBuilder BuildSlideOverlayTransition() {
            return (context, animation, secondaryAnimation, child) => new CompositeTransition {
                Opacity  = animation,
                Position = animation.Drive(new Vector2Tween(Vector2.down * 0.2f, Vector2.zero)),
                Child    = child,
            };
        }
    }
}