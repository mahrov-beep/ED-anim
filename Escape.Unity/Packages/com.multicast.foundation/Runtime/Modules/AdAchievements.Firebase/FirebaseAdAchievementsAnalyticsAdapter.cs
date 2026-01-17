#if FIREBASE_SDK

namespace Multicast.Modules.AdAchievements.Firebase {
    using Multicast.Analytics;
    using global::Firebase.Analytics;
    using Multicast.AdAchievements;
    using System.Threading;
    using System.Globalization;

    internal sealed class FirebaseAdAchievementsAnalyticsAdapter : IAnalyticsAdapter {
        public string Name { get; } = "AdAchievements.Firebase";

        public void Send(BakedAnalyticsEvent evt) {
            switch (evt.SourceEvent) {
                case AdEcpmAchievedAnalyticsEvent ecpm:
                    SendEcpmAchieved(ecpm);
                    break;

                case AdRevenueAchievedAnalyticsEvent revenue:
                    SendAdRevenueAchieved(revenue);
                    break;

                case AdImpressionsAchievedAnalyticsEvent impressions:
                    SendAdImpressionAchieved(impressions);
                    break;

                case AdPlayTimeAchievedAnalyticsEvent playtime:
                    SendPlayTimeAchieved(playtime);
                    break;

                case AdRevenueFirstDayAchievedAnalyticsEvent firstDayRevenue:
                    SendFirstDayRevenueAchieved(firstDayRevenue);
                    break;

                case AdImpressionsFirstDayAchievedAnalyticsEvent firstDayImpressions:
                    SendFirstDayImpressionAchieved(firstDayImpressions);
                    break;

                case AdPlayTimeFirstDayAchievedAnalyticsEvent firstDayPlayTime:
                    SendFirstDayPlaytimeAchieved(firstDayPlayTime);
                    break;

                case AdRevenueAnalyticsEvent adRevenue:
                    SendAdRevenue(adRevenue);
                    break;
            }
        }

        private static void SendAdRevenue(AdRevenueAnalyticsEvent adRevenue) {
            var userCulture   = Thread.CurrentThread.CurrentCulture;
            var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            customCulture.NumberFormat.CurrencyGroupSeparator = "";
            Thread.CurrentThread.CurrentCulture               = customCulture;
            
            var revenue = adRevenue.Revenue;

            var parameters = new[] {
                new Parameter("ad_platform", "AppLovin"),
                new Parameter("ad_source", adRevenue.NetworkName),
                new Parameter("ad_unit_name", adRevenue.AdUnitIdentifier),
                new Parameter("ad_format", adRevenue.AdFormat),
                new Parameter("value", revenue),
                new Parameter("currency", "USD"),
            };
            FirebaseAnalytics.LogEvent("ad_impression", parameters);
            
            Thread.CurrentThread.CurrentCulture = userCulture;
        }

        private static void SendFirstDayPlaytimeAchieved(AdPlayTimeFirstDayAchievedAnalyticsEvent firstDayPlayTime) {
            var evtName = $"playTime_{firstDayPlayTime.Value}_first_day";
            FirebaseAnalytics.LogEvent(evtName);
        }

        private static void SendFirstDayImpressionAchieved(AdImpressionsFirstDayAchievedAnalyticsEvent firstDayImpressions) {
            var evtName = $"ad_impressions_{firstDayImpressions.Value}_first_day";
            FirebaseAnalytics.LogEvent(evtName);
        }

        private static void SendFirstDayRevenueAchieved(AdRevenueFirstDayAchievedAnalyticsEvent firstDayRevenue) {
            var evtName = $"ad_revenue_{firstDayRevenue.Value}_usd_first_day";
            var parameters = new Parameter[] {
                new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
                new Parameter(FirebaseAnalytics.ParameterValue, firstDayRevenue.Value),
            };
            FirebaseAnalytics.LogEvent(evtName, parameters);
        }

        private static void SendPlayTimeAchieved(AdPlayTimeAchievedAnalyticsEvent playtime) {
            var evtName = $"playTime_{playtime.Value}";
            FirebaseAnalytics.LogEvent(evtName);
        }

        private static void SendAdImpressionAchieved(AdImpressionsAchievedAnalyticsEvent impressions) {
            var evtName = $"ad_impressions_{impressions.Value}";
            FirebaseAnalytics.LogEvent(evtName);
        }

        private static void SendAdRevenueAchieved(AdRevenueAchievedAnalyticsEvent revenue) {
            var evtName = $"ad_revenue_{revenue.Value}_usd";
            var parameters = new Parameter[] {
                new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
                new Parameter(FirebaseAnalytics.ParameterValue, revenue.Value),
            };
            FirebaseAnalytics.LogEvent(evtName, parameters);
        }

        private static void SendEcpmAchieved(AdEcpmAchievedAnalyticsEvent ecpm) {
            var evtName = $"ecpm_{ecpm.Value}_usd";

            FirebaseAnalytics.LogEvent(evtName);
        }

        public void Flush() {
        }
    }
}
#endif