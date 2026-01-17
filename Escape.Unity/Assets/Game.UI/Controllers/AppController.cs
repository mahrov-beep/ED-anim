namespace Game.UI.Controllers {
    using Cysharp.Threading.Tasks;
    using Drops;
    using Features.GameplayStart;
    using Features.GameResults;
    using Features.Gunsmith;
    using Features.NameEdit;
    using Features.Quest;
    using Features.Storage;
    using Features.Store;
    using Features.Thresher;
    using Features.TraderShop;
    using Features.Friends;
    using Features.Party;
    using Multicast;
    using Photon;
    using Server;
    using Sound;
    using Gameplay;
    using RewardOpening;

    public class AppController : FlowController<AppMainControllerArgs> {
        protected override async UniTask Activate(Context context) {
            await context.RunChild(new LunarCheatsControllerArgs());
            await context.RunChild(new ServerConnectToAppEventsControllerArgs());
            await context.RunChild(new ServerListenProfileUpdatedEventControllerArgs());
            await context.RunChild(new ServerListenDebugLogEventControllerArgs());

            await context.GetNavigator(AppNavigatorType.System).PushFloatNotifications();

            var scenesController = (IScenesController)await context.RunChild(new ScenesControllerArgs());

            await context.RunChild(new GameResultsFeatureControllerArgs());
            
            await context.RunChild(new SoundControllerArgs());

            await context.RunChild(new ReconnectOnConnectionLostControllerArgs {
                ScenesController = scenesController,
            });
            await context.RunChild(new GameplayStartFeatureControllerArgs {
                ScenesController = scenesController,
            });
            await context.RunChild(new GameplayListenFinishControllerArgs {
                ScenesController = scenesController,
            });

            await context.RunChild(new StorageFeatureControllerArgs());
            await context.RunChild(new GunsmithFeatureControllerArgs());
            await context.RunChild(new TraderShopFeatureControllerArgs());
            await context.RunChild(new ThresherFeatureControllerArgs());
            await context.RunChild(new QuestFeatureControllerArgs());
            await context.RunChild(new NameEditFeatureControllerArgs());
            await context.RunChild(new StoreFeatureControllerArgs());
            await context.RunChild(new FriendsFeatureControllerArgs());
            await context.RunChild(new PartyFeatureControllerArgs {
                ScenesController = scenesController,
            });

            await context.RunChild(new DropOpeningControllerArgs());
            await context.RunChild(new RewardOpeningControllerArgs());
        }
    }
}