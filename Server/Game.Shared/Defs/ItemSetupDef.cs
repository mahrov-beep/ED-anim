namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [Serializable, DDEObject]
    public class ItemSetupDef : Def {
        [DDE("item"), DDEExternalKey("Items")] public string itemKey;

        [DDE("scope", null), DDEExternalKey("Items")]    public string scope;
        [DDE("grip", null), DDEExternalKey("Items")]     public string grip;
        [DDE("muzzle", null), DDEExternalKey("Items")]   public string muzzle;
        [DDE("magazine", null), DDEExternalKey("Items")] public string magazine;
        [DDE("stock", null), DDEExternalKey("Items")]    public string stock;
        [DDE("ammo", null), DDEExternalKey("Items")]     public string ammo;
        [DDE("laser", null), DDEExternalKey("Items")]    public string laser;
    }
}