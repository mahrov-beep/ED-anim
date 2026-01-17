// ReSharper disable InconsistentNaming

namespace Multicast.RewardSystem {
    using System;
    using DirtyDataEditor;

    [Serializable, DDEObject, DDEBase("type")]
    public abstract class RewardDef {
        [DDE("type")] public string type;
    }
}