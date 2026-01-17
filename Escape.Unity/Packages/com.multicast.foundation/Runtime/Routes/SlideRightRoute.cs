namespace Multicast.Routes {
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class SlideRightRoute : PageRouteBuilder {
        public SlideRightRoute(
            RouteSettings settings,
            PageBuilder pageBuilder,
            float transitionDuration = 0.2f,
            float reverseTransitionDuration = 0.2f)
            : base(settings, pageBuilder, SlideRight, transitionDuration, reverseTransitionDuration) {
        }

        public static PageTransitionsBuilder SlideRight { get; } = (context, animation, secondaryAnimation, child) => new CompositeTransition {
            Position = animation.Drive(new Vector2Tween(Vector2.right, Vector2.zero)),
            Child    = child,
        };
    }
}