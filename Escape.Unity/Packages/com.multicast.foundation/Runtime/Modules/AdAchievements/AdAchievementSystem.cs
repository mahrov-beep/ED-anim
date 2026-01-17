namespace Multicast.Modules.AdAchievements {
    using System;
    using GameProperties;
    using Multicast.AdAchievements;
    using Multicast.Advertising;
    using Multicast.Analytics;
    using Multicast.UserStats;
    using Playtime;
    using Scellecs.Morpeh;
    using Scellecs.Morpeh.Collections;
    using UserStats;

    [SkipInstallWithoutDependency(typeof(AdAchievementsModel))]
    internal class AdAchievementSystem : SystemBase {
        [Inject] private readonly UdUserStatsRepo     userStatsData;
        [Inject] private readonly IAnalytics          analytics;
        [Inject] private readonly AdAchievementsModel adAchievementsModel;
        [Inject] private readonly ITimeService        timeService;
        [Inject] private readonly GamePropertiesModel properties;

        [Inject] private readonly Event<AdImpressionEvent> adImpressionEvent;
        [Inject] private readonly Event<RevenuePaidEvent>  revenuePaidEvent;

        public override void OnAwake() {
            var ecpm = this.userStatsData.AdRevenue.Value / this.userStatsData.AdImpressionCount.Value * 1000;

            this.analytics.Send(new EcpmProfileAnalyticsEvent {
                Ecpm = ecpm,
            });
        }

        public override void OnUpdate(float deltaTime) {
            this.UpdateAchievements();

            if (this.adImpressionEvent.isPublished) {
                this.AddImpressionsData(this.adImpressionEvent.publishedChanges);
            }

            if (this.revenuePaidEvent.isPublished) {
                this.OnRevenuePaid(this.revenuePaidEvent.publishedChanges);
            }
        }

        private void UpdateAchievements() {
            foreach (var adAchievement in this.adAchievementsModel.Values) {
                if (adAchievement.WasSent) {
                    continue;
                }

                if (adAchievement.Impressions > this.userStatsData.AdImpressionCount.Value) {
                    continue;
                }

                if (adAchievement.Revenue > this.userStatsData.AdRevenue.Value) {
                    continue;
                }

                if (adAchievement.Ecpm > 0 && this.userStatsData.AdImpressionCount.Value > 0) {
                    var ecpm = this.userStatsData.AdRevenue.Value / this.userStatsData.AdImpressionCount.Value * 1000;

                    if (adAchievement.Ecpm > ecpm) {
                        continue;
                    }
                }

                if (adAchievement.PlayTime > this.userStatsData.PlaytimeMinutes.Value) {
                    continue;
                }

                var secondsFromStart = (this.timeService.Now - this.userStatsData.FirstOpenTime.Value).TotalSeconds;

                if (secondsFromStart > adAchievement.TimeFromStart) {
                    continue;
                }

                var evt = CreateEvent(adAchievement);

                evt.AdjustCode = adAchievement.AdjustEventCode;
                evt.Value      = adAchievement.ParameterValue;

                this.analytics.Send(evt);

                App.Execute(new AdAchievementSendCommand(adAchievement.Key));
            }
        }

        private static AdAchievementAnalyticsEventBase CreateEvent(AdAchievementModel adAchievement) {
            return adAchievement.AchievementType switch {
                AdAchievementType.Ecpm => new AdEcpmAchievedAnalyticsEvent(),
                AdAchievementType.Revenue => new AdRevenueAchievedAnalyticsEvent(),
                AdAchievementType.Impressions => new AdImpressionsAchievedAnalyticsEvent(),
                AdAchievementType.Playtime => new AdPlayTimeAchievedAnalyticsEvent(),
                AdAchievementType.RevenueFirstDay => new AdRevenueFirstDayAchievedAnalyticsEvent(),
                AdAchievementType.ImpressionsFirstDay => new AdImpressionsFirstDayAchievedAnalyticsEvent(),
                AdAchievementType.PlaytimeFirstDay => new AdPlayTimeFirstDayAchievedAnalyticsEvent(),
                _ => throw new ArgumentOutOfRangeException(nameof(adAchievement.AchievementType)),
            };
        }

        private void OnRevenuePaid(FastList<RevenuePaidEvent> revenueData) {
            foreach (var data in revenueData) {
                this.analytics.Send(
                    new AdRevenueAnalyticsEvent {
                        AdUnitIdentifier   = data.AdUnitIdentifier,
                        AdFormat           = data.AdFormat,
                        NetworkName        = data.NetworkName,
                        NetworkPlacement   = data.NetworkPlacement,
                        Placement          = data.Placement,
                        CreativeIdentifier = data.CreativeIdentifier,
                        Revenue            = data.Revenue,
                    });

                var ecpm            = this.userStatsData.AdRevenue.Value / this.userStatsData.AdImpressionCount.Value * 1000;
                var hasSubscription = this.properties.Get(AppGameProperties.Booleans.HasSubscription);

                this.analytics.Send(
                    new AdProfileEvent {
                        Impressions     = this.userStatsData.AdImpressionCount.Value,
                        Ecpm            = ecpm,
                        HasSubscription = hasSubscription,
                    });

                this.analytics.Send(new LocalImpressionAnalyticsEvent {
                    Subscription        = hasSubscription,
                    Revenue             = data.Revenue,
                    RewardedImpressions = this.userStatsData.AdImpressionCount.Value,
                    RewardedRevenue     = this.userStatsData.AdRevenue.Value,
                    RewardedEcpm        = ecpm,
                    TotalEcpm           = ecpm,
                });
            }
        }

        private void AddImpressionsData(FastList<AdImpressionEvent> adImpressionEvents) {
            foreach (var data in adImpressionEvents) {
                App.Execute(new AddImpressionDataCommand(data));
            }
        }
    }
}