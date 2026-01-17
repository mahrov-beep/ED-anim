namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [Serializable, DDEObject]
    public class LevelDef : Def {
        [DDE("level")]             public int level;
        [DDE("exp_to_next_level")] public int expToNextLevel;
    }
}