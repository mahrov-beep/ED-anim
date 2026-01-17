namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [Serializable, DDEObject]
    public class CurrencyDef : Def {
        [DDE("initial_amount")] public int InitialAmount;

        [DDE("min_amount", null)] public int? MinAmount;

        [DDE("allow_negative_additions", false)] public bool AllowNegativeAdditions;
    }
}