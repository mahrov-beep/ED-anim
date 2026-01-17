namespace Quantum {
    public static class ReloadItemBoxCommandHelper {
        public class StorageData {
            public GameSnapshotStorage Storage;
            public int Width;
            public int Height;
        }

        private static StorageData pendingData;

        public static void SetPendingStorageData(GameSnapshotStorage storage, int width, int height) {
            pendingData = new StorageData {
                Storage = storage,
                Width = width,
                Height = height,
            };
        }

        public static StorageData GetPendingStorageData() {
            return pendingData;
        }

        public static void ClearPendingStorageData() {
            pendingData = null;
        }
    }
}

