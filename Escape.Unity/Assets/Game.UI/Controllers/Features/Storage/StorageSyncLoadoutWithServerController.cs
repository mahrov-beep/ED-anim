namespace Game.UI.Controllers.Loadout {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using ECS.Systems.Player;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using Shared.UserProfile.Commands.Loadouts;
    using Shared.UserProfile.Commands.Storage;
    using Shared.UserProfile.Data;

    [Serializable, RequireFieldsInit]
    public struct StorageSyncLoadoutWithServerControllerArgs : IFlowControllerArgs {
    }

    public class StorageSyncLoadoutWithServerController : FlowController<StorageSyncLoadoutWithServerControllerArgs> {
        [Inject] private PhotonService     photonService;
        [Inject] private LocalPlayerSystem localPlayerSystem;
        [Inject] private SdUserProfile     gameData;

        protected override async UniTask Activate(Context context) {
            this.Lifetime.Register(QuantumEvent.SubscribeManual<EventLocalCharacterLoadoutModified>(this.OnLocalLoadoutModified));
        }

        private void OnLocalLoadoutModified(EventLocalCharacterLoadoutModified evt) {
            if (evt.Game.Frames.Predicted.GameMode.rule != GameRules.MainMenuStorage) {
                return;
            }

            if (this.photonService.VerifiedFrame is not { } f) {
                return;
            }

            if (this.localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            var selectedLoadoutGuid = this.gameData.Loadouts.SelectedLoadout.Value;

            var args = new LoadoutSyncArgs {
                loadoutGuid     = selectedLoadoutGuid,
                loadoutSnapshot = GameSnapshotHelper.MakeLoadout(f, localRef),
                modifiedItems   = evt.ModifiedItems.ToList(),
                snapshotStorage = this.MakeStorageSnapshot(),
            };
            
            this.RequestFlow(this.SyncLoadout, args);
        }

        private async UniTask SyncLoadout(Context context, LoadoutSyncArgs args) {
            // Сначала обновляем loadout (который может удалить предметы из storage при переносе в инвентарь)
            await context.Server.ExecuteUserProfile(new UserProfileModifyLoadoutCommand {
                LoadoutGuid       = args.loadoutGuid,
                LoadoutSnapshot   = args.loadoutSnapshot,
                ModifiedItemGuids = args.modifiedItems,
            }, ServerCallRetryStrategy.RetryWithUserDialog);
            
            // Затем синхронизируем storage с актуальными координатами из ItemBox
            if (args.snapshotStorage != null) {
                await context.Server.ExecuteUserProfile(new UserProfileSyncStorageCommand {
                    StorageSnapshot = args.snapshotStorage,
                    LoadoutGuid     = this.gameData.Loadouts.SelectedLoadout.Value,
                }, ServerCallRetryStrategy.RetryWithUserDialog);
            }
        }
        
        private unsafe GameSnapshotStorage MakeStorageSnapshot() {
            if (this.photonService.PredictedFrame is not { } f) {
                return null;
            }
            
            if (this.localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return null;
            }
            
            if (!f.TryGet(localRef, out Unit unit) || unit.NearbyItemBox == EntityRef.None) {
                return null;
            }
            
            if (!f.Unsafe.TryGetPointer<CharacterLoadout>(localRef, out var loadout)) {
                return null;
            }
            
            if (!f.TryGetPointer<ItemBox>(unit.NearbyItemBox, out var itemBox)) {
                return null;
            }
            
            if (itemBox->Width > 0 && itemBox->Height > 0) {
                var runtimeStorage = itemBox->LoadToRuntimeStorage(f);
                this.UpdateUserProfileStorage(runtimeStorage);

                return runtimeStorage;
            }

            return null;
        }
        
        private void UpdateUserProfileStorage(GameSnapshotStorage runtimeStorage) {
            var items = runtimeStorage?.items;
            var storageItemsList = this.gameData.Storage.Lookup.ToList();
            
            for (var i = storageItemsList.Count - 1; i >= 0; i--) {
                this.gameData.Storage.Lookup.Remove(storageItemsList[i].ItemGuid);
            }
            
            if (items == null || items.Length == 0) {
                return;
            }
            
            foreach (var item in items) {
                if (item == null || string.IsNullOrEmpty(item.ItemGuid)) {
                    continue;
                }
                
                var storageItem = this.gameData.Storage.Lookup.GetOrCreate(item.ItemGuid, out _);
                storageItem.Item.Value    = item;
                storageItem.IndexI.Value  = item.IndexI;
                storageItem.IndexJ.Value  = item.IndexJ;
                storageItem.Rotated.Value = item.Rotated;
            }
        }

        [Serializable, RequireFieldsInit]
        private struct LoadoutSyncArgs {
            public string              loadoutGuid;
            public GameSnapshotLoadout loadoutSnapshot;
            public List<string>        modifiedItems;
            public GameSnapshotStorage snapshotStorage;
        }
    }
}