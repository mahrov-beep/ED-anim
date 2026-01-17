namespace Quantum.CellsInventory {
    public static class CellsInventoryMath {
        /// <summary>
        /// True если в любом месте инвентаря есть достаточно места для размещения предмета, иначе false.
        /// </summary>
        /// <param name="inventory">Инвентарь в котором идет поиск.</param>
        /// <param name="itemSize">Размер предмета который добавляется в инвентарь.</param>
        /// <param name="outRange">Найденные допустимые координаты и тот же размер предмета.</param>
        /// <param name="search">Допустимый способ добавления предмета.</param>
        public static bool TryFindFreeSpaceForItem(ref CellsInventoryAccess inventory, (int width, int height) itemSize, out CellsRange outRange, CellsInventoryRotationSearchMode search) {
            var range = CellsRange.FromIJWH(0, 0, itemSize.width, itemSize.height, false);

            for (var i = 0; i < inventory.Height; i++) {
                for (var j = 0; j < inventory.Width; j++) {
                    if (FitsWithRotation(ref inventory, range.WithIJ(i, j), out var matchedPlace, search)) {
                        outRange = matchedPlace;
                        return true;
                    }
                }
            }

            outRange = default;
            return false;
        }

        /// <summary>
        /// True если предмет с координатами и размером range можно поместить в инвентарь inventory
        /// с учетом правила поиска search, иначе false.
        /// </summary>
        /// <remarks>
        /// В отличии от метода FitsWithRotation дополнительно пытается немного сдвинуть предмет относительно указанных в range координат.
        /// </remarks>
        /// <param name="inventory">Инвентарь в котором идет поиск.</param>
        /// <param name="range">Координаты и размер предмета который добавляется в инвентарь.</param>
        /// <param name="outRange">Найденные допустимые координаты и тот же размер предмета.</param>
        /// <param name="search">Допустимый способ добавления предмета.</param>
        public static bool CanBePlaceIn(ref CellsInventoryAccess inventory, CellsRange range, out CellsRange outRange, CellsInventoryRotationSearchMode search) {
            for (var i = 0; i < range.Height; i++) {
                for (var j = 0; j < range.Width; j++) {
                    if (FitsWithRotation(ref inventory, range.WithIJ(range.I - i, range.J - j), out var matchedPlace, search)) {
                        outRange = matchedPlace;
                        return true;
                    }
                }
            }

            outRange = CellsRange.Empty;
            return false;
        }

        /// <summary>
        /// True если предмет с координатами и размером range можно поместить в инвентарь inventory
        /// с учетом правила поиска search, иначе false.
        /// </summary>
        /// <param name="inventory">Инвентарь в котором идет поиск.</param>
        /// <param name="range">Координаты и размер предмета который добавляется в инвентарь.</param>
        /// <param name="outRange">Найденные допустимые координаты и тот же размер предмета.</param>
        /// <param name="search">Допустимый способ добавления предмета.</param>
        public static bool FitsWithRotation(ref CellsInventoryAccess inventory, CellsRange range, out CellsRange outRange, CellsInventoryRotationSearchMode search) {
            if (search is CellsInventoryRotationSearchMode.Default or CellsInventoryRotationSearchMode.Find) {
                if (Fits(ref inventory, range)) {
                    outRange = range;
                    return true;
                }
            }

            if (search is CellsInventoryRotationSearchMode.Rotated or CellsInventoryRotationSearchMode.Find) {
                if (Fits(ref inventory, range.GetRotated())) {
                    outRange = range.GetRotated();
                    return true;
                }
            }

            outRange = CellsRange.Empty;
            return false;
        }

        /// <summary>
        /// True если предмет с координатами и размером range можно поместить в инвентарь inventory, иначе false.
        /// </summary>
        public static bool Fits(ref CellsInventoryAccess inventory, CellsRange range) {
            for (var i = range.MinI; i <= range.MaxI; i++) {
                if (i < 0 || i >= inventory.Height) {
                    return false;
                }

                for (var j = range.MinJ; j <= range.MaxJ; j++) {
                    if (j < 0 || j >= inventory.Width) {
                        return false;
                    }

                    if (inventory[i, j]) {
                        return false;
                    }
                }
            }

            return true;
        }
        
        public static void FillLoadout(ref CellsInventoryAccess inventory, CellsRange[] ranges, CellsRange? exceptItem = null) {
            foreach (var range in ranges) {
                if (exceptItem.HasValue 
                 && range.I == exceptItem.Value.I 
                 && range.J == exceptItem.Value.J 
                 && range.Width == exceptItem.Value.Width 
                 && range.Height == exceptItem.Value.Height) {
                    continue;
                }

                for (var i = range.MinI; i <= range.MaxI; i++) {
                    for (var j = range.MinJ; j <= range.MaxJ; j++) {
                        inventory[i, j] = true;
                    }
                }
            }
        }
        
        public static void ClearLoadout(ref CellsInventoryAccess inventory, int width, int height) {
            for (var i = 0; i < height; i++) {
                for (var j = 0; j < width; j++) {
                    inventory[i, j] = false;
                }
            }
        }
    }
}