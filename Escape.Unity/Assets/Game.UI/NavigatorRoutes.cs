namespace Game.UI {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Routes;
    using UniMob;
    using UniMob.UI.Widgets;
    using Widgets;
    using Widgets.Common;
    using Widgets.EditName;
    using Widgets.Game;
    using Widgets.GameModes;
    using Widgets.Gunsmiths;
    using Widgets.Header;
    using Widgets.ItemInfo;
    using Widgets.LevelUp;
    using Widgets.MainMenu;
    using Widgets.Purchases;
    using Widgets.QuestMenu;
    using Widgets.Storage;
    using Widgets.Subscription;
    using Widgets.Threshers;
    using Widgets.Tutorial;

    public static class NavigatorRoutes {
        private const string ROUTE_MAIN_MENU      = "main_menu";
        private const string ROUTE_GAME_MENU      = "game_menu";
        private const string ROUTE_GAME_ITEM_INFO = "item_info";

        public static bool TopMostRouteIs(this NavigatorState navigatorState, RouteSettings route) {
            return navigatorState.TopmostRoute.Key == route.Name;
        }

        public static bool IsOnGameplay(this NavigatorState navigatorState) => navigatorState.TopmostRoute.Key == "gameplay";
        
        public static bool IsOnMainMenu(this NavigatorState navigatorState) => navigatorState.TopmostRoute.Key == ROUTE_MAIN_MENU;

        public static bool IsOnGameMenu(this NavigatorState navigatorState) => navigatorState.TopmostRoute.Key == ROUTE_GAME_MENU;
        public static bool IsOnItemInfo(this NavigatorState navigatorState) => navigatorState.TopmostRoute.Key == ROUTE_GAME_ITEM_INFO;

        public static async Task NewRootEmpty(this NavigatorState navigatorState) {
            await navigatorState.NewRoot(new PageRouteBuilder(new RouteSettings("empty", RouteModalType.Fullscreen),
                (context, animation, secondaryAnimation) => new Empty())).PushTask;
        }

        public static Task NewRootMainMenu(this NavigatorState navigatorState) {
            return navigatorState.NewRoot(new PageRouteBuilder(
                new RouteSettings(ROUTE_MAIN_MENU, RouteModalType.Fullscreen),
                (buildContext, animation, secondaryAnimation) => new ZStack {
                    Children = {
                        new MainMenuWidget(),
                        new HeaderWidget(),
                    },
                })
            ).PushTask;
        }

        public static Task NewRootGameMenu(this NavigatorState navigatorState) {
            return navigatorState.NewRoot(new PageRouteBuilder(
                new RouteSettings(ROUTE_GAME_MENU, RouteModalType.Fullscreen),
                (buildContext, animation, secondaryAnimation) => new GameWidget())).PushTask;
        }

        public static Task PushFloatNotifications(this NavigatorState navigatorState) {
            return navigatorState.Push(new RouteBuilder(
                new RouteSettings("float_notifications", RouteModalType.Popup),
                buildContext => new FloatNotificationsWidget()
            )).PushTask;
        }

        public static Task PushGameInventoryByNearby(this NavigatorState navigatorState) {
            return navigatorState.Push(new FadeRoute(
                new RouteSettings("game_inventory_by_nearby", RouteModalType.Popup),
                (buildContext, animation, secondaryAnimation) => new InventoryByNearbyWidget {
                    OnClose   = () => navigatorState.Pop(),
                    Animation = animation,
                },
                transitionDuration: 0.1f,
                reverseTransitionDuration: 0.1f
            )).PushTask;
        }

        public static Task<bool> PushTutorialPopup<TStep>(this NavigatorState navigatorState, string tutorialKey, TStep tutorialStep)
            where TStep : struct, Enum {
            var tutorialStepName = EnumNames<TStep>.GetName(tutorialStep);

            return navigatorState.Push<bool>(new ScaleOutOverlayRoute(
                UiConstants.Views.BlackOverlay,
                UiConstants.Routes.TutorialPopup(tutorialKey, tutorialStepName),
                (context, animation, secondaryAnimation) => new TutorialPopupWidget {
                    TutorialKey  = tutorialKey,
                    TutorialStep = tutorialStepName,
                    OnClose      = () => navigatorState.Pop(),
                }
            ));
        }

        public static Task<bool> Alert(this NavigatorState navigatorState, AlertDialogWidget dialog) {
            var route = new ScaleOutOverlayRoute(
                UiConstants.Views.BlackOverlay,
                new RouteSettings($"alert_{dialog.Title}", RouteModalType.Popup),
                (buildContext, animation, secondaryAnimation) => dialog
            );

            if (dialog.IsCloseable) {
                route = route.WithPopOnBack(navigatorState, result: false);
            }

            return navigatorState.Push<bool>(route);
        }

        public static Task AlertMetaFailedBadRequest(this NavigatorState navigatorState, string message) {
            return navigatorState.Alert(AlertDialogWidget.Ok("META_FAILED_BAD_REQUEST").WithArgs(message).NonCloseable());
        }

        public static Task AlertMetaFailedToConnectToServer(this NavigatorState navigatorState, string message) {
            return navigatorState.Alert(AlertDialogWidget.Retry("META_FAILED_TO_CONNECT").WithArgs(message).NonCloseable());
        }

        public static Task<bool> AlertPhotonFailedToConnectToServer(this NavigatorState navigatorState, string message) {
            return navigatorState.Alert(AlertDialogWidget.RetryCancel("PHOTON_FAILED_TO_CONNECT").WithArgs(message).NonCloseable());
        }
    }
}