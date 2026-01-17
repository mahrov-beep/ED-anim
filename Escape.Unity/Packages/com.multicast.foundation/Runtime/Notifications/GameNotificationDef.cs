namespace Multicast.Notifications {
    using System;
    using DirtyDataEditor;
    using JetBrains.Annotations;

    [DDEObject, Serializable]
    public class GameNotificationDef : Def {
        [DDE("enabled")] public bool enabled;

        [DDE("channel")] public string channel;

        [DDE("id")] public int id;

        [DDE("small_icon"), CanBeNull] public string smallIcon;
        [DDE("large_icon"), CanBeNull] public string largeIcon;
    }
}