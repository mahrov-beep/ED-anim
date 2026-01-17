namespace Game.Domain.Storage {
    using System.Buffers;
    using System.Collections.Generic;
    using Multicast;
    using Quantum;
    using Shared;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Helpers;
    using UniMob;

    public class StorageModel : Model {
        [Inject] private SdUserProfile userProfile;
        [Inject] private GameDef       gameDef;

        public StorageModel(Lifetime lifetime) : base(lifetime) {
        }

        public bool HasItemInInventory(string itemKey) {
            return this.ItemsInInventoryCount(itemKey) > 0;
        }
        
        public int ItemsInInventoryCount(string itemKey) {
            var count = 0;
            
            foreach (var storageItem in this.userProfile.Storage.Lookup) {
                if (storageItem.Item.Value.ItemKey == itemKey) {
                    count += 1;
                }
            }
            
            return count;
        }

        public bool CanAddItems(string[] itemKeys, HashSet<string> exceptItemGuids = default) {
            var newRanges = ArrayPool<CellsRange>.Shared.Rent(itemKeys.Length);
            
            for (var i = 0; i < itemKeys.Length; i++) {
                if (!this.gameDef.Items.TryGet(itemKeys[i], out var itemDef)) {
                    continue;
                }
                
                var range = CellsRange.FromIJWH(0, 0, itemDef.CellsWidth, itemDef.CellsHeight, true);

                newRanges[i] = range;
            }

            if (!StoragePlacementHelper.TryFindPlaceInStorage(this.gameDef, this.userProfile, newRanges, out var foundRanges, exceptItemGuids)) {
                return false;
            }

            return true;
        }
    }
}
