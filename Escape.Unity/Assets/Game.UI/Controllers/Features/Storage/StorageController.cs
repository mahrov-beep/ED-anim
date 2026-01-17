namespace Game.UI.Controllers.Features.Storage {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Domain;
    using ECS.Systems.Player;
    using Loadout;
    using Multicast;
    using Photon;
    using Quantum;
    using SelectedItemInfo;
    using Services.Photon;
    using Shared.UserProfile.Commands.Loadouts;
    using Shared.UserProfile.Data;
    using Sound;
    using UI;
    using Widgets;

    [Serializable, RequireFieldsInit]
    public struct StorageControllerArgs : IFlowControllerArgs {
    }

    public class StorageController : FlowController<StorageControllerArgs> {
        [Inject] private SdUserProfile  sdUserProfile;
        [Inject] private PhotonService  photonService;
        [Inject] private LocalPlayerSystem localPlayerSystem;

        private IDisposableController quantumSimulationController;

        private IUniTaskAsyncDisposable storageScreen;
        private IUniTaskAsyncDisposable bgScreen;

        private IUniTaskAsyncDisposable selectedItemInfoController;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);

            await context.RunChild(new StorageSyncLoadoutWithServerControllerArgs());

            this.selectedItemInfoController = await context.RunDisposable(new SelectedItemInfoFeatureControllerArgs());

            StorageFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));
            StorageFeatureEvents.IncrementLoadout.Listen(this.Lifetime, () => this.RequestFlow(this.IncrementLoadout));
            StorageFeatureEvents.DecrementLoadout.Listen(this.Lifetime, () => this.RequestFlow(this.DecrementLoadout));
        }

        private async UniTask Open(Context context) {
            var selectedLoadout = this.sdUserProfile.Loadouts.Get(this.sdUserProfile.Loadouts.SelectedLoadout.Value);

            if (!string.IsNullOrEmpty(selectedLoadout.LockedForGame.Value)) {
                return;
            }

            this.bgScreen = await context.RunBgScreenDisposable();
            await context.RunChild(new BackgroundAudioLowPassActivationControllerArgs());

            this.quantumSimulationController = await context.RunDisposable(new PhotonLocalSimulationControllerArgs {
                GameModeAssetPath = CoreConstants.Quantum.GameModeAssets.MAIN_MENU_STORAGE,
                UnitySceneName    = CoreConstants.Scenes.MAIN_MANU_STORAGE_ADDITIVE,
                Loadout           = this.sdUserProfile.Loadouts.GetSelectedLoadoutClone(),
                Storage           = this.CreateGameSnapshotStorage(),
            });

            var loadoutCount = this.sdUserProfile.Loadouts.Lookup.Count;
            var current      = this.sdUserProfile.Loadouts.SelectedLoadout.Value;
            var loadouts     = this.sdUserProfile.Loadouts.Lookup.ToList();
            var index        = Math.Max(0, loadouts.FindIndex(g => g.Guid == current)) + 1;
            

            this.storageScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.Storage,
                Page = () => new StorageWithInventoryWidget {
                    OnClose            = () => StorageFeatureEvents.Close.Raise(),
                    OnIncrementLoadout = () => StorageFeatureEvents.IncrementLoadout.Raise(),
                    OnDecrementLoadout = () => StorageFeatureEvents.DecrementLoadout.Raise(),
                    HasSomeLoadouts    = loadoutCount > 1,
                    LoadoutIndex       = index,
                    LoadoutCount       = loadoutCount,
                },
            });
        }
        
        private async UniTask SwitchLoadout(Context context, int delta) {
            var loadouts = this.sdUserProfile.Loadouts.Lookup.ToList();
            
            if (loadouts.Count == 0) {
                return;
            }

            var current = this.sdUserProfile.Loadouts.SelectedLoadout.Value;
            var i       = Math.Max(0, loadouts.FindIndex(g => g.Guid == current));
            
            i = Math.Abs(i + delta) % loadouts.Count;
            
            var nextGuid = loadouts[i].Guid;

            await context.Server.ExecuteUserProfile(new UserProfileSelectLoadoutCommand {
                LoadoutGuid = nextGuid,
            }, ServerCallRetryStrategy.RetryWithUserDialog);

            await this.selectedItemInfoController.DisposeAsync();
            await this.storageScreen.DisposeAsync();
            await this.quantumSimulationController.DisposeAsyncNullable();
            await this.bgScreen.DisposeAsync();

            await this.Open(context);
            this.selectedItemInfoController = await context.RunDisposable(new SelectedItemInfoFeatureControllerArgs());
        }
        
        private async UniTask IncrementLoadout(Context context) {
            await this.SwitchLoadout(context, +1);
        }
        
        private async UniTask DecrementLoadout(Context context) {
            await this.SwitchLoadout(context, -1);
        }

        private async UniTask Close(Context context) {
            await this.selectedItemInfoController.DisposeAsync();
            await this.storageScreen.DisposeAsync();
            await this.quantumSimulationController.DisposeAsync();
            await this.bgScreen.DisposeAsync();
            
            this.Stop();
        }

        private GameSnapshotStorage CreateGameSnapshotStorage() {
            return new GameSnapshotStorage {
                items = this.sdUserProfile.Storage.Lookup.Select(it => {
                    var item = it.Item.Value;
                    if (item != null) {
                        item.IndexI = (byte)it.IndexI.Value;
                        item.IndexJ = (byte)it.IndexJ.Value;
                        item.Rotated = it.Rotated.Value;
                    }
                    return item;
                }).ToArray(),
            };
        }
    }
}