namespace Game.Shared.Defs {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Quantum;

    [Serializable, DDEObject]
    public class GameModeDef : Def {
        [DDE("visible", true)] public bool      visible;
        [DDE("game_rule")]     public GameRules gameRule;

        [DDE("exp_for_kill")]              public int expForKill;
        [DDE("exp_for_kill_in_lost_raid")] public int expForKillInLostRaid;
        [DDE("exp_for_raid")]              public int expForRaid;
        [DDE("exp_for_lost_raid")]         public int expForLostRaid;
        [DDE("exp_for_earnings_percent")]  public int expForEarningsPercent;

        [DDE("min_loadout_quality", 0)] public int minLoadoutQuality;
        [DDE("min_profile_level", 0)]   public int minProfileLevel;

        [DDE("loot_rarity", DDE.Empty)] public List<ERarityType> lootRarities;
    }
}