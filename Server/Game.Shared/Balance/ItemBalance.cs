namespace Game.Shared.Balance {
    using System;
    using Defs;
    using Multicast.Numerics;
    using Quantum;

    public static class ItemBalance {
        public static int GetLoadoutQuality(GameDef gameDef, GameSnapshotLoadout loadout) {
            if (loadout.SlotItems == null) {
                return 0;
            }

            var quality = 0;

            foreach (var slot in CharacterLoadoutSlotsExtension.UsedForQualitySlots) {
                if (slot.ToInt() < loadout.SlotItems.Length && loadout.SlotItems[slot.ToInt()] is { } itemAtSlot) {
                    var itemDef = gameDef.Items.Get(itemAtSlot.ItemKey);

                    quality += itemDef.Quality;
                }
            }

            return quality;
        }

        public static IntCost CalculateSellCost(GameDef gameDef, GameSnapshotLoadout loadout) {
            var cost = IntCost.Empty;

            if (loadout.SlotItems != null) {
                foreach (var slotItem in loadout.SlotItems) {
                    if (slotItem != null) {
                        cost += CalculateSellCost(gameDef, slotItem);
                    }
                }
            }

            if (loadout.TrashItems != null) {
                foreach (var trashItem in loadout.TrashItems) {
                    if (trashItem != null) {
                        cost += CalculateSellCost(gameDef, trashItem);
                    }
                }
            }

            return cost;
        }

        public static IntCost CalculateSellCost(GameDef gameDef, GameSnapshotLoadoutItem item) {
            return CalculateCost(gameDef, item, static it => new IntCost(it.SellCost));
        }

        public static IntCost CalculateBuyCost(GameDef gameDef, GameSnapshotLoadoutItem item) {
            return CalculateCost(gameDef, item, static it => new IntCost(it.BuyCost));
        }

        private static IntCost CalculateCost(GameDef gameDef, GameSnapshotLoadoutItem item, Func<ItemDef, IntCost> selector) {
            var cost = gameDef.Items.TryGet(item.ItemKey, out var itemDef) ? selector(itemDef) : IntCost.Empty;

            if (item.WeaponAttachments != null) {
                foreach (var attachment in item.WeaponAttachments) {
                    if (attachment != null && !string.IsNullOrEmpty(attachment.ItemKey) && gameDef.Items.TryGet(attachment.ItemKey, out var attachmentDef)) {
                        cost += selector(attachmentDef);
                    }
                }
            }

            return cost;
        }
    }
}