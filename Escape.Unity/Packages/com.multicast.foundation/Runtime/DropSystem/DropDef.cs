namespace Multicast.DropSystem {
    using System;
    using DirtyDataEditor;

    [Serializable, DDEObject, DDEBase("type")]
    public abstract class DropDef {
        [DDE("type")] public string type;
    }
}