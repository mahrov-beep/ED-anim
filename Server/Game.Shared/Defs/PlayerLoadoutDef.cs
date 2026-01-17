namespace Game.Shared.Defs {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [Serializable, DDEObject]
    public class PlayerLoadoutDef : Def {
        [DDE("melee_weapon", null), DDEExternalKey("ItemSetups"), CanBeNull]     public string meleeWeapon;
        [DDE("primary_weapon", null), DDEExternalKey("ItemSetups"), CanBeNull]   public string primaryWeapon;
        [DDE("secondary_weapon", null), DDEExternalKey("ItemSetups"), CanBeNull] public string secondaryWeapon;
        [DDE("backpack", null), DDEExternalKey("ItemSetups"), CanBeNull]         public string backpack;
        [DDE("helmet", null), DDEExternalKey("ItemSetups"), CanBeNull]           public string helmet;
        [DDE("armor", null), DDEExternalKey("ItemSetups"), CanBeNull]            public string armor;
        [DDE("headphones", null), DDEExternalKey("ItemSetups"), CanBeNull]       public string headphones;
        [DDE("safe", null), DDEExternalKey("ItemSetups"), CanBeNull]             public string safe;
        [DDE("skin", null), DDEExternalKey("ItemSetups"), CanBeNull]             public string skin;
        [DDE("skill", null), DDEExternalKey("ItemSetups"), CanBeNull]            public string skill;
        [DDE("perk1", null), DDEExternalKey("ItemSetups"), CanBeNull]            public string perk1;
        [DDE("perk2", null), DDEExternalKey("ItemSetups"), CanBeNull]            public string perk2;
        [DDE("perk3", null), DDEExternalKey("ItemSetups"), CanBeNull]            public string perk3;

        [DDE("trash", DDE.Empty), DDEExternalKey("ItemSetups")] public List<string> trash;
    }
}
