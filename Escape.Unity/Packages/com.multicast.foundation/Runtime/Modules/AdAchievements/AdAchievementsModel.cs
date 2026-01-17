namespace Multicast.Modules.AdAchievements {
    using System.Collections.Generic;
    using Collections;
    using Multicast;
    using UniMob;

    public class AdAchievementsModel : KeyedSingleInstanceModel<AdAchievementDef, AdAchievementData, AdAchievementModel> {
        public new List<AdAchievementModel> Values => base.Values;

        public AdAchievementsModel(Lifetime lifetime, LookupCollection<AdAchievementDef> def, UdAdAchievementsRepo data)
            : base(lifetime, def, data.Lookup) {
        }
    }

    public class AdAchievementModel : Model<AdAchievementDef, AdAchievementData> {
        public AdAchievementModel(Lifetime lifetime, AdAchievementDef def, AdAchievementData data)
            : base(lifetime, def, data) {
        }

        public int   Impressions   => this.Def.impressions;
        public float Revenue       => this.Def.revenue;
        public float Ecpm          => this.Def.ecpm;
        public float PlayTime      => this.Def.playTime;
        public float TimeFromStart => this.Def.timeFromStart;

        public bool WasSent => this.Data.WasSent.Value;

        public string ParameterValue => this.Def.parameterValue;

        public string AdjustEventCode => this.Def.adjustEventCode;

        public AdAchievementType AchievementType => this.Def.achievementType;
    }
}