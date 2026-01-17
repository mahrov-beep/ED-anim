namespace Game.UI.Widgets.GameInventory {
    using Quantum;
    using Views;

    public static class DropPlaceHelper {
        public delegate bool TryTetrisAt(DragAndDropPayloadItem payload, int i, int j, out CellsRange dropRange, RotationType rotationType, byte source);
        public delegate bool TryMergeAt(DragAndDropPayloadItem payload, int i, int j, out CellsRange mergeRange, byte source);
        public delegate (int width, int height, RotationType rotationType) GetMetrics(DragAndDropPayloadItem payload, byte source);

        public static CellsRange Compute(TryTetrisAt tryTetrisAt,
                                         TryMergeAt tryMergeAt,
                                         GetMetrics getMetrics,
                                         DragAndDropPayloadItem payload,
                                         int i,
                                         int j,
                                         out bool valid,
                                         byte source = 0) {
            var metrics = getMetrics(payload, source);

            if (tryTetrisAt(payload, i, j, out var dropRange, RotationType.Find, source)) {
                valid = true;
                return dropRange;
            }

            if (tryMergeAt != null && tryMergeAt(payload, i, j, out var mergeRange, source)) {
                valid = true;
                return mergeRange;
            }

            valid = false;

            if (metrics.rotationType == RotationType.Rotated) {
                return CellsRange.FromIJWH(i, j, metrics.height, metrics.width, true);
            }

            return CellsRange.FromIJWH(i, j, metrics.width, metrics.height, false);
        }
    }
}


