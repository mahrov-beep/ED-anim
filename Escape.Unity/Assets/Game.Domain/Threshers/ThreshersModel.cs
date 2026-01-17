namespace Game.Domain.Threshers {
    using System.Collections.Generic;
    using System.Linq;
    using ItemBoxStorage;
    using Multicast;
    using Multicast.Collections;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Threshers;
    using Storage;
    using UniMob;

    public class ThreshersModel : KeyedSingleInstanceModel<ThresherDef, SdThresher, ThresherModel> {
        public ThreshersModel(Lifetime lifetime, LookupCollection<ThresherDef> defs, SdUserProfile userProfile)
            : base(lifetime, defs, userProfile.Threshers.Lookup) {
            this.AutoConfigureData = true;
        }

        [Atom] public List<ThresherModel> AllThreshers => this.Values.ToList();

        [Atom] public int Notify => this.AllThreshers.Sum(it => it.Notify);
    }

    public class ThresherModel : Model<ThresherDef, SdThresher> {
        [Inject] private StorageModel storageModel;

        public ThresherModel(Lifetime lifetime, ThresherDef def, SdThresher data) : base(lifetime, def, data) {
        }

        [Atom] public ThresherLevelDef ThresherLevelDef => this.Data.Level.Value - 1 < this.Def.level.Count
            ? this.Def.level[this.Data.Level.Value - 1]
            : ThresherLevelDef.MaxLevelDef;

        public int Level    => this.Data.Level.Value;
        public int MaxLevel => this.Def.level.Count + 1;

        public bool CanLevelUp => this.HasEnoughItemsInStorageToLevelUp && this.IsAtMaxLevel == false;

        public bool IsAtMaxLevel => this.ThresherLevelDef == ThresherLevelDef.MaxLevelDef;

        [Atom] public int Notify => (this.CanLevelUp ? 1 : 0);

        public bool HasEnoughItemsInStorageToLevelUp {
            get {
                foreach (var (itemKey, requiredCount) in this.ThresherLevelDef.items) {
                    if (this.ItemsInStorageCount(itemKey) < requiredCount) {
                        return false;
                    }
                }

                return true;
            }
        }

        public int ItemsInStorageCount(string itemKey) {
            return this.storageModel.ItemsInInventoryCount(itemKey);
        }
    }
}