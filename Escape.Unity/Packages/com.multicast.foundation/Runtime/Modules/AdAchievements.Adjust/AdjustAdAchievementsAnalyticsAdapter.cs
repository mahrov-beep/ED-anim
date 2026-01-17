#if ADJUST_SDK

namespace Multicast.Modules.AdAchievements.Adjust {
    using Multicast.Analytics;
    using AdjustSdk;
    using Multicast.AdAchievements;
    using Multicast.Advertising;

    internal sealed class AdjustAdAchievementsAnalyticsAdapter : IAnalyticsAdapter {
        private readonly AdjustAdAchievementsConfiguration configuration;

        public string Name { get; } = "AdAchievements.Adjust";

        public AdjustAdAchievementsAnalyticsAdapter(AdjustAdAchievementsConfiguration configuration) {
            this.configuration = configuration;
        }

        public void Send(BakedAnalyticsEvent evt) {
            switch (evt.SourceEvent) {
                case AdEcpmAchievedAnalyticsEvent ecpm:
                    //
                    break;

                case AdRevenueAchievedAnalyticsEvent revenue:
                    SendAdRevenueAchievedEvent(revenue);
                    break;

                case AdImpressionsAchievedAnalyticsEvent impressions:
                    //
                    break;

                case AdPlayTimeAchievedAnalyticsEvent playtime:
                    SendPlayTimeAchievedEvent(playtime);
                    break;

                case AdRevenueAnalyticsEvent adRevenue:
                    SendAdRevenueAchievedEvent(adRevenue);
                    break;

                case RewardedAdHiddenAnalyticsEvent rewardedAdHidden:
                    this.SendRewardedAdHiddenAnalyticsEvent(rewardedAdHidden);
                    break;

                case AppOpenAdHiddenAnalyticsEvent appOpenAdHidden:
                    this.SendAppOpenAdHiddenEvent(appOpenAdHidden);
                    break;
            }
        }

        private void SendAppOpenAdHiddenEvent(AppOpenAdHiddenAnalyticsEvent _) {
            if (string.IsNullOrEmpty(this.configuration.appOpenAdHiddedEvent)) {
                return;
            }

            var evt = new AdjustEvent(this.configuration.appOpenAdHiddedEvent);
            Adjust.TrackEvent(evt);
        }

        private void SendRewardedAdHiddenAnalyticsEvent(RewardedAdHiddenAnalyticsEvent _) {
            if (string.IsNullOrEmpty(this.configuration.rewardedAdHiddedEvent)) {
                return;
            }

            var evt = new AdjustEvent(this.configuration.rewardedAdHiddedEvent);
            Adjust.TrackEvent(evt);
        }

        private void SendAdRevenueAchievedEvent(AdRevenueAnalyticsEvent adRevenue) {
            var rev = new AdjustAdRevenue("applovin_max_sdk");
            rev.SetRevenue(adRevenue.Revenue, "USD");
            rev.AddPartnerParameter("AdFormat", adRevenue.AdFormat);
            rev.AddPartnerParameter("NetworkName", adRevenue.NetworkName);
            rev.AddPartnerParameter("NetworkPlacement", adRevenue.NetworkPlacement);
            rev.AddPartnerParameter("Placement", adRevenue.Placement);
            rev.AddPartnerParameter("CreativeIdentifier", adRevenue.CreativeIdentifier);
            Adjust.TrackAdRevenue(rev);
            
            if (string.IsNullOrEmpty(this.configuration.customRevenueEvent)) {
                return;
            }

            AdjustEvent customAdRevenue = new AdjustEvent(this.configuration.customRevenueEvent);
            customAdRevenue.AddPartnerParameter("value", adRevenue.Revenue.ToString("f2"));
            customAdRevenue.AddPartnerParameter("currency", "USD");
            Adjust.TrackEvent(customAdRevenue);
        }

        private static void SendPlayTimeAchievedEvent(AdPlayTimeAchievedAnalyticsEvent playtime) {
            if (string.IsNullOrEmpty(playtime.AdjustCode)) {
                return;
            }

            var evt = new AdjustEvent(playtime.AdjustCode);
            Adjust.TrackEvent(evt);
        }

        private static void SendAdRevenueAchievedEvent(AdRevenueAchievedAnalyticsEvent revenue) {
            if (string.IsNullOrEmpty(revenue.AdjustCode)) {
                return;
            }

            var evt = new AdjustEvent(revenue.AdjustCode);
            evt.AddPartnerParameter("value", revenue.Value);
            evt.AddPartnerParameter("currency", "USD");
            Adjust.TrackEvent(evt);
        }

        public void Flush() {
        }
    }
}
#endif