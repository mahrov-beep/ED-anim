namespace Game.Shared.Defs {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [Serializable, DDEObject]
    public class ThresherDef : Def {
        [DDE("levels")] 
        public List<ThresherLevelDef> level;
    }

    [Serializable, DDEObject]
    public class ThresherLevelDef {
        [DDE("items"), DDEExternalKey("Items")] 
        public Dictionary<string, int> items;

        public static readonly ThresherLevelDef MaxLevelDef = new ThresherLevelDef {
            items = new Dictionary<string, int>(),
        };
    }
}