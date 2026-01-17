namespace Multicast.Advertising {
    using GameProperties;

    public static class AdGameProperties {
        public static readonly BoolGamePropertyName AdNoRewarded = "ad_no_rewarded";

        public static readonly IntGamePropertyName AdRewardedSucceed  = "ad_rewarded_succeed";
        public static readonly IntGamePropertyName AdRewardedCanceled = "ad_rewarded_canceled";
        public static readonly IntGamePropertyName AdRewardedFailed   = "ad_rewarded_failed";
    }
}