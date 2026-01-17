namespace Game.Shared.UserProfile.Helpers {
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using Data;
    using Quantum;
    using Quantum.CellsInventory;

    public static class StoragePlacementHelper {
        private struct ItemWithIndex {
            public int Index;
            public CellsRange Range;
            public int Area;
        }

        public static bool TryFindPlaceInStorage(
            GameDef gameDef,
            SdUserProfile gameData,
            CellsRange[] newItems,
            out CellsRange[] outRanges,
            HashSet<string> exceptItems = default) {
            
            var count = gameData.Storage.Lookup.Count + newItems.Length;
            var items = ArrayPool<CellsRange>.Shared.Rent(count);
            
            outRanges = new CellsRange[newItems.Length];

            var i = 0;
            
            foreach (var item in gameData.Storage.Lookup) {
                if (!gameDef.Items.TryGet(item.Item.Value.ItemKey, out var currentItemDef)) {
                    ArrayPool<CellsRange>.Shared.Return(items, true);
                    return false;
                }

                if (exceptItems != default && exceptItems.Contains(item.ItemGuid)) {
                    continue;
                }

                var width  = item.Rotated.Value ? currentItemDef.CellsHeight : currentItemDef.CellsWidth;
                var height = item.Rotated.Value ? currentItemDef.CellsWidth : currentItemDef.CellsHeight;
                
                items[i++] = CellsRange.FromIJWH(item.IndexI.Value, item.IndexJ.Value, width, height, item.Rotated.Value);
            }
            
            var sortedItems = new ItemWithIndex[newItems.Length];
            for (var j = 0; j < newItems.Length; j++) {
                sortedItems[j] = new ItemWithIndex {
                    Index = j,
                    Range = newItems[j],
                    Area = newItems[j].Width * newItems[j].Height,
                };
            }
            
            Array.Sort(sortedItems, (a, b) => b.Area.CompareTo(a.Area));
            
            var grid = CellsInventoryAccess.Rent((gameData.StorageWidth.Value, gameData.StorageHeight.Value));

            try {
                for (var j = 0; j < sortedItems.Length; j++) {
                    CellsInventoryMath.ClearLoadout(ref grid, gameData.StorageWidth.Value, gameData.StorageHeight.Value);
                    CellsInventoryMath.FillLoadout(ref grid, items.AsSpan(0, i + j).ToArray());
                    
                    for (var index = 0; index < grid.Height; index++) {
                        var str = "";

                        for (var indexW = 0; indexW < grid.Width; indexW++) {
                            str += grid.Array[index * grid.Width + indexW].ToString();
                        }
                    }
                    
                    if (!CellsInventoryMath.TryFindFreeSpaceForItem(ref grid, (sortedItems[j].Range.Width, sortedItems[j].Range.Height), out var foundRange, CellsInventoryRotationSearchMode.Find)) {
                        ArrayPool<CellsRange>.Shared.Return(items, true);
                        grid.Dispose();
                        return false;
                    }

                    outRanges[sortedItems[j].Index] = foundRange;
                    items[i + j] = foundRange;
                }
            }
            finally {
                ArrayPool<CellsRange>.Shared.Return(items, true);
                grid.Dispose();
            }

            return true;
        }

        public static void PlaceItemInStorage(SdUserProfile gameData, string itemGuid, GameSnapshotLoadoutItem item, CellsRange placement) {
            var storageItem = gameData.Storage.Lookup.GetOrCreate(itemGuid, out _);
            
            item.IndexI  = (byte)placement.I;
            item.IndexJ  = (byte)placement.J;
            item.Rotated = placement.Rotated;
            
            storageItem.Item.Value    = item;
            storageItem.IndexI.Value  = placement.I;
            storageItem.IndexJ.Value  = placement.J;
            storageItem.Rotated.Value = placement.Rotated;
        }
    }
}
