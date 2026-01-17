namespace Multicast.Boot.Steps {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    internal struct UniMobUiControllerArgs : IFlowControllerArgs {
        public ViewPanel SystemViewPanel;
        public ViewPanel RootViewPanel;

        public GlobalKey<NavigatorState> SystemKey;
        public GlobalKey<NavigatorState> RootKey;
    }

    internal class UniMobUiController : FlowController<UniMobUiControllerArgs> {
        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            ControllersShared.RegisterController<UniMobUiControllerArgs, UniMobUiController>();
        }

        protected override async UniTask Activate(Context context) {
            UniMobUI.RunApp(this.Lifetime, StateProvider.Shared, this.Args.SystemViewPanel, _ => this.BuildSystemNavigator());
            UniMobUI.RunApp(this.Lifetime, StateProvider.Shared, this.Args.RootViewPanel, _ => this.BuildRootNavigator());

            while (this.Args.SystemKey.CurrentState == null) {
                await UniTask.Yield();
            }

            while (this.Args.RootKey.CurrentState == null) {
                await UniTask.Yield();
            }

            Atom.Reaction(this.Lifetime,
                () => this.Args.RootKey.CurrentState.TopmostRoute,
                route => this.OnNavigatorRouteChanged(route),
                fireImmediately: false);
        }

        protected void OnNavigatorRouteChanged(Route route) {
            App.RequestAppUpdateFlow();

            if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                Debug.Log($"Route changed: {route.Key}");
            }

            CoreAnalytics.ReportEvent("route", new Dictionary<string, object> {
                ["route"] = route.Key,
            });
        }

        private Widget BuildSystemNavigator() {
            return new Navigator("system", new Dictionary<string, Func<Route>> {
                ["system"] = () => this.BuildSystemRoute(),
            }) {
                Key = this.Args.SystemKey,
            };
        }

        private Route BuildSystemRoute() {
            return new PageRouteBuilder(
                new RouteSettings("system", RouteModalType.Fullscreen),
                (context, controller, secondaryAnimation) => new Empty()
            );
        }

        private Widget BuildRootNavigator() {
            return new Navigator("empty", new Dictionary<string, Func<Route>> {
                ["empty"] = this.BuildLoaderRoute,
            }) {
                Key = this.Args.RootKey,
            };
        }

        private Route BuildLoaderRoute() {
            return new PageRouteBuilder(
                new RouteSettings("loader", RouteModalType.Fullscreen),
                (context, controller, secondaryAnimation) => new Empty()
            );
        }
    }
}