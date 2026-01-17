namespace Multicast.CoreAchievements {
    using System;
    using DirtyDataEditor;

    [Serializable, DDEObject]
    public class CoreAchievementDef : Def {
        [DDE("property")] public string property;
        [DDE("value")]    public int    value;
    }
}