namespace Multicast.Advertising {
    using System;
    using Analytics;
    using Cysharp.Threading.Tasks;
    using GameProperties;
    using Multicast;
    using UnityEngine;

    /// <summary>
    /// Show rewarded ad. Returns true is ad succeed, false otherwise.
    /// </summary>
    [Serializable, RequireFieldsInit]
    public struct RewardedAdShowControllerArgs : IResultControllerArgs<bool> {
        public string Placement;
    }

    [SkipInstallWithoutDependency(typeof(IAdvertising))]
    public class RewardedAdShowController : ResultController<RewardedAdShowControllerArgs, bool> {
        [Inject] private readonly IAdvertising        advertising;
        [Inject] private readonly IAnalytics          analytics;
        [Inject] private readonly GamePropertiesModel gameProperties;

        protected override async UniTask<bool> Execute(Context context) {
            if (this.gameProperties.Get(AdGameProperties.AdNoRewarded)) {
                context.Execute(new IncrementGamePropertyIntCommand(AdGameProperties.AdRewardedSucceed, 1));
                return true;
            }

            this.analytics.Send(new RewardedStartAnalyticsEvent {
                Placement = this.Args.Placement,
            });

            AdResult result;
            try {
                result = await this.advertising.ShowRewarded(this.Args.Placement);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                result = AdResult.NotAvailable("error");
            }

            this.analytics.Send(new RewardedEndAnalyticsEvent {
                Placement = this.Args.Placement,
            });

            switch (result) {
                case AdResult.AdCompleted completed:
                    this.analytics.Send(new RewardedCompleteAnalyticsEvent {
                        Placement = this.Args.Placement,
                        AdNetwork = completed.AdNetwork,
                        AdUnitId  = completed.ADUnitId,
                    });

                    context.Execute(new IncrementGamePropertyIntCommand(AdGameProperties.AdRewardedSucceed, 1));
                    break;

                case AdResult.AdCanceled canceled:
                    this.analytics.Send(new RewardedSkipAnalyticsEvent {
                        Placement = this.Args.Placement,
                        AdNetwork = canceled.AdNetwork,
                    });

                    context.Execute(new IncrementGamePropertyIntCommand(AdGameProperties.AdRewardedCanceled, 1));
                    break;

                case AdResult.AdNotAvailable notAvailable:
                    this.analytics.Send(new RewardedFailAnalyticsEvent {
                        Placement    = this.Args.Placement,
                        ErrorMessage = notAvailable.Message,
                    });

                    context.Execute(new IncrementGamePropertyIntCommand(AdGameProperties.AdRewardedFailed, 1));
                    break;
            }

            return result is AdResult.AdCompleted;
        }
    }
}