namespace Game.Shared.Defs {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Multicast.RewardSystem;

    [Serializable, DDEObject]
    public class QuestDef : Def {
        [DDE("prev_quests", DDE.Empty), DDEExternalKey("Quests")] public List<string> prevQuests;

        [DDE("reward")] public List<RewardDef> rewards;
    }
}