namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Multicast.RewardSystem;

    [Serializable, DDEObject, DDEImpl(typeof(RewardDef), "item")]
    public class ItemRewardDef : RewardDef {
        [DDE("item"), DDEExternalKey("Items")] public string item;
    }
}