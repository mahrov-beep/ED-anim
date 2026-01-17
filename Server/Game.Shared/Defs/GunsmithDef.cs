namespace Game.Shared.Defs {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [Serializable, DDEObject]
    public class GunsmithDef : Def {
        [DDE("thresher"), DDEExternalKey("Threshers")] public string thresher;
    }
}