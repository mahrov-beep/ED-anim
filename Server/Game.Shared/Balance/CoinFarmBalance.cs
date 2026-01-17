namespace Game.Shared.Balance {
    using System;
    using Defs;
    using Multicast;
    using Multicast.Numerics;
    using UserProfile.Data;
    using UserProfile.Data.CoinFarms;

    public readonly struct CoinFarmBalance {
        private readonly GameDef       gameDef;
        private readonly SdUserProfile userProfile;
        private readonly ITimeService  timeService;

        public CoinFarmBalance(GameDef gameDef, SdUserProfile userProfile, ITimeService timeService) {
            this.gameDef     = gameDef;
            this.userProfile = userProfile;
            this.timeService = timeService;
        }

        public CoinFarmDef GetDef(string coinFarmKey) {
            return this.gameDef.CoinFarms.Get(coinFarmKey);
        }

        public SdCoinFarm GetData(string coinFarmKey) {
            return this.userProfile.CoinFarms.Get(coinFarmKey);
        }

        public int GetCurrentLevel(string coinFarmKey) {
            var coinFarmDef  = this.GetDef(coinFarmKey);
            var thresherData = this.userProfile.Threshers.Get(coinFarmDef.ThresherKey);
            return thresherData.Level.Value;
        }

        public bool IsCollectAllowed(string coinFarmKey) {
            var coinFarmDef = this.GetDef(coinFarmKey);

            if (!this.userProfile.Features.Get(coinFarmDef.LockedByFeatureKey).Unlocked.Value) {
                return false;
            }

            return this.CalcCollectedRewardAmount(coinFarmKey, out _) > 0;
        }

        public int CalcCollectedRewardAmount(string coinFarmKey, out GameTime dueTime) {
            var coinFarmDef = this.GetDef(coinFarmKey);

            if (!this.userProfile.Features.Get(coinFarmDef.LockedByFeatureKey).Unlocked.Value) {
                dueTime = this.timeService.Now;
                return 0;
            }

            var coinFarmData = this.GetData(coinFarmKey);

            var lastCollectTime = coinFarmData.LastCollectTime.Value;

            if (lastCollectTime.Equals(default)) {
                dueTime = this.timeService.Now;
                return this.GetCurrentStorageCapacity(coinFarmKey);
            }

            var produceIntervalSeconds = this.GetCurrentProduceIntervalSeconds(coinFarmKey);
            var produceQuantity        = this.GetCurrentProduceQuantity(coinFarmKey);
            var storageCapacity        = this.GetCurrentStorageCapacity(coinFarmKey);

            var secondsSinceLastCollect   = (this.timeService.Now - lastCollectTime).TotalSeconds;
            var intervalsSinceLastCollect = (int)(secondsSinceLastCollect / produceIntervalSeconds);

            var rewardAmount = intervalsSinceLastCollect * produceQuantity;

            if (rewardAmount >= storageCapacity) {
                dueTime = this.timeService.Now;
                return storageCapacity;
            }

            dueTime = coinFarmData.LastCollectTime.Value.AddSeconds(intervalsSinceLastCollect * produceIntervalSeconds);
            return rewardAmount;
        }

        public float CalcCurrentProduceRatio(string coinFarmKey) {
            var collectedReward = this.CalcCollectedRewardAmount(coinFarmKey, out var dueTime);
            if (collectedReward >= this.GetCurrentStorageCapacity(coinFarmKey)) {
                return 0f;
            }

            var elapsed = (float)(this.timeService.Now - dueTime).TotalSeconds;
            return elapsed / this.GetCurrentProduceIntervalSeconds(coinFarmKey);
        }

        public int GetCurrentProduceQuantity(string coinFarmKey) {
            var coinFarmDef = this.GetDef(coinFarmKey);
            var index       = Math.Clamp(this.GetCurrentLevel(coinFarmKey) - 1, 0, coinFarmDef.ProduceQuantity.Count);
            return coinFarmDef.ProduceQuantity[index];
        }

        public int GetCurrentProduceIntervalSeconds(string coinFarmKey) {
            var coinFarmDef = this.GetDef(coinFarmKey);
            var index       = Math.Clamp(this.GetCurrentLevel(coinFarmKey) - 1, 0, coinFarmDef.ProduceIntervalSeconds.Count);
            return coinFarmDef.ProduceIntervalSeconds[index];
        }

        public int GetCurrentStorageCapacity(string coinFarmKey) {
            var coinFarmDef = this.GetDef(coinFarmKey);
            var index       = Math.Clamp(this.GetCurrentLevel(coinFarmKey) - 1, 0, coinFarmDef.StorageCapacity.Count);
            return coinFarmDef.StorageCapacity[index];
        }
    }
}