namespace Game.UI.Widgets.GameInventory.Snapshot {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Multicast;
    using Quantum;
    using Storage;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class SnapshotGameInventoryLoadoutHocWidget : StatefulWidget {
        public GameSnapshotLoadout LoadoutSnapshot;

        public CharacterLoadoutSlots[] VisibleSlots;

        public List<GameSnapshotLoadoutItem> ExtraItems;
    }

    public class SnapshotGameInventoryLoadoutHocState : HocState<SnapshotGameInventoryLoadoutHocWidget> {
        public override Widget Build(BuildContext context) {
            var slots = this.Widget.LoadoutSnapshot.SlotItems ?? Array.Empty<GameSnapshotLoadoutItem>();

            return new GridFlow {
                MaxCrossAxisExtent = 600 + 20,
                ChildrenBuilder = () => this.Widget.VisibleSlots
                    .Select(slotType => slotType.ToInt() < slots.Length && slots[slotType.ToInt()] is { } item
                        ? this.BuildSlotItem(slotType, item)
                        : this.BuildEmptySlotItem(slotType))
                    .Concat(this.Widget.ExtraItems.Select(it => this.BuildExtraItem(it)))
                    .ToList(),
            };
        }

        private Widget BuildEmptySlotItem(CharacterLoadoutSlots slotType) {
            return new GameInventoryEmptySlotItemWidget {
                Key      = Key.Of(EnumNames<CharacterLoadoutSlots>.GetName(slotType)),
                SlotType = slotType,
            };
        }

        private Widget BuildSlotItem(CharacterLoadoutSlots slotType, GameSnapshotLoadoutItem item) {
            return new SnapshotGameInventorySlotItemWidget {
                SlotType     = slotType,
                ItemSnapshot = item,

                Key = Key.Of(slotType),
            };
        }

        private Widget BuildExtraItem(GameSnapshotLoadoutItem item) {
            return new StorageItemSimpleWidget {
                ItemKey = item.ItemKey,
                Details = new Empty(),

                Key = Key.Of(item!.ItemGuid!),
            };
        }
    }
}