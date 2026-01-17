namespace Multicast.Routes {
    using UniMob.UI.Widgets;

    public class FadeRoute : PageRouteBuilder {
        public FadeRoute(
            RouteSettings settings,
            PageBuilder pageBuilder,
            float transitionDuration = 0.2f,
            float reverseTransitionDuration = 0.2f)
            : base(settings, pageBuilder, Fade, transitionDuration, reverseTransitionDuration) {
        }

        public static PageTransitionsBuilder Fade { get; } = (context, animation, secondaryAnimation, child) => new CompositeTransition {
            Opacity = animation,
            Child   = child,
        };
    }
}