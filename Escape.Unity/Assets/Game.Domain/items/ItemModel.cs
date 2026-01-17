namespace Game.Domain.items {
    using System.Collections.Generic;
    using Multicast;
    using Multicast.Collections;
    using Multicast.Numerics;
    using Quantum;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Items;
    using UniMob;

    public class ItemsModel : KeyedSingleInstanceModel<ItemDef, SdItem, ItemModel> {
        public ItemsModel(Lifetime lifetime, LookupCollection<ItemDef> defs, SdUserProfile gameData)
            : base(lifetime, defs, gameData.Items.Lookup) {
            this.AutoConfigureData = true;
        }

        public List<ItemModel> AllItems => this.Values;
    }

    public class ItemModel : Model<ItemDef, SdItem> {
        public ItemModel(Lifetime lifetime, ItemDef def, SdItem data) : base(lifetime, def, data) {
        }

        [Atom] public ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Key)
        );
    }
}