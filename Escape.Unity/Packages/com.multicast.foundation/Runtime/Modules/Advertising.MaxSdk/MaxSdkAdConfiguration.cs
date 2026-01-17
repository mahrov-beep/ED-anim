namespace Multicast.Modules.Advertising.MaxSdk {
    using System;
    using JetBrains.Annotations;
    using UnityEngine;

    [Serializable]
    public struct MaxSdkAdConfiguration {
        [SerializeField] private string sdkKey;
        [SerializeField] private string rewardedAdUnitId;
        [SerializeField] private string interAdUnitId;

        public string Rewarded => this.rewardedAdUnitId;
        public string Inter    => this.interAdUnitId;
        public string SdkKey   => this.sdkKey;

        [CanBeNull] public IMaxSdkAdUnitOverride RewardedAdUnitOverride { get; private set; }

        public MaxSdkAdConfiguration WithRewardedAdUnitOverride(IMaxSdkAdUnitOverride adUnitOverride) {
            var config = this;
            config.RewardedAdUnitOverride = adUnitOverride;
            return config;
        }
    }
}