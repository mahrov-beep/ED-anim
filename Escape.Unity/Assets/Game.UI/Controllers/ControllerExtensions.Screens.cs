namespace Game.UI.Controllers {
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UI;
    using UniMob;
    using Widgets.Common;

    public static partial class ControllerExtensions {
        public static async UniTask<IUniTaskAsyncDisposable> RunProgressScreenDisposable(this ControllerBase.Context context,
            string message, string parameters = null, bool useSystemNavigator = false) {
            return await RunProgressScreenDisposable(context, message: Atom.Value(message), parameters: Atom.Value(parameters), useSystemNavigator: useSystemNavigator);
        }

        public static async UniTask<IUniTaskAsyncDisposable> RunProgressScreenDisposable(this ControllerBase.Context context,
            Atom<float> progress = null, Atom<string> message = null, Atom<string> parameters = null, bool useSystemNavigator = false) {
            return await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.ProgressScreen,
                Page = () => new ProgressScreenWidget {
                    Progress   = progress,
                    Message    = message,
                    Parameters = parameters,
                },
                TransitionDuration        = 0.05f,
                ReverseTransitionDuration = 0.05f,
                UseSystemNavigator        = useSystemNavigator,
            });
        }

        public static async UniTask<IUniTaskAsyncDisposable> RunBlackScreenDisposable(this ControllerBase.Context context, bool useSystemNavigator = false) {
            return await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.BlackScreen,
                Page = () => new LoadingScreenWidget {
                    View = UiConstants.Views.LoadingBlackScreen,
                },
                TransitionDuration        = 0.1f,
                ReverseTransitionDuration = 0.1f,
                UseSystemNavigator        = useSystemNavigator,
            });
        }

        public static async UniTask<IUniTaskAsyncDisposable> RunLoadingScreenDisposable(this ControllerBase.Context context, bool useSystemNavigator = false) {
            return await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.LoadingScreen,
                Page = () => new LoadingScreenWidget {
                    View = UiConstants.Views.LoadingScreen,
                },
                TransitionDuration        = 0.4f,
                ReverseTransitionDuration = 0.4f,
                UseSystemNavigator        = useSystemNavigator,
            });
        }

        public static async UniTask<IUniTaskAsyncDisposable> RunSearchGameScreenDisposable(this ControllerBase.Context context, bool useSystemNavigator = false) {
            return await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.SearchGameScreen,
                Page = () => new SearchGameScreenWidget(),
                TransitionDuration        = 0f,
                ReverseTransitionDuration = 0f,
                UseSystemNavigator        = useSystemNavigator,
            });
        }

        public static async UniTask<IUniTaskAsyncDisposable> RunFadeScreenDisposable(this ControllerBase.Context context) {
            return await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.FadeScreen,
                Page = () => new LoadingScreenWidget {
                    View = UiConstants.Views.LoadingFadeScreen,
                },
                TransitionDuration        = 0.1f,
                ReverseTransitionDuration = 0.1f,
            });
        }

        public static async UniTask<IUniTaskAsyncDisposable> RunBgScreenDisposable(this ControllerBase.Context context, float showDuration = 0.2f) {
            return await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.BgScreen,
                Page = () => new LoadingScreenWidget {
                    View = UiConstants.Views.LoadingBgScreen,
                },
                TransitionDuration        = showDuration,
                ReverseTransitionDuration = 0.2f,
            });
        }
    }
}