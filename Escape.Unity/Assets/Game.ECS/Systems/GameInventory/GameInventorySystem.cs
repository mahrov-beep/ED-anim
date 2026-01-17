namespace Game.ECS.Systems.GameInventory {
    using Domain.GameInventory;
    using Domain.Safe;
    using JetBrains.Annotations;
    using Multicast;
    using Player;
    using Quantum;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class GameInventorySystem : SystemBase {
        [Inject] private GameInventoryModel gameInventoryModel;
        [Inject] private PhotonService      photonService;
        [Inject] private LocalPlayerSystem  localPlayerSystem;
        [Inject] private SafeModel          safeModel;

        public override void OnAwake() {
        }

        public override void Dispose() {
            base.Dispose();

            this.gameInventoryModel.CurrentItemWeight = 0;
            this.gameInventoryModel.LimitItemsWeight  = 1;

            this.gameInventoryModel.DeleteOutdated(int.MaxValue);
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return;
            }

            var frameNum = Time.frameCount;

            this.gameInventoryModel.CurrentItemWeight = loadout.GetTotalItemsWeight(f).AsFloat;
            this.gameInventoryModel.LimitItemsWeight  = loadout.GetTotalItemsWeightLimit(f).AsFloat;
            this.gameInventoryModel.LoadoutQuality    = loadout.GetLoadoutQuality(f);
            this.gameInventoryModel.UpdatedFrame      = loadout.UpdatedFrame;

            var loadoutParameters = loadout.GetLoadoutParameters(f);
            
            this.gameInventoryModel.LoadoutWidth  = loadoutParameters.width;
            this.gameInventoryModel.LoadoutHeight = loadoutParameters.height;

            // Safe sync (moved from SafeModelSyncController)
            if (f.TryGet(localRef, out CharacterSafe safe)) {
                this.safeModel.SetSize(safe.Width, safe.Height);
            }
            
            var safeSlotEntity = loadout.ItemAtSlot(CharacterLoadoutSlots.Safe);
            
            QGuid currentSafeGuid = default;
            
            if (safeSlotEntity != EntityRef.None) {
                currentSafeGuid = f.Get<Item>(safeSlotEntity).MetaGuid;
            }

            foreach (var slotType in CharacterLoadoutSlotsExtension.AllValidSlots) {
                var slotItemEntity = loadout.ItemAtSlot(slotType);
                if (slotItemEntity == EntityRef.None) {
                    continue;
                }

                var slotItem      = f.Get<Item>(slotItemEntity);
                var slotItemAsset = f.FindAsset(slotItem.Asset);

                var slotItemModel = this.gameInventoryModel.UpdateSlotItems(frameNum, slotType, slotItemEntity);

                slotItemModel.IsBlocked.Value       = !slotItemAsset.CanBeUnAssignedFromSlot(f, slotItemEntity, slotType, WeaponAttachmentSlots.Invalid, out _);
                slotItemModel.RemainingUsages.Value = Item.GetRemainingUsages(f, slotItemEntity);

                var weaponAttachments = this.gameInventoryModel.GetWeaponAttachmentsModel(slotType);
                if (weaponAttachments != null && f.TryGet(slotItemEntity, out WeaponItem weaponItem)) {
                    foreach (var weaponSlotType in WeaponAttachmentSlotsExtension.AllValidSlots) {
                        var weaponSlotItem = weaponItem.AttachmentAtSlot(weaponSlotType);
                        if (weaponSlotItem == EntityRef.None) {
                            continue;
                        }

                        var attachmentModel = weaponAttachments.UpdateSlotItems(frameNum, slotType, weaponSlotType, weaponSlotItem);

                        attachmentModel.RemainingUsages.Value = Item.GetRemainingUsages(f, weaponSlotItem);
                    }
                }
            }

            var trashItems = loadout.GetTrashItems(f);
            var safeItems = loadout.GetSafeItems(f);

            var trashIndex = 0;
            
            for (var index = 0; index < trashItems.Count; index++) {
                var itemRef = trashItems[index];
                var item    = f.Get<Item>(itemRef);
                var asset = f.FindAsset(item.Asset);
                
                var model = this.gameInventoryModel.UpdateTrashItem(frameNum, itemRef, trashIndex++);

                model.ItemGuid              = item.MetaGuid;
                model.ItemAsset             = asset;
                model.IndexI.Value          = item.IndexI;
                model.IndexJ.Value          = item.IndexJ;
                model.CanBeUsed.Value       = this.IsItemCamBeUsed(f, itemRef);
                model.RemainingUsages.Value = Item.GetRemainingUsages(f, itemRef);
                model.Weight.Value          = Item.GetItemWeight(f, itemRef).AsFloat;
                model.Rotated.Value         = item.Rotated;
            }

            var safeIndex = 0;
            
            for (var index = 0; index < safeItems.Count; index++) {
                var itemRef = safeItems[index];
                var item    = f.Get<Item>(itemRef);
                var asset = f.FindAsset(item.Asset);

                if (item.SafeGuid == currentSafeGuid) {
                    var safeItem = this.safeModel.UpdateItem(frameNum, itemRef, safeIndex++);

                    safeItem.ItemGuid              = item.MetaGuid;
                    safeItem.ItemAsset             = asset;
                    safeItem.IndexI.Value          = item.IndexI;
                    safeItem.IndexJ.Value          = item.IndexJ;
                    safeItem.Rotated.Value         = item.Rotated;
                    safeItem.CanBeUsed.Value       = this.IsItemCamBeUsed(f, itemRef);
                    safeItem.RemainingUsages.Value = Item.GetRemainingUsages(f, itemRef);
                    safeItem.Weight.Value          = Item.GetItemWeight(f, itemRef).AsFloat;
                    safeItem.IsFromSafe.Value      = true;
                }
            }

            this.gameInventoryModel.DeleteOutdated(frameNum);
            this.safeModel.DeleteOutdated(frameNum);
        }

        [PublicAPI]
        public bool IsEnoughSpaceForItem(EntityRef entityRef) {
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            return loadout.HasEnoughFreeSpaceForItem(f, entityRef);
        }
        
        [PublicAPI]
        public bool IsEnoughTetrisSpaceForItem(EntityRef entityRef, out CellsRange place, RotationType rotationType) {
            place = CellsRange.Empty;
            
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            return loadout.HasEnoughFreeTetrisSpaceForItem(f, entityRef, out place, rotationType);
        }

        [PublicAPI]
        public bool TryFindSlotForItem(EntityRef itemEntity,
            out CharacterLoadoutSlots slot, out WeaponAttachmentSlots weaponSlot) {
            slot       = CharacterLoadoutSlots.Invalid;
            weaponSlot = WeaponAttachmentSlots.Invalid;
            
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            return loadout.TryFindSlotForItem(f, itemEntity, out slot, out weaponSlot);
        }

        [PublicAPI]
        public bool TryGetItemAt(int i, int j, out EntityRef itemRef, byte source = 0) {
            itemRef = EntityRef.None;

            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            return loadout.TryGetItemAt(f, i, j, out itemRef, source);
        }

        [PublicAPI]
        public bool CanMerge(EntityRef sourceItemRef, EntityRef targetItemRef, out CellsRange mergeRange) {
            mergeRange = CellsRange.Empty;

            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            if (!loadout.CanMerge(f, sourceItemRef, targetItemRef)) {
                return false;
            }

            mergeRange = Item.GetItemTetris(f, targetItemRef, withRotation: true);
            return true;
        }

        [PublicAPI]
        public bool IsEnoughTetrisSpaceForItemAt(EntityRef entityRef, int i, int j, out CellsRange dropRange, RotationType rotationType, byte source) {
            dropRange = CellsRange.Empty;

            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            return loadout.CanBePlaceIn(f, entityRef, i, j, out dropRange, rotationType, source);
        }
        
        [PublicAPI]
        public bool HasItemAtSlot(CharacterLoadoutSlots slotType, out EntityRef entityRef) {
            entityRef = EntityRef.None;
            
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            if (loadout.HasItemAtSlot(slotType)) {
                entityRef = loadout.ItemAtSlot(slotType);
                return true;
            }

            return false;
        }
        
        [PublicAPI]
        public bool CanSwapItem(CharacterLoadoutSlots slot, EntityRef newItemRef) {
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            if (!loadout.HasItemAtSlot(slot)) {
                return true;
            }

            var oldItemRef = loadout.ItemAtSlot(slot);

            return loadout.CanTrashSwapItemAtSlot(f, slot, oldItemRef, newItemRef);
        }
        
        [PublicAPI]
        public bool CanSwapItemFromSafe(CharacterLoadoutSlots slot, EntityRef newItemRef) {
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }
            
            if (!f.TryGet(localRef, out CharacterSafe safe)) {
                return false;
            }

            if (!loadout.HasItemAtSlot(slot)) {
                return true;
            }

            var oldItemRef = loadout.ItemAtSlot(slot);

            return loadout.CanTrashSwapItemAtSlotFromSafe(f, slot, oldItemRef, newItemRef);
        }

        [PublicAPI]
        public bool CanAssignToLoadoutSlot(EntityRef itemEntity, CharacterLoadoutSlots slot, CharacterLoadout.AssignOptions options) {
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            return loadout.CanAssignItemToSlot(f, slot, itemEntity, options);
        }

        [PublicAPI]
        public bool CanAssignToWeaponSlot(EntityRef attachmentItemEntity, CharacterLoadoutSlots slot, WeaponAttachmentSlots weaponSlot,
            CharacterLoadout.AssignOptions options) {
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            if (!f.TryGet(loadout.ItemAtSlot(slot), out WeaponItem targetWeaponItem)) {
                return false;
            }

            return targetWeaponItem.CanAssignAttachmentToSlot(f, weaponSlot, attachmentItemEntity, options);
        }

        private bool IsItemCamBeUsed(Frame f, EntityRef itemEntity) {
            if (!f.Exists(itemEntity) || !f.TryGet(itemEntity, out Item item)) {
                return false;
            }

            var itemAsset = f.FindAsset(item.Asset);
            return itemAsset is UsableItemAsset usableItemAsset && usableItemAsset.CanBeUsed(f, itemEntity);
        }
        
        [PublicAPI]
        public bool IsBusyAt(int i, int j) {
            if (this.photonService.PredictedFrame is not { } f) {
                return false;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return false;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return false;
            }

            return loadout.TryGetItemAt(f, i, j, out _);
        }
    }
}