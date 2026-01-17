namespace Game.UI.Controllers.Features.TraderShop {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Domain;
    using Domain.TraderShop;
    using ECS.Systems.Player;
    using JetBrains.Annotations;
    using Loadout;
    using Multicast;
    using Multicast.Routes;
    using Photon;
    using Quantum;
    using Quantum.Commands;
    using SelectedItemInfo;
    using Services.Photon;
    using Shared;
    using Shared.UserProfile.Commands.Features;
    using Shared.UserProfile.Commands.Storage;
    using Shared.UserProfile.Commands.TraderShop;
    using Shared.UserProfile.Data;
    using Sound;
    using SoundEffects;
    using UI;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using Widgets;

    [Serializable, RequireFieldsInit]
    public struct TraderShopControllerArgs : IFlowControllerArgs {
    }

    public class TraderShopController : FlowController<TraderShopControllerArgs> {
        [Inject] private SdUserProfile       sdUserProfile;
        [Inject] private TraderShopModel     traderShopModel;
        [Inject] private ISoundEffectService soundEffectService;
        [Inject] private PhotonService       photonService;
        [Inject] private LocalPlayerSystem   localPlayerSystem;

        private IDisposableController   quantumSimulationController;
        private IUniTaskAsyncDisposable traderShopScreen;
        private IUniTaskAsyncDisposable bgScreen;
        private IUniTaskAsyncDisposable selectedItemInfoController;

        protected override async UniTask Activate(Context context) {
            await this.Open(context);

            await context.RunChild(new StorageSyncLoadoutWithServerControllerArgs());

            this.selectedItemInfoController = await context.RunDisposable(new SelectedItemInfoFeatureControllerArgs());

            TraderShopFeatureEvents.Close.Listen(this.Lifetime, () => this.RequestFlow(this.Close));
            TraderShopFeatureEvents.Sell.Listen(this.Lifetime, () => this.RequestFlow(this.Sell));
        }

        private async UniTask Open(Context context) {
            var selectedLoadout = this.sdUserProfile.Loadouts.Get(this.sdUserProfile.Loadouts.SelectedLoadout.Value);

            if (!string.IsNullOrEmpty(selectedLoadout.LockedForGame.Value)) {
                Debug.LogError($"[{this}] Trying to open trader shop but Selected loadout is not in Available state");
                return;
            }

            this.bgScreen = await context.RunBgScreenDisposable();
            await context.RunChild(new BackgroundAudioLowPassActivationControllerArgs());

            await using (await context.RunProgressScreenDisposable("fetching_items")) {
                await context.Server.ExecuteUserProfile(new UserProfileTraderShopRefreshCommand(), ServerCallRetryStrategy.RetryWithUserDialog);

                await context.Server.ExecuteUserProfile(new UserProfileViewFeatureCommand {
                    FeatureKey = SharedConstants.Game.Features.TRADER_SHOP,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }

            this.quantumSimulationController = await context.RunDisposable(new PhotonLocalSimulationControllerArgs {
                GameModeAssetPath = CoreConstants.Quantum.GameModeAssets.MAIN_MENU_STORAGE,
                UnitySceneName    = CoreConstants.Scenes.MAIN_MANU_STORAGE_ADDITIVE,
                Loadout           = this.sdUserProfile.Loadouts.GetSelectedLoadoutClone(),
                Storage           = this.CreateGameSnapshotStorage(),
            });

            this.traderShopScreen = await context.RunDisposable(new UiScreenControllerArgs {
                Route = UiConstants.Routes.TraderShop,
                Page = () => new StorageWithTraderShopWidget {
                    OnClose = () => this.RequestFlow(this.Close),
                },
            });
        }

        private async UniTask Close(Context context) {
            await this.selectedItemInfoController.DisposeAsync();
            this.traderShopModel.Cleanup();
            await this.traderShopScreen.DisposeAsync();
            await this.quantumSimulationController.DisposeAsync();
            await this.bgScreen.DisposeAsync();
            this.Stop();
        }

        private GameSnapshotStorage CreateGameSnapshotStorage() {
            return new GameSnapshotStorage {
                items = this.sdUserProfile.Storage.Lookup.Select(it => {
                    var item = it.Item.Value;
                    if (item != null) {
                        item.IndexI  = (byte)it.IndexI.Value;
                        item.IndexJ  = (byte)it.IndexJ.Value;
                        item.Rotated = it.Rotated.Value;
                    }
                    return item;
                }).ToArray(),
            };
        }

        private async UniTask Sell(Context context) {
            await using (await context.RunFadeScreenDisposable())
            await using (await context.RunProgressScreenDisposable("selling")) {
                await context.Server.ExecuteUserProfile(new UserProfileTraderShopMakeDealCommand {
                    ItemGuidsToSell = this.traderShopModel.EnumerateToSellGuids(),
                    ItemGuidsToBuy  = this.traderShopModel.EnumerateToBuyGuids(),
                }, ServerCallRetryStrategy.RetryWithUserDialog);
                
                this.traderShopModel.CleanupToBuy();

                this.soundEffectService.PlayOneShot(CoreConstants.SoundEffectKeys.DEAL_ITEM);

                await context.Server.ExecuteUserProfile(new UserProfileTraderShopRefreshCommand(), ServerCallRetryStrategy.RetryWithUserDialog);
                
                await UniTask.Yield();
                
                this.ReloadItemBox();
                
                await UniTask.Yield();

                this.traderShopModel.CleanupToSell();
            }
        }

        private void ReloadItemBox() {
            var game = QuantumRunner.DefaultGame;
            if (game == null) {
                return;
            }

            var storageSnapshot = this.CreateGameSnapshotStorage();

            ReloadItemBoxCommandHelper.SetPendingStorageData(
                storageSnapshot,
                this.sdUserProfile.StorageWidth.Value,
                this.sdUserProfile.StorageHeight.Value
            );

            game.SendCommand(new ReloadItemBoxCommand());
        }
    }
}