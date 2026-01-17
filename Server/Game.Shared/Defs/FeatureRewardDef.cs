namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Multicast.RewardSystem;

    [Serializable, DDEObject, DDEImpl(typeof(RewardDef), "feature")]
    public class FeatureRewardDef : RewardDef {
        [DDE("feature"), DDEExternalKey("Features")] public string feature;
    }
}