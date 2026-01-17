namespace Multicast.AdAchievements {
    using Analytics;

    public abstract class AdAchievementAnalyticsEventBase : IAnalyticsEvent {
        public string Name => "impressionAchivements";

        public string AdjustCode { get; set; }
        public string Value      { get; set; }

        public abstract AnalyticsArgCollection Args { get; }
    }

    public class AdImpressionsAchievedAnalyticsEvent : AdAchievementAnalyticsEventBase {
        public override AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("Ad Impressions", $"> {this.Value}"));
    }

    public class AdEcpmAchievedAnalyticsEvent : AdAchievementAnalyticsEventBase {
        public override AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("eCPM", $"> {this.Value}$"));
    }

    public class AdRevenueAchievedAnalyticsEvent : AdAchievementAnalyticsEventBase {
        public override AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("Ad Revenue", $"> {this.Value}$"));
    }

    public class AdPlayTimeAchievedAnalyticsEvent : AdAchievementAnalyticsEventBase {
        public override AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("Play Time", $"> {this.Value}"));
    }

    public class AdImpressionsFirstDayAchievedAnalyticsEvent : AdAchievementAnalyticsEventBase {
        public override AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("Ad Impressions 1st day", $"> {this.Value}"));
    }

    public class AdRevenueFirstDayAchievedAnalyticsEvent : AdAchievementAnalyticsEventBase {
        public override AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("Ad Revenue 1st day", $"> {this.Value}$"));
    }

    public class AdPlayTimeFirstDayAchievedAnalyticsEvent : AdAchievementAnalyticsEventBase {
        public override AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add(new AnalyticsArg("Play Time 1st day", $"> {this.Value}"));
    }

    public class AdRevenueAnalyticsEvent : IAnalyticsEvent {
        public string Name => "adRevenue";

        public string AdUnitIdentifier   { get; set; }
        public string AdFormat           { get; set; }
        public string NetworkName        { get; set; }
        public string NetworkPlacement   { get; set; }
        public string Placement          { get; set; }
        public string CreativeIdentifier { get; set; }
        public double Revenue            { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection();
    }

    public class IapRevenueAnalyticsEvent : IAnalyticsEvent {
        public string Name => "iapRevenue";

        public string  StoreSpecificId  { get; set; }
        public string  ValidatedReceipt { get; set; }
        public string  Currency         { get; set; }
        public decimal Price            { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection();
    }

    public class EcpmProfileAnalyticsEvent : IAnalyticsEvent {
        public string Name => "adEcpmProfile";

        public double Ecpm { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection();
    }

    public class AdProfileEvent : IAnalyticsEvent {
        public string Name => "adProfile";

        public int    Impressions     { get; set; }
        public double Ecpm            { get; set; }
        public bool   HasSubscription { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection();
    }

    public class LocalImpressionAnalyticsEvent : IAnalyticsEvent {
        public string Name => "User Local Impression Data";

        public bool   Subscription        { get; set; }
        public double Revenue             { get; set; }
        public int    RewardedImpressions { get; set; }
        public double RewardedRevenue     { get; set; }
        public double RewardedEcpm        { get; set; }
        public double TotalEcpm           { get; set; }

        public AnalyticsArgCollection Args =>
            new AnalyticsArgCollection()
                .Add(new AnalyticsArg("Is Active Subscription", this.Subscription.ToString()))
                .Add(new AnalyticsArg("IcrementRevenue", this.Revenue.ToString()))
                .Add(new AnalyticsArg("userRewardedImpressions", this.RewardedImpressions.ToString()))
                .Add(new AnalyticsArg("userRewardedAdRevenue", this.RewardedRevenue.ToString()))
                .Add(new AnalyticsArg("userRewardedECPM", this.RewardedEcpm.ToString()))
                .Add(new AnalyticsArg("totalECPM", this.TotalEcpm.ToString()));
    }
}