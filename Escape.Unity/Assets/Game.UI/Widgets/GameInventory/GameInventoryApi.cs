namespace Game.UI.Widgets.GameInventory {
    using Domain.GameInventory;
    using Domain.Safe;
    using Domain.TraderShop;
    using ECS.Systems.GameInventory;
    using ECS.Systems.Player;
    using Multicast;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using UnityEngine;
    using Views;

    public class GameInventoryApi {
        [Inject] private PhotonService       photonService;
        [Inject] private GameInventorySystem gameInventorySystem;
        [Inject] private GameInventoryModel  gameInventoryModel;
        [Inject] private SafeModel           safeModel;
        [Inject] private LocalPlayerSystem   localPlayerSystem;
        [Inject] private TraderShopModel     traderShopModel;

        public bool CanThrowItem(DragAndDropPayloadItem payload) {
            if (payload is DragAndDropPayloadItemEntityFromTetris tetris) {
                if (tetris.Source == TetrisSource.Storage) {
                    return false;
                }
            }

            return true;
        }

        public void ThrowItem(DragAndDropPayloadItem payload) {
            switch (payload) {
                case DragAndDropPayloadItemEntityFromTetris fromTetris:
                    this.photonService.Runner?.Game.SendCommand(new ThrowAwayItemFromTetrisLoadoutCommand {
                        ItemEntity = fromTetris.ItemEntity,
                        Source     = (byte)fromTetris.Source,
                    });
                    break;

                case DragAndDropPayloadItemEntityFromSlot fromSlot:
                    this.photonService.Runner?.Game.SendCommand(new ThrowAwayItemFromSlotLoadoutCommand {
                        OldSlotType = fromSlot.SourceSlot,
                        ItemEntity  = fromSlot.ItemEntity,
                    });
                    break;

                case DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot:
                    this.photonService.Runner?.Game.SendCommand(new ThrowAwayWeaponAttachmentFromSlotLoadoutCommand {
                        OldSlotType       = fromWeaponSlot.SourceSlot,
                        OldWeaponSlotType = fromWeaponSlot.SourceWeaponAttachmentSlot,
                        ItemEntity        = fromWeaponSlot.ItemEntity,
                    });
                    break;

                default:
                    Debug.LogError($"Cannot throw away item from source={payload?.GetType()}");
                    break;
            }
        }
        
        public bool IsEnoughSpace(DragAndDropPayloadItem payload) {
            var entity = payload switch {
                DragAndDropPayloadItemEntityFromSlot fromSlot => fromSlot.ItemEntity,
                DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot => fromWeaponSlot.ItemEntity,
                _ => EntityRef.None,
            };

            if (entity == EntityRef.None) {
                Debug.LogError($"GameInventoryApi.IsEnoughSpace: bad source={payload?.GetType().Name}");
            }

            return this.gameInventorySystem.IsEnoughSpaceForItem(entity);
        }
        
        public bool IsEnoughSpaceTetris(EntityRef entity, out CellsRange place, RotationType rotationType) {
            return this.gameInventorySystem.IsEnoughTetrisSpaceForItem(entity, out place, rotationType);
        }

        public bool TryFindSlotForItem(EntityRef itemEntity, 
            out CharacterLoadoutSlots slot, out WeaponAttachmentSlots weaponSlot) {
            return this.gameInventorySystem.TryFindSlotForItem(itemEntity, out slot, out weaponSlot);
        }

        public bool CanMergeInto(DragAndDropPayloadItem payload, int i, int j, out CellsRange mergeRange, byte source) {
            mergeRange = CellsRange.Empty;

            switch (payload) {
                case DragAndDropPayloadItemEntityFromTetris fromTetris:
                    return this.gameInventorySystem.TryGetItemAt(i, j, out var possibleMergeTarget, source) &&
                           this.gameInventorySystem.CanMerge(fromTetris.ItemEntity, possibleMergeTarget, out mergeRange);
                default:
                    return false;
            }
        }

        public bool IsEnoughSpaceTetrisAt(DragAndDropPayloadItem payload, int i, int j, out CellsRange dropRange, RotationType rotationType, byte source) {
            var entity = payload switch {
                DragAndDropPayloadItemEntityFromSlot fromSlot => fromSlot.ItemEntity,
                DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot => fromWeaponSlot.ItemEntity,
                DragAndDropPayloadItemEntityFromTetris fromTetris => fromTetris.ItemEntity,
                DragAndDropPayloadItemFromTraderShopToSell fromToSell => fromToSell.ItemEntity,
                _ => EntityRef.None,
            };

            if (entity == EntityRef.None) {
                Debug.LogError($"GameInventoryApi.IsEnoughSpaceTetrisAt: bad source={payload?.GetType().Name}");
                dropRange = CellsRange.Empty;
                return false;
            }

            return this.gameInventorySystem.IsEnoughTetrisSpaceForItemAt(entity, i, j, out dropRange, rotationType, source);
        }

        public bool IsBusyAt(int i, int j) {
            return this.gameInventorySystem.IsBusyAt(i, j);
        }

        public (int width, int height, RotationType rotationType) GetMetrics(DragAndDropPayloadItem payload, byte source) {
            var entity = payload switch {
                DragAndDropPayloadItemEntityFromSlot fromSlot => fromSlot.ItemEntity,
                DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot => fromWeaponSlot.ItemEntity,
                DragAndDropPayloadItemEntityFromTetris fromTetris => fromTetris.ItemEntity,
                DragAndDropPayloadItemFromTraderShopToSell fromSell => fromSell.ItemEntity,
                _ => EntityRef.None,
            };

            if (entity == EntityRef.None) {
                if (payload is DragAndDropPayloadItemEntityFromTetris fromTetris) {
                    if (!this.safeModel.TryGetItem(fromTetris.ItemEntity, out var safeItem)) {
                        return (0, 0, RotationType.Default);
                    }

                    return (safeItem.ItemAsset.Width, safeItem.ItemAsset.Height, RotationType.Default);
                }

                Debug.LogError($"GameInventoryApi.GetMetrics: bad source={payload?.GetType().Name}");
                return (0, 0, RotationType.Default);
            }

            var item      = this.photonService.PredictedFrame!.Get<Item>(entity);
            var itemAsset = this.photonService.PredictedFrame!.FindAsset(item.Asset);

            return (
                width: itemAsset.Width,
                height: itemAsset.Height,
                rotationType: item.Rotated ? RotationType.Rotated : RotationType.Default
            );
        }
        
        public void MoveItemToTrash(DragAndDropPayloadItem payload, int indexI, int indexJ, bool rotated, byte destination) {
            switch (payload) {
                case DragAndDropPayloadItemEntityFromSlot fromSlot:
                    this.photonService.Runner?.Game.SendCommand(new MoveItemFromSlotToTetrisLoadoutCommand {
                        OldSlotType       = fromSlot.SourceSlot,
                        ItemEntity        = fromSlot.ItemEntity,
                        IndexI            = indexI,
                        IndexJ            = indexJ,
                        Rotated           = rotated,
                        DestinationSource = destination,
                    });
                    break;

                case DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot:
                    this.photonService.Runner?.Game.SendCommand(new MoveWeaponAttachmentFromSlotToTetrisLoadoutCommand {
                        OldSlotType       = fromWeaponSlot.SourceSlot,
                        OldWeaponSlotType = fromWeaponSlot.SourceWeaponAttachmentSlot,
                        ItemEntity        = fromWeaponSlot.ItemEntity,
                        IndexInTrashI     = indexI,
                        IndexInTrashJ     = indexJ,
                        Rotated           = rotated,
                        DestinationSource = destination,
                        
                    });
                    break;
                case DragAndDropPayloadItemEntityFromTetris fromTetris:
                    this.photonService.Runner?.Game.SendCommand(new MoveItemFromTetrisToTetrisLoadoutCommand {
                        ItemEntity  = fromTetris.ItemEntity,
                        IndexI      = indexI,
                        IndexJ      = indexJ,
                        Rotated     = rotated,
                        Source      = (byte)fromTetris.Source,
                        Destination = destination,
                    });
                    break;

                case DragAndDropPayloadItemFromTraderShopToSell fromSell:
                    this.traderShopModel.RemoveToSellGuid(fromSell.ItemGuid);
                    
                    this.photonService.Runner?.Game.SendCommand(new MoveItemFromTetrisToTetrisLoadoutCommand {
                        ItemEntity        = fromSell.ItemEntity,
                        IndexI            = indexI,
                        IndexJ            = indexJ,
                        Rotated           = rotated,
                        Destination       = (byte)TetrisSource.Storage,
                        Source            = (byte)TetrisSource.Storage,
                    });
                    break;

                default:
                    Debug.LogError($"Cannot move item to trash from source={payload?.GetType().Name}");
                    break;
            }
        }

        public bool CanAssignItemToSlot(DragAndDropPayloadItem payload, CharacterLoadoutSlots slot) {
            var opt = CharacterLoadout.AssignOptions.SKipSlotAlreadyAssignedCheck;

            foreach (var weaponSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
                if (this.CanAssignItemToWeaponSlot(payload, slot, weaponSlot)) {
                    return true;
                }
            }

            return payload is DragAndDropPayloadItemEntityFromSlot fromSlot && this.gameInventorySystem.CanAssignToLoadoutSlot(fromSlot.ItemEntity, slot, opt) ||
                   payload is DragAndDropPayloadItemEntityFromTetris fromTetris && this.gameInventorySystem.CanAssignToLoadoutSlot(fromTetris.ItemEntity, slot, opt);
        }

        public bool CanSwapItem(CharacterLoadoutSlots slot, EntityRef newItemRef) {
            return this.gameInventorySystem.CanSwapItem(slot, newItemRef);
        }

        public bool HasItemAtSlot(CharacterLoadoutSlots slot, out EntityRef entityRef) {
            return this.gameInventorySystem.HasItemAtSlot(slot, out entityRef);
        }

        public void AssignItemToSlot(DragAndDropPayloadItem payload, CharacterLoadoutSlots slot) {
            foreach (var weaponSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
                if (this.CanAssignItemToWeaponSlot(payload, slot, weaponSlot)) {
                    this.AssignItemToWeaponSlot(payload, slot, weaponSlot);
                    return;
                }
            }

            switch (payload) {
                case DragAndDropPayloadItemEntityFromTetris fromTetris:
                    this.photonService.Runner?.Game.SendCommand(new MoveItemFromTetrisToSlotLoadoutCommand {
                        NewSlotType = slot,
                        ItemEntity  = fromTetris.ItemEntity,
                        FromSource  = (byte)fromTetris.Source,
                    });
                    break;

                case DragAndDropPayloadItemEntityFromSlot fromSlot:
                    this.photonService.Runner?.Game.SendCommand(new MoveItemFromSlotToSlotLoadoutCommand {
                        OldSlotType = fromSlot.SourceSlot,
                        NewSlotType = slot,
                        ItemEntity  = fromSlot.ItemEntity,
                    });
                    break;

                default:
                    Debug.LogError($"Cannot assign item to slot from source={payload?.GetType().Name}");
                    break;
            }
        }

        public bool CanAssignItemToWeaponSlot(DragAndDropPayloadItem payload, CharacterLoadoutSlots slot, WeaponAttachmentSlots weaponSlot) {
            var opt = CharacterLoadout.AssignOptions.SKipSlotAlreadyAssignedCheck;

            return payload is DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot && this.gameInventorySystem.CanAssignToWeaponSlot(fromWeaponSlot.ItemEntity, slot, weaponSlot, opt) ||
                   payload is DragAndDropPayloadItemEntityFromTetris fromTetris && this.gameInventorySystem.CanAssignToWeaponSlot(fromTetris.ItemEntity, slot, weaponSlot, opt);
        }

        public void AssignItemToWeaponSlot(DragAndDropPayloadItem payload, CharacterLoadoutSlots slot, WeaponAttachmentSlots weaponSlot) {
            switch (payload) {
                case DragAndDropPayloadItemEntityFromTetris fromTetris:
                    this.photonService.Runner?.Game.SendCommand(new MoveWeaponAttachmentFromTetrisToSlotLoadoutCommand {
                        NewSlotType       = slot,
                        NewWeaponSlotType = weaponSlot,
                        ItemEntity        = fromTetris.ItemEntity,
                        Source            = (byte)fromTetris.Source,
                    });
                    break;

                case DragAndDropPayloadItemEntityFromWeaponSlot fromWeaponSlot:
                    this.photonService.Runner?.Game.SendCommand(new MoveWeaponAttachmentFromSlotToSlotLoadoutCommand {
                        OldSlotType       = fromWeaponSlot.SourceSlot,
                        OldWeaponSlotType = fromWeaponSlot.SourceWeaponAttachmentSlot,
                        NewSlotType       = slot,
                        NewWeaponSlotType = weaponSlot,
                        ItemEntity        = fromWeaponSlot.ItemEntity,
                    });
                    break;

                default:
                    Debug.LogError($"Cannot assign item to weapon slot from source={payload?.GetType().Name}");
                    break;
            }
        }

        public unsafe void FastItemMoveToTetris(EntityRef itemEntity, TetrisSource currentSource, bool inShop) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (this.localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            var item = f.Get<Item>(itemEntity);

            if (inShop) {
                this.traderShopModel.AddToSellGuid(item.MetaGuid);
                
                return;
            }
            
            if (!f.Unsafe.TryGetPointer<CharacterLoadout>(localRef, out var loadout)) {
                return;
            }

            loadout->TryFindSlotForItem(f, itemEntity, out var slot, out var weaponSlot);
            
            var destination = (byte)(currentSource == TetrisSource.Inventory ? TetrisSource.Storage : TetrisSource.Inventory);

            if (destination == (byte)TetrisSource.Storage && loadout->StorageEntity == EntityRef.None) {
                if (slot != CharacterLoadoutSlots.Invalid && weaponSlot != WeaponAttachmentSlots.Invalid) {
                    this.photonService.Runner?.Game.SendCommand(new MoveWeaponAttachmentFromTetrisToSlotLoadoutCommand {
                        ItemEntity        = itemEntity,
                        NewSlotType       = slot,
                        NewWeaponSlotType = weaponSlot,
                        Source            = (byte)currentSource,
                    });
                }
                else if (slot != CharacterLoadoutSlots.Invalid) {
                    this.photonService.Runner?.Game.SendCommand(new MoveItemFromTetrisToSlotLoadoutCommand {
                        ItemEntity  = itemEntity,
                        NewSlotType = slot,
                        FromSource  = (byte)currentSource,
                    });
                }

                return;
            }

            this.photonService.Runner?.Game.SendCommand(new SwapTetrisCommand {
                ItemEntity        = itemEntity,
                Slot              = slot,
                WeaponSlot        = weaponSlot,
                DestinationSource = destination,
            });
        }
    }
}