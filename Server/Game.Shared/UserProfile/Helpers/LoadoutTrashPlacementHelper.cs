namespace Game.Shared.UserProfile.Helpers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Balance;
    using Quantum;
    using Quantum.CellsInventory;

    public static class LoadoutTrashPlacementHelper {
        public const int DefaultLoadoutWidth = 5;
        public const int DefaultLoadoutHeight = 10;

        public static GameSnapshotLoadoutItem[] ArrangeTetrisItems(
            GameDef gameDef,
            List<string> itemSetupKeys,
            ItemSetupBalance itemSetupBalance) {
            
            return ArrangeTetrisItems(
                gameDef,
                itemSetupKeys,
                itemSetupBalance,
                DefaultLoadoutWidth,
                DefaultLoadoutHeight);
        }

        public static GameSnapshotLoadoutItem[] ArrangeTetrisItems(
            GameDef gameDef,
            List<string> itemSetupKeys,
            ItemSetupBalance itemSetupBalance,
            int loadoutWidth,
            int loadoutHeight) {
            
            if (itemSetupKeys == null || itemSetupKeys.Count == 0) {
                return Array.Empty<GameSnapshotLoadoutItem>();
            }

            var items = itemSetupKeys
                .Select(it => itemSetupBalance.MakeItemOrNull(it))
                .Where(it => it != null)
                .ToArray();

            if (items.Length == 0) {
                return Array.Empty<GameSnapshotLoadoutItem>();
            }

            var inventory = CellsInventoryAccess.Rent(loadoutWidth, loadoutHeight);

            try {
                foreach (var item in items) {
                    var itemDef = gameDef.Items.Get(item.ItemKey);
                    var itemSize = (width: itemDef.CellsWidth, height: itemDef.CellsHeight);

                    if (CellsInventoryMath.TryFindFreeSpaceForItem(
                        ref inventory,
                        itemSize,
                        out var place,
                        CellsInventoryRotationSearchMode.Find)) {

                        item.IndexI  = (byte)place.I;
                        item.IndexJ  = (byte)place.J;
                        item.Rotated = place.Rotated;

                        for (var i = place.MinI; i <= place.MaxI && i < loadoutHeight; i++) {
                            for (var j = place.MinJ; j <= place.MaxJ && j < loadoutWidth; j++) {
                                inventory[i, j] = true;
                            }
                        }
                    }
                }
            }
            finally {
                inventory.Dispose();
            }

            return items;
        }
    }
}
