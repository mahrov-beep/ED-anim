namespace Multicast.Modules.AdAchievements {
    using System;
    using DirtyDataEditor;

    [DDEObject, Serializable]
    public class AdAchievementDef : Def {
        [DDE("impressions", 0)]        public int   impressions;
        [DDE("revenue", 0)]            public float revenue;
        [DDE("ecpm", 0)]               public float ecpm;
        [DDE("play_time", 0)]          public float playTime;
        [DDE("time_from_start", float.MaxValue)] public float timeFromStart;

        [DDE("parameter_value", null)]     public string parameterValue;
        [DDE("adjust_event_code", null)]   public string adjustEventCode;

        [DDE("achievement_type")] public AdAchievementType achievementType;
    }
    
    public enum AdAchievementType {
        Revenue,
        Ecpm,
        Impressions,
        Playtime,
        RevenueFirstDay,
        ImpressionsFirstDay,
        PlaytimeFirstDay,
    }
}