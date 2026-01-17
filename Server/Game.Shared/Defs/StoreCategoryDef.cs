namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using Multicast.ExpressionParser;

    [Serializable, DDEObject]
    public class StoreCategoryDef : Def {
        [DDE("type", StoreCategoryType.Currency)] public StoreCategoryType categoryType;

        [DDE("priority")] public int priority;
        [DDE("page")]     public int page;
    }

    public enum StoreCategoryType {
        Purchasing,
        Currency,
        Upgrades,
        TimedCard,
    }
}