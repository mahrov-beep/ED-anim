namespace Game.UI.Controllers {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Multicast.Routes;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.Tutorial;

    [RequireFieldsInit(Optional = new[] {
        nameof(PageAnimated),
        nameof(TransitionDuration),
        nameof(ReverseTransitionDuration),
        nameof(UseSystemNavigator),
        nameof(OnBackPerformed),
    })]
    public struct UiScreenControllerArgs : IDisposableControllerArgs {
        public RouteSettings Route;

        public Func<Widget> Page;
        public PageBuilder  PageAnimated;

        public float? TransitionDuration;
        public float? ReverseTransitionDuration;

        public bool UseSystemNavigator;

        public Action OnBackPerformed;
    }

    public class UiScreenController : DisposableController<UiScreenControllerArgs> {
        private Route route;

        public override string DebugName => $"UI : {this.Args.Route.Name} ({this.Args.Route.ModalType})";

        protected override async UniTask Activate(Context context) {
            var routeSettings             = this.Args.Route;
            var transitionDuration        = this.Args.TransitionDuration.GetValueOrDefault(0.2f);
            var reverseTransitionDuration = this.Args.ReverseTransitionDuration.GetValueOrDefault(transitionDuration);

            var navigator = this.Args.UseSystemNavigator
                ? context.GetNavigator(AppNavigatorType.System)
                : context.RootNavigator;

            var page = this.Args.PageAnimated ?? delegate { return this.Args.Page(); };

            this.route = navigator.Push(new FadeRoute(
                settings: routeSettings,
                pageBuilder: (buildContext, animation, secondaryAnimation) => new ZStack {
                    Children = {
                        page.Invoke(buildContext, animation, secondaryAnimation),
                        new TutorialWidget {
                            Route = routeSettings,
                        },
                    },
                },
                transitionDuration: transitionDuration,
                reverseTransitionDuration: reverseTransitionDuration
            ));

            IBackActionOwner backActionOwner = this.route;
            backActionOwner.SetBackAction(() => {
                if (this.Args.OnBackPerformed == null) {
                    return false;
                }
                
                this.Args.OnBackPerformed.Invoke();
                return true;
            });

            await this.route.PushTask;
            await UniTask.Delay(TimeSpan.FromSeconds(transitionDuration), DelayType.UnscaledDeltaTime);
        }

        protected override async UniTask OnDisposeAsync(Context context) {
            var navigator = this.Args.UseSystemNavigator
                ? context.GetNavigator(AppNavigatorType.System)
                : context.RootNavigator;

            if (navigator.TopmostRoute == this.route) {
                navigator.Pop();
                await this.route.PopTask;
            }
            else {
                Debug.LogError($"[UIScreen] Critical: Failed to pop {this.route.Key} route: TopmostRoute is {navigator.TopmostRoute.Key}");
            }
        }
    }
}