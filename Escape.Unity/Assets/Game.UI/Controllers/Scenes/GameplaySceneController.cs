namespace Game.UI.Controllers.Scenes {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain;
    using ECS.Systems.Sounds;
    using Features.GameInventory;
    using Features.Quest;
    using Features.Settings;
    using Features.SelectedItemInfo;
    using Gameplay;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Utilities;
    using Photon;
    using Shared.UserProfile.Data;
    using Sound;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets.Game;

    [Serializable, RequireFieldsInit]
    public struct GameplaySceneControllerArgs : IDisposableControllerArgs {
        public IScenesController ScenesController;
    }

    public class GameplaySceneController : DisposableController<GameplaySceneControllerArgs> {
        [Inject] private SdUserProfile     sdUserProfile;

        private IDisposableController backgroundAudio;

        [CanBeNull] private Route route;

        protected override async UniTask Activate(Context context) {
            await using (await context.RunProgressScreenDisposable("loading_game_scene", useSystemNavigator: true)) {
                // scene loaded by Quantum

                this.route = context.RootNavigator.NewRoot(new PageRouteBuilder(
                    new RouteSettings("gameplay", RouteModalType.Fullscreen),
                    (buildContext, animation, secondaryAnimation) => new GameWidget()
                ));
                await this.route!.PushTask;

                this.backgroundAudio = await context.RunDisposable(new BackgroundAudioActivateControllerArgs());
            }

            await context.RunChild(new GameInventoryFeatureControllerArgs());
            await context.RunChild(new SettingsFeatureControllerArgs());
            await context.RunChild(new GameplayControlsControllerArgs());
        }

        protected override async UniTask OnDisposeAsync(Context context) {
            await this.backgroundAudio.DisposeAsyncNullable();

            await context.RootNavigator.Push(new PageRouteBuilder(
                new RouteSettings("empty", RouteModalType.Fullscreen),
                (context, animation, secondaryAnimation) => new Empty())
            ).PushTask;
            /*
            if (this.route != null && context.RootNavigator.TopmostRoute == this.route) {
                context.RootNavigator.Pop();
                await this.route.PopTask;
                this.route = null;
            }*/

            await using (await context.RunProgressScreenDisposable("unloading_game_scene", useSystemNavigator: true)) {
                await AddressablesUtils.LoadSceneAsync(CoreConstants.Scenes.EMPTY);
            }
        }
    }
}