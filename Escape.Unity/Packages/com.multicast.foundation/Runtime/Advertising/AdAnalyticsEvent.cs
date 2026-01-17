namespace Multicast.Advertising {
    using Analytics;

    public class RewardedStartAnalyticsEvent : IAnalyticsEvent {
        public string Name => "rewarded_start";

        public string Placement { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("placement", this.Placement);
    }

    public class RewardedEndAnalyticsEvent : IAnalyticsEvent {
        public string Name => "rewarded_end";

        public string Placement { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("placement", this.Placement);
    }

    public class RewardedCompleteAnalyticsEvent : IAnalyticsEvent {
        public string Name => "rewarded_complete";

        public string Placement { get; set; }
        public string AdNetwork { get; set; }
        public string AdUnitId  { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("placement", this.Placement)
            .Add("ad_network", this.AdNetwork)
            .Add("ad_unit_id", this.AdUnitId);
    }

    public class RewardedSkipAnalyticsEvent : IAnalyticsEvent {
        public string Name => "rewarded_skip";

        public string Placement { get; set; }
        public string AdNetwork { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("placement", this.Placement)
            .Add("ad_network", this.AdNetwork);
    }

    public class RewardedFailAnalyticsEvent : IAnalyticsEvent {
        public string Name => "rewarded_fail";

        public string Placement    { get; set; }
        public string ErrorMessage { get; set; }

        public AnalyticsArgCollection Args => new AnalyticsArgCollection()
            .Add("placement", this.Placement)
            .Add("error", this.ErrorMessage);
    }

    public class RewardedAdHiddenAnalyticsEvent : IAnalyticsEvent {
        public string Name => "rewardedHidden";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection();
    }

    public class AppOpenAdHiddenAnalyticsEvent : IAnalyticsEvent {
        public string Name => "appHidden";

        public AnalyticsArgCollection Args => new AnalyticsArgCollection();
    }
}