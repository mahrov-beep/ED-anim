namespace Game.Shared.Defs {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Multicast.RewardSystem;

    [Serializable, DDEObject]
    public class ExpProgressionRewardDef : Def {
        [DDE("level_to_complete")] public int levelToComplete;

        [DDE("rewards", DDE.Empty)] public List<RewardDef> rewards;
    }
}