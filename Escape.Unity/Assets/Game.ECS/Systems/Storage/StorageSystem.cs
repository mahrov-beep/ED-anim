namespace Game.ECS.Systems.Storage {
    using Domain.ItemBoxStorage;
    using Multicast;
    using Player;
    using Quantum;
    using Services.Photon;
    using Shared.UserProfile.Data;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class ItemBoxStorageSystem : SystemBase {
        [Inject] private PhotonService       photonService;
        [Inject] private LocalPlayerSystem   localPlayerSystem;
        [Inject] private ItemBoxStorageModel itemBoxStorageModel;
        [Inject] private SdUserProfile       sdUserProfile;

        public override void OnAwake() { }

        public override void Dispose() {
            base.Dispose();
            this.itemBoxStorageModel.DeleteOutdated(int.MaxValue);
        }

        public override void OnUpdate(float deltaTime) { 
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (this.localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return;
            }

            if (!f.TryGet(localRef, out Unit unit)) {
                return;
            }

            var frameNum = UnityEngine.Time.frameCount;

            var nearbyRef = loadout.StorageEntity;

            if (nearbyRef != EntityRef.None && f.TryGet(nearbyRef, out ItemBox itemBox)) {
                this.itemBoxStorageModel.SetSize(itemBox.Width, itemBox.Height);
                
                if (itemBox.Width > 0 && itemBox.Height > 0) {
                    this.itemBoxStorageModel.UpdatedFrame = loadout.UpdatedFrame;

                    var itemRefs = loadout.GetStorageItems(f);
                    var storageIndex = 0;

                    foreach (var itemRef in itemRefs) {
                        if (!f.Exists(itemRef)) {
                            continue;
                        }

                        var item  = f.Get<Item>(itemRef);
                        var asset = f.FindAsset(item.Asset);

                        var storageItem = this.itemBoxStorageModel.UpdateItem(frameNum, itemRef, storageIndex++);

                        storageItem.ItemGuid = item.MetaGuid;
                        storageItem.ItemAsset = asset;
                        storageItem.IndexI.Value = item.IndexI;
                        storageItem.IndexJ.Value = item.IndexJ;
                        storageItem.Rotated.Value = item.Rotated;
                        storageItem.CanBeUsed.Value = false;
                        storageItem.RemainingUsages.Value = Item.GetRemainingUsages(f, itemRef);
                        storageItem.Weight.Value = Item.GetItemWeight(f, itemRef).AsFloat;
                        storageItem.IsFromSafe.Value = false;
                    }
                } 
                else {
                    this.itemBoxStorageModel.UpdatedFrame = frameNum;
                }
            } 
            else {
                this.itemBoxStorageModel.SetSize(0, 0);
                this.itemBoxStorageModel.UpdatedFrame = frameNum;
            }

            this.itemBoxStorageModel.DeleteOutdated(frameNum);
        }
    }
}

