namespace Game.UI.Controllers.Scenes {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Domain;
    using ECS.Systems.Sounds;
    using Features.CoinFarm;
    using Features.ExpProgressionRewards;
    using Features.MailBox;
    using Features.Settings;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Utilities;
    using Shared.DTO;
    using Sound;
    using Tutorial;
    using UniMob;
    using UniMob.UI.Widgets;
    using Widgets.Friends;
    using Widgets.Header;
    using Widgets.MainMenu;
    using Widgets.Party;
    using Widgets.Tutorial;

    [Serializable, RequireFieldsInit]
    public struct MainMenuSceneControllerArgs : IDisposableControllerArgs {
        public IScenesController ScenesController;
    }

    public class MainMenuSceneController : DisposableController<MainMenuSceneControllerArgs> {
        [Inject] private Domain.Party.PartyModel partyModel;

        [CanBeNull] private Route route;

        private IDisposableController backgroundAudio;

        private MutableAtom<FriendsPanelState.FriendVM[]> requests;
        private MutableAtom<string[]>                     onlineIds;
        private MutableAtom<string[]>                     inGameIds;
        protected override async UniTask Activate(Context context) {
            this.requests  = Atom.Value(Array.Empty<FriendsPanelState.FriendVM>());
            this.onlineIds = Atom.Value(Lifetime, Array.Empty<string>());
            this.inGameIds = Atom.Value(Lifetime, Array.Empty<string>());

            try {
                await context.Server.MatchmakingCancel(new Game.Shared.DTO.MatchmakingCancelRequest { }, ServerCallRetryStrategy.Throw);
                this.partyModel.StopMatchmaking();
            }
            catch {
            }

            await using (await context.RunProgressScreenDisposable("loading_main_menu_scene", useSystemNavigator: true)) {
                await AddressablesUtils.LoadSceneAsync(CoreConstants.Scenes.MAIN_MENU);
                this.backgroundAudio = await context.RunDisposable(new BackgroundAudioActivateControllerArgs());

                App.Events.Listen<Shared.ServerEvents.FriendRequestIncomingAppServerEvent>(Lifetime, _ => {
                    RequestFlow(UpdateFriends);
                });

                App.Events.Listen<Shared.ServerEvents.FriendAddedAppServerEvent>(Lifetime, _ => {
                    RequestFlow(UpdateFriends);
                });

                App.Events.Listen<Shared.ServerEvents.FriendRemovedAppServerEvent>(Lifetime, _ => {
                    RequestFlow(UpdateFriends);
                });
                
                App.Events.Listen<Shared.ServerEvents.FriendStatusChangedAppServerEvent>(Lifetime, evt => {
                    RequestFlow(UpdateOnline, evt.FriendId.ToString());
                });
                
                await RefreshOnline(context);

                await UpdateFriends(context);
                
                this.route = context.RootNavigator.NewRoot(new PageRouteBuilder(
                    UiConstants.Routes.MainMenu,
                    (buildContext, animation, secondaryAnimation) => new ZStack {
                        Children = {
                            new MainMenuWidget() {
                                FriendRequestCount = this.requests.Value.Length,
                                OnlineIds = this.onlineIds.Value,
                                InGameIds = this.inGameIds.Value,
                            },
                            new HeaderWidget(),
                            new TutorialWidget {
                                Route = UiConstants.Routes.MainMenu,
                            },
                        },
                    })
                );
                await this.route!.PushTask;

                await context.RunChild(new CoinFarmFeatureControllerArgs());
                await context.RunChild(new ExpProgressionRewardsFeatureControllerArgs());
                await context.RunChild(new MailBoxFeatureControllerArgs());
                await context.RunChild(new SettingsFeatureControllerArgs());
                await context.RunTutorialOnFlow(seq => seq.On_MainMenu_Flow);
            }
        }
        
        private async UniTask RefreshOnline(Context context) {
            var online   = await context.Server.FriendsOnline(new FriendsListRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
            var statuses = online.Friends ?? Array.Empty<FriendInfoDto>();
            onlineIds.Value = statuses.Where(f => f.Status == EUserStatus.InMenu).Select(f => f.Id.ToString()).ToArray();
            inGameIds.Value = statuses.Where(f => f.Status == EUserStatus.InGame).Select(f => f.Id.ToString()).ToArray();
        }

        private async UniTask UpdateOnline(Context context, string friendId) {
            await RefreshOnline(context);
        }
        
        private async UniTask UpdateFriends(Context context) {
            await using (await context.RunProgressScreenDisposable("refreshing_friends_list")) {
                var requestsResponse =
                    await context.Server.FriendsIncoming(
                        new IncomingRequestsRequest() { },
                        ServerCallRetryStrategy.RetryWithUserDialog);
                
                requests.Value = requestsResponse.IncomingRequests?.Select(x =>
                    new FriendsPanelState.FriendVM(x.Id, x.NickName)).ToArray();
            }
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
            }
            */

            this.requests?.Deactivate();

            await using (await context.RunProgressScreenDisposable("unloading_main_menu_scene", useSystemNavigator: true)) {
                await AddressablesUtils.LoadSceneAsync(CoreConstants.Scenes.EMPTY);
            }
        }
    }
}